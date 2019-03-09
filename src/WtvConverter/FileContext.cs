
namespace WtvConverter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Serilog;
    using Serilog.Core;
    using VideoTools;

    internal class FileContext : IDisposable
    {
        private readonly AppContext _appContext;
        private readonly string _inputVideoFile;
        private readonly string _inputVideoFileName;
        private readonly string _inputVideoFileExtension;
        private Logger _log;
        private string _outputFileName;
        private DirectoryScope _outputDirectory;
        private IDictionary<string, object> _attributes;
        private JsonPayload _metadata;
        private TvEpisode _episode;

        public FileContext(AppContext appContext, string file)
        {
            _appContext = appContext;

            _inputVideoFile = file;
            _inputVideoFileName = Path.GetFileNameWithoutExtension(file);
            _inputVideoFileExtension = Path.GetExtension(file).ToLowerInvariant();
        }

        public void ProcessFile()
        {
            _log = new LoggerConfiguration()
                .WriteTo.File($"logs\\{DateTime.UtcNow.ToString("yyyyMMdd-HHmm")}-{_inputVideoFileName}.log")
                .WriteTo.Console()
                .CreateLogger();

            switch (_inputVideoFileExtension)
            {
                case ".wtv":
                case ".dvr-ms":
                    ProcessMediaCenterFile();
                    break;
                case ".mkv":
                case ".mp4":
                case ".mpg":
                case ".webm":
                case ".ts":
                    ProcessTvhFile();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// input file => get metadata => (episode) => Opt:((Convert video file),(Get thumbnail),(Set metadata)) => generate NFO file
        /// </summary>
        private void ProcessMediaCenterFile()
        {
            _attributes = GetAttributes();

            _episode = new TvEpisode()
                .Defaults()
                .FromFileName(_inputVideoFileName)
                .FromMediaCenterAttributes(_attributes)
                .FromDescription()
                .Fix();
            _log.Information("Episode={@Episode}", _episode);

            _outputDirectory = GetOutputDirectoryScope();
            _log.Information("Output directory={@OutputDirectory}", _outputDirectory);

            _outputFileName = _episode.GetConversionOutputFileName();
            _log.Information("Output file name={OutputFileName}", _outputFileName);

            using (var outputVideoFile = FileScope.Create(GetOutputFilePath(".mp4"), whatIf: _appContext.WhatIf))
            {
                _ = outputVideoFile.TryCreate(TryCreateOutputVideoFile);
            }

            NfoFileHandler();
        }

        private void ProcessTvhFile()
        {
            _metadata = GetMetadata();

            _episode = new TvEpisode()
                .Defaults()
                .FromFileName(_inputVideoFileName)
                .FromJsonMetadata(_metadata)
                .FromDescription()
                .Fix();
            _log.Information("Episode={@Episode}", _episode);

            _outputDirectory = GetOutputDirectoryScope();
            _log.Information("Output directory={@OutputDirectory}", _outputDirectory);

            _outputFileName = _inputVideoFileName;
            _log.Information("Output file name={OutputFileName}", _outputFileName);

            // Copy the input video file to the output directory
            using (var outputVideoFile = FileScope.Create(GetOutputFilePath(_inputVideoFileExtension), whatIf: _appContext.WhatIf))
            {
                _ = outputVideoFile.TryCreate(TryCopyToOutputVideoFile);
            }

            NfoFileHandler();
        }

        private DirectoryScope GetOutputDirectoryScope()
        {
            var result = DirectoryScope.Create(_appContext.OutputDirectory, whatIf: _appContext.WhatIf)
                .ExpandEnvironmentVariables()
                .FormatWith(_episode)
                .EnsureCreated();

            return result;
        }

        private DirectoryScope GetIntermediateDirectoryScope()
        {
            var result = DirectoryScope.Create(_appContext.IntermediateDirectory, whatIf: _appContext.WhatIf)
                .ExpandEnvironmentVariables()
                .EnsureCreated();

            return result;
        }

        private FileScope GetIntermediateFileScope()
        {
            var directory = GetIntermediateDirectoryScope();
            var path = GetIntermediateFilePath(directory.Path, _inputVideoFileName);
            var result = FileScope.Create(path, deleteOnDispose: _appContext.DeleteIntermediateFile, whatIf: _appContext.WhatIf);

            return result;
        }

        private bool TryCreateOutputVideoFile(FileScope scope, bool whatIf)
        {
            using (var intermediateVideoFile = GetIntermediateFileScope())
            {
                if (intermediateVideoFile.TryCreate(TryConvertToMp4) == false)
                {
                    _log.Information("Skipping file conversion because intermediate file already exists");
                }

                _metadata = GetMetadata();

                using (var thumbnailFile = FileScope.Create(GetOutputFilePath(".jpg"), whatIf: _appContext.WhatIf))
                {
                    if (thumbnailFile.TryCreate(TryCreateThumbnailFile))
                    {
                        _episode.ThumbnailFile = thumbnailFile.Path;
                    }
                }

                _log.Information("Applying media tags and copying from \"{Input}\" to \"{Output}\"", intermediateVideoFile.Path, scope.Path);
                if (whatIf == false)
                {
                    // AtomicParsley sets the ID3 metadata tags but also copies the intermediate file to the output file
                    _appContext.AtomicParsleyCommand.SetMp4FileMetadata(_episode, intermediateVideoFile.Path, scope.Path);

                    _log.Information("Set media tags {@AtomicParsley}", _appContext.AtomicParsleyCommand);
                }
            }

            return true;
        }

        private bool TryCopyToOutputVideoFile(FileScope scope, bool whatIf)
        {
            _log.Information("Copying video file from \"{Input}\" to \"{Output}\"", _inputVideoFile, scope.Path);

            if (whatIf == false)
            {
                bool overwrite = (scope.Policy & FileScopePolicy.Overwrite) == FileScopePolicy.Overwrite;

                File.Copy(_inputVideoFile, scope.Path, overwrite);

                _log.Information("Copied video file from \"{Input}\" to \"{Output}\"", _inputVideoFile, scope.Path);
            }

            return true;
        }

        private bool TryConvertToMp4(FileScope scope, bool whatIf)
        {
            _log.Information("Converting video file from \"{Input}\" to \"{Output}\"", _inputVideoFile, scope.Path);

            if (whatIf == false)
            {
                _appContext.FfmpegCommand.ConvertWtvToMp4File(_inputVideoFile, scope.Path);

                _log.Information("Converted video file {@ffmpeg}", _appContext.FfmpegCommand);
            }

            return true;
        }

        /// <summary>
        /// Writes a Kodi episode NFO file to the destination directory
        /// </summary>
        private void NfoFileHandler()
        {
            if (Helpers.IsFileNameKodiCompatible(_outputFileName) == false)
            {
                using (var nfoFile = FileScope.Create(GetOutputFilePath(".nfo"), whatIf: _appContext.WhatIf))
                {
                    _ = nfoFile.TryCreate((scope, whatIf) =>
                    {
                        _log.Information("Saving episode NFO file to \"{NfoPath}\"", scope.Path);

                        if (whatIf == false)
                        {
                            _episode.SaveNfoFile(scope.Path);
                        }
                        return true;
                    });
                }
            }
        }

        private bool TryCreateThumbnailFile(FileScope scope, bool whatIf)
        {
            bool result = whatIf;       // return true when whatIf is true

            _log.Information("Saving video thumbnail file from \"{Input}\" to \"{Output}\"", _inputVideoFile, scope.Path);

            if (_metadata.TryGetThumbnailStreamIndex(out int streamIndex))
            {
                if (whatIf == false)
                {
                    _appContext.FfmpegCommand.ExtractThumbnailToFile(_inputVideoFile, streamIndex, scope.Path);

                    result = _appContext.FfmpegCommand.ExitCode == 0 && File.Exists(scope.Path);

                    _log.Information("Saved video thumbnail file {@ffmpeg}", _appContext.FfmpegCommand);
                }
            }

            return result;
        }

        private IDictionary<string, object> GetAttributes()
        {
            if (_attributes is null)
            {
                _attributes = WtvAttributeReader.GetAttributes(_inputVideoFile);
                _log.Information("Media attributes {@attributes}", _attributes);
            }

            return _attributes;
        }

        private JsonPayload GetMetadata()
        {
            if (_metadata is null)
            {
                _metadata = RunProbe(_inputVideoFile);
                _log.Information("Media tags {@metadata}", _metadata);
            }

            return _metadata;
        }

        private JsonPayload RunProbe(string source)
        {
            var result = default(JsonPayload);

            _appContext.FfprobeCommand.GetMetadataAsJson(source);

            if (String.IsNullOrEmpty(_appContext.FfprobeCommand.StandardOutput) == false)
            {
                result = new JsonPayload(_appContext.FfprobeCommand.StandardOutput);
            }

            return result;
        }

        private string GetIntermediateFilePath(string intermediateDirectory, string fileName)
        {
            string newFileName = fileName + ".tmp";

            return Path.Combine(intermediateDirectory, newFileName);
        }

        private string GetOutputFilePath(string extension)
        {
            string fileName = _outputFileName + extension;

            return Path.Combine(_outputDirectory.Path, fileName);
        }

        public void Dispose()
        {
            if (_log != null)
            {
                _log.Dispose();
                _log = null;
            }
        }
    }
}

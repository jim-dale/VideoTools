
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
        private string _tempDirectory;
        private string _outputDirectory;
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

            _attributes = GetAttributes();
            _log.Information("Media attributes:{@attributes}", _attributes);

            _metadata = GetMetadata();
            _log.Information("Media tags:{@metadata}", _metadata.Text);

            _episode = new TvEpisode()
                .Defaults()
                .FromFileName(_inputVideoFileName)
                .FromJsonMetadata(_metadata)
                .FromMediaCenterAttributes(_attributes)
                .FromDescription()
                .FixTitleAndShowName()
                .SetSafeShowName().Log(_log, "Episode");

            _outputFileName = _episode.GetOutputFileName();
            _log.Information("Output file name:\"{OutputFileName}\"", _outputFileName);

            var outputFile = GetOutputFile(".mp4");
            _log.Information("Output file:\"{OutputFile}\"", outputFile);

            if (File.Exists(outputFile) == false)
            {
                var tempFile = GetTemporaryFile();
                _log.Information("Temporary file:\"{TempFile}\"", tempFile);

                bool tempFileCreated = false;
                bool tempFileExists = File.Exists(tempFile);

                if (tempFileExists == false)
                {
                    tempFileCreated = tempFileExists = _appContext.WhatIf;

                    if (_appContext.WhatIf == false)
                    {
                        _appContext.FfmpegCommand.ConvertWtvToMp4File(_inputVideoFile, tempFile);
                        _log.Information("ConvertWtvToMp4File:{@ffmpeg}", _appContext.FfmpegCommand);

                        tempFileCreated = tempFileExists = _appContext.FfmpegCommand.ExitCode == 0 && File.Exists(tempFile);
                    }
                }
                else
                {
                    _log.Information("Not converting input file because the converted file already exists");
                }

                if (tempFileExists)
                {
                    var thumbnailFile = GetOutputFile(".jpg");
                    if (TryCreateThumbnailFile(thumbnailFile))
                    {
                        _episode.ThumbnailFile = thumbnailFile;
                    }

                    if (_appContext.WhatIf == false)
                    {
                        _appContext.AtomicParsleyCommand.SetMp4FileMetadata(_episode, tempFile, outputFile);
                        _log.Information("SetMp4FileMetadata:{@AtomicParsley}", _appContext.AtomicParsleyCommand);
                    }

                    NfoFileHandler();

                    if (_appContext.DeleteTempFile && tempFileCreated)
                    {
                        if (_appContext.WhatIf == false)
                        {
                            _log.Information("Deleting temporary file:\"{TempFile}\"", tempFile);
                            File.Delete(tempFile);
                        }
                    }
                }
            }
            else
            {
                _log.Information("Not processing input file because the output file already exists");
            }
        }

        /// <summary>
        /// Writes a Kodi episode NFO file to the destination directory unless the output video file name is Kodi compatible
        /// </summary>
        private void NfoFileHandler()
        {
            if (Helpers.IsFileNameKodiCompatible(_outputFileName))
            {
                _log.Information("Not generating Kodi TV episode NFO file because output file name is compatible with Kodi");
            }
            else
            {
                _log.Information("Saving episode:{@Episode}", _episode);

                var episodeNfoFile = GetOutputFile(".nfo");
                _log.Information("Saving episode NFO file to \"{EpisodeNfoPath}\"", episodeNfoFile);

                if (_appContext.WhatIf == false)
                {
                    _episode.SaveNfoFile(episodeNfoFile);
                }
            }
        }

        private bool TryCreateThumbnailFile(string path)
        {
            bool result = _appContext.WhatIf;

            _log.Information("Extracting thumbnail from \"{Input}\" to \"{Output}\"", _inputVideoFile, path);

            if (_appContext.WhatIf == false)
            {
                _appContext.FfmpegCommand.ExtractThumbnailToFile(_inputVideoFile, path);
                _log.Information("ExtractThumbnailToFile:{@ffmpeg}", _appContext.FfmpegCommand);

                result = _appContext.FfmpegCommand.ExitCode == 0 && File.Exists(path);
            }

            return result;
        }

        private IDictionary<string, object> GetAttributes()
        {
            if (_attributes is null)
            {
                _attributes = WtvAttributeReader.GetAttributes(_inputVideoFile);
            }

            return _attributes;
        }

        private JsonPayload GetMetadata()
        {
            if (_metadata is null)
            {
                _appContext.FfprobeCommand.GetMetadataAsJson(_inputVideoFile);

                if (String.IsNullOrEmpty(_appContext.FfprobeCommand.StandardOutput) == false)
                {
                    _metadata = new JsonPayload(_appContext.FfprobeCommand.StandardOutput);
                }
            }

            return _metadata;
        }

        private string GetTemporaryFile()
        {
            var fileName = _inputVideoFileName + ".mp4";

            return Path.Combine(GetTempDirectory(), fileName);
        }

        private string GetTempDirectory()
        {
            if (_tempDirectory is null)
            {
                _tempDirectory = _appContext.TempDirectory;

                if (_appContext.WhatIf == false)
                {
                    if (Directory.Exists(_tempDirectory) == false)
                    {
                        Directory.CreateDirectory(_tempDirectory);
                    }
                }
            }

            return _tempDirectory;
        }

        private string GetOutputFile(string extension)
        {
            var fileName = _outputFileName + extension;

            return Path.Combine(GetOutputDirectory(), fileName);
        }

        private string GetOutputDirectory()
        {
            if (_outputDirectory is null)
            {
                _outputDirectory = _appContext.OutputDirectory.FormatWith(_episode);

                if (_appContext.WhatIf == false)
                {
                    if (Directory.Exists(_outputDirectory) == false)
                    {
                        Directory.CreateDirectory(_outputDirectory);
                    }
                }
            }

            return _outputDirectory;
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

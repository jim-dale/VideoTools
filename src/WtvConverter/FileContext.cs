
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
        private readonly string _file;
        private readonly string _fileName;
        private readonly string _extension;
        private Logger _log;
        private string _outputFileName;
        private IDictionary<string, object> _attributes;
        private JsonPayload _metadata;
        private TvEpisode _episode;

        public FileContext(AppContext appContext, string file)
        {
            _appContext = appContext;

            _file = file;
            _fileName = Path.GetFileNameWithoutExtension(file);
            _extension = Path.GetExtension(file).ToLowerInvariant();
        }

        public void ProcessFile()
        {
            _log = new LoggerConfiguration()
                .WriteTo.File($"logs\\{DateTime.UtcNow.ToString("yyyyMMdd-HHmm")}-{_fileName}.log")
                .WriteTo.Console()
                .CreateLogger();

            if (Helpers.IsFileNameKodiCompatible(_fileName))
            {
                _log.Information("Ignoring current file because the file name is compatible with Kodi");
            }
            else
            {
                switch (_extension)
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
        }

        /// <summary>
        /// input file => get metadata => (show, episode) => Opt:((Convert video file),(Get thumbnail),(Set metadata)) => generate NFO file
        /// </summary>
        private void ProcessMediaCenterFile()
        {
            _attributes = GetAttributes();

            _episode = new TvEpisode()
                .Defaults()
                .FromMediaCenterAttributes(_attributes);

            _outputFileName = _episode.GetConversionOutputFileName();

            var outputFilePath = Helpers.GetOutputFilePath(_appContext.TargetDirectory, _outputFileName);
            using (var outputVideoFile = FileScope.Create(outputFilePath, whatIf: _appContext.WhatIf))
            {
                _ = outputVideoFile.TryCreate(TryCreateOutputVideoFile);
            }

            var nfoFilePath = Helpers.GetNfoFilePath(_appContext.TargetDirectory, _outputFileName);
            using (var nfoFile = FileScope.Create(nfoFilePath, whatIf: _appContext.WhatIf))
            {
                _ = nfoFile.TryCreate(TrySaveNfoFile);
            }
        }

        private void ProcessTvhFile()
        {
            _metadata = GetMetadata();

            _episode = new TvEpisode()
                .Defaults()
                .FromFileName(_fileName)
                .FromJsonMetadata(_metadata)
                .FromDescription()
                .Fix();

            _outputFileName = _fileName;

            var nfoFilePath = Helpers.GetNfoFilePath(_appContext.TargetDirectory, _outputFileName);
            using (var nfoFile = FileScope.Create(nfoFilePath, whatIf: _appContext.WhatIf))
            {
                _ = nfoFile.TryCreate(TrySaveNfoFile);
            }
        }

        private bool TryCreateOutputVideoFile(FileScope scope, bool whatIf)
        {
            var intermediateFilePath = Helpers.GetIntermediateFilePath(_appContext.IntermediateDirectory, _outputFileName);

            using (var intermediateVideoFile = FileScope.Create(intermediateFilePath, deleteOnDispose: _appContext.DeleteIntermediateFile, whatIf: _appContext.WhatIf))
            {
                if (intermediateVideoFile.TryCreate(TryConvertToMp4) == false)
                {
                    _log.Information("Skipping file conversion because intermediate file already exists");
                }

                _metadata = GetMetadata();

                var thunbnailFilePath = Helpers.GetThumbnailFilePath(_appContext.TargetDirectory, _outputFileName);
                using (var thumbnailFile = FileScope.Create(thunbnailFilePath, whatIf: _appContext.WhatIf))
                {
                    if (thumbnailFile.TryCreate(TryCreateThumbnailFile))
                    {
                        _episode.ThumbnailFile = thunbnailFilePath;
                    }
                }
                _appContext.AtomicParsleyCommand.SetMp4FileMetadata(_episode, intermediateVideoFile.Path, scope.Path);
                _log.Information("{@AtomicParsley}", _appContext.AtomicParsleyCommand);
            }

            return true;
        }

        private bool TryConvertToMp4(FileScope scope, bool whatIf)
        {
            _log.Information("Converting {input} to {output}", _file, scope.Path);

            if (whatIf == false)
            {
                _appContext.FfmpegCommand.ConvertWtvToMp4File(_file, scope.Path);

                _log.Information("{@ffmpeg}", _appContext.FfmpegCommand);
            }

            return true;
        }

        private bool TrySaveNfoFile(FileScope scope, bool whatIf)
        {
            _log.Information("Saving episode NFO file {@Episode}", _episode);

            if (whatIf == false)
            {
                _episode.SaveNfoFile(scope.Path);
            }

            return true;
        }

        private bool TryCreateThumbnailFile(FileScope scope, bool whatIf)
        {
            bool result = false;

            if (_metadata.TryGetThumbnailStreamIndex(out int streamIndex))
            {
                _appContext.FfmpegCommand.ExtractThumbnailToFile(_file, streamIndex, scope.Path);
                _log.Information("{@ffmpeg}", _appContext.FfmpegCommand);

                result = _appContext.FfmpegCommand.ExitCode == 0 && File.Exists(scope.Path);
            }

            return result;
        }

        private IDictionary<string, object> GetAttributes()
        {
            if (_attributes is null)
            {
                _attributes = WtvAttributeReader.GetAttributes(_file);
                _log.Information("{@attributes}", _attributes);
            }

            return _attributes;
        }

        private JsonPayload GetMetadata()
        {
            if (_metadata is null)
            {
                _metadata = RunProbe(_file);
                _log.Information("{@metadata}", _metadata);
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


//result.Programme = (string)attributes["Title"];

//long encodeTime = (long)attributes["WM/WMRVEncodeTime"];
//result.AiredTime = new DateTime(encodeTime, DateTimeKind.Utc).ToLocalTime();
//result.OriginalAirDate = DateTime.Parse((string)attributes["WM/MediaOriginalBroadcastDateTime"]).ToLocalTime();
//string tempSubTitle = (string)attributes["WM/SubTitle"];
//string subTitle = result.AiredTime.ToString("t");
//string sortSubTitle = result.AiredTime.ToString("HH'-'mm");
//if (string.IsNullOrWhiteSpace(tempSubTitle) == false)
//{
//    subTitle = subTitle + " " + tempSubTitle;
//    sortSubTitle = sortSubTitle + " " + tempSubTitle;
//}
//string airedTime = result.AiredTime.ToString("d");
//string sortAiredDate = result.AiredTime.ToString("yyyy'-'MM'-'dd");
//result.Title = string.Format("{0} {1} {2}", result.Programme, sortAiredDate, sortSubTitle);
//result.SortTitle = string.Format("{0} {1} {2}", result.Programme, sortAiredDate, sortSubTitle);
//result.Description = (string)attributes["WM/SubTitleDescription"];
//result.Channel = (string)attributes["WM/MediaStationCallSign"];
//result.Credits = (string)attributes["WM/MediaCredits"];

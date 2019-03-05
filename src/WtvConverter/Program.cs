using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;
using VideoTools;

namespace WtvConverter
{
    internal class FileContext
    {
        private AppContext _appContext;
        private readonly string _file;
        private readonly string _fileName;
        private readonly string _extension;
        private string _outputFileName;
        private ILogger _log;
        private IDictionary<string, object> _attributes;
        private JsonPayload _metadata;

        public FileContext(AppContext appContext, string file)
        {
            _appContext = appContext;

            _file = file;
            _fileName = Path.GetFileNameWithoutExtension(file);
            _extension = Path.GetExtension(file);
        }

        public void ProcessFile()
        {
            _log = new LoggerConfiguration()
                .WriteTo.File($"logs\\{DateTime.UtcNow.ToString("yyyyMMdd-HHmm")}-{_fileName}.log")
                .WriteTo.Console()
                .CreateLogger();

            using (_log.BeginTimedOperation("Processing file", _file))
            {
                if (Helpers.IsFileNameKodiCompatible(_file))
                {

                }
            }
        }

        /// <summary>
        /// input file => get metadata => (show, episode) => Opt:((Convert video file),(Get thumbnail),(Set metadata)) => generate NFO file
        /// </summary>
        public void ProcessTvhFile()
        {
            var attributes = GetAttributes();

            var episode = new TvEpisode()
                .Defaults()
                .FromMediaCenterAttributes(attributes);

            _outputFileName = episode.GetOutputFileName();
            var outputVideoFile = Helpers.GetOutputFilePath(_appContext.TargetDirectory, _outputFileName);

            if (File.Exists(outputVideoFile) == false)
            {
                using (var ivf = new FileScope(Helpers.GetIntermediateFilePath(_appContext.IntermediateDirectory, _outputFileName)))
                {
                    if (ivf.IfNotExist(ConvertToMp4))
                    {
                        ivf.DeleteOnDispose = _appContext.DeleteIntermediateFile;
                    }
                    else
                    {
                        _log.Information("Skipping file conversion because intermediate file already exists");
                    }

                    var metadata = GetMetadata();

                    if (TryCreateThumbnailFile(metadata, episode, out string thumbnailFile))
                    {
                        episode.ThumbnailFile = Path.GetFileName(thumbnailFile);
                    }

                    _appContext.AtomicParsleyCommand.SetMp4FileMetadata(episode, ivf.Path, thumbnailFile, outputVideoFile);
                    _log.Information("{@AtomicParsley}", _appContext.AtomicParsleyCommand);
                }
            }
            else
            {
                _log.Information("Skipping processing because the output file already exists");
            }

            var nfoFile = Helpers.GetNfoFilePath(_appContext.TargetDirectory, _outputFileName);
            episode.SaveNfoFile(nfoFile);
        }

        private void ProcessMediaCenterFile()
        {
            var metadata = GetMetadata();

            var episode = new TvEpisode()
                .Defaults()
                .FromFileName(_file)
                .FromJsonMetadata(metadata)
                .FromDescription()
                .Fix();
        }

        private void ConvertToMp4(string path)
        {
            _appContext.FfmpegCommand.ConvertWtvToMp4File(_file, path);
            _log.Information("{@ffmpeg}", _appContext.FfmpegCommand);
        }

        private bool TryCreateThumbnailFile(JsonPayload metadata, TvEpisode episode, out string result)
        {
            bool success = false;
            result = null;

            var thumbnailFile = Helpers.GetThumbnailFilePath(_appContext.TargetDirectory, _outputFileName);

            if (metadata.TryGetThumbnailStreamIndex(out int streamIndex))
            {
                _appContext.FfmpegCommand.ExtractThumbnailToFile(_file, streamIndex, result);
                _log.Information("{@ffmpeg}", _appContext.FfmpegCommand);

                if (_appContext.FfmpegCommand.ExitCode == 0 && File.Exists(thumbnailFile))
                {
                    result = thumbnailFile;
                }
            }

            return success;
        }

        public JsonPayload RunProbe(string source)
        {
            var result = default(JsonPayload);

            _appContext.FfprobeCommand.GetMetadataAsJson(source);
            _log.Information("{@ffprobe}", _appContext.FfprobeCommand);

            if (string.IsNullOrEmpty(_appContext.FfprobeCommand.StandardOutput) == false)
            {
                result = new JsonPayload(_appContext.FfprobeCommand.StandardOutput);
            }

            return result;
        }

        public IDictionary<string, object> GetAttributes()
        {
            if (_attributes is null)
            {
                _attributes = WtvAttributeReader.GetAttributes(_file);
                _log.Information("{@attributes}", _attributes);
            }

            return _attributes;
        }

        public JsonPayload GetMetadata()
        {
            if (_metadata is null)
            {
                _metadata = RunProbe(_file);
                _log.Information("{@ffprobe}", _appContext.FfprobeCommand);
            }

            return _metadata;
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("logs\\{Date}-wtvconverter.log")
                .WriteTo.Console()
                .CreateLogger();

            var appContext = new AppContext()
                .SetDefaults()
                .SetFromConfiguration(ConfigurationManager.AppSettings)
                .Build();

            Log.Information("{@AppContext}", appContext);

            ProcessSource(appContext);

            Log.CloseAndFlush();
        }

        private static void ProcessSource(AppContext appContext)
        {
            var patterns = appContext.SearchPatterns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var matcher = new Matcher();
            matcher.AddIncludePatterns(patterns);

            var files = matcher.GetResultsInFullPath(appContext.SourcePath);

            foreach (var file in files)
            {
                try
                {
                    var fileContext = new FileContext(appContext, file);

                    fileContext.ProcessFile();
                }
                catch (Exception exception)
                {
                    Log.Error(exception, String.Empty);
                }
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

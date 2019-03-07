using System;
using System.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;

namespace WtvConverter
{
    // converted intermediate video file
    // output
    internal class Program
    {
        private static int Main(string[] args)
        {
            int result = 0;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("logs\\{Date}-wtvconverter.log")
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var appContext = new AppContext()
                    .SetDefaults()
                    .SetFromConfiguration(ConfigurationManager.AppSettings)
                    .Build();

                Log.Information("{@AppContext}", appContext);

                ProcessInputDirectory(appContext);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly");
                result = -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return result;
        }

        private static void ProcessInputDirectory(AppContext appContext)
        {
            var patterns = appContext.SearchPatterns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var matcher = new Matcher();
            matcher.AddIncludePatterns(patterns);

            var files = matcher.GetResultsInFullPath(appContext.InputDirectory);

            foreach (var file in files)
            {
                try
                {
                    using (Log.Logger.BeginTimedOperation("Processing file", file))
                    {
                        var fileContext = new FileContext(appContext, file);

                        fileContext.ProcessFile();
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Failed to process {Path}", file);
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

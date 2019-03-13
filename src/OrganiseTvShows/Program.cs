using System;
using System.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;

namespace WtvConverter
{
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
                    .Initialise();

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

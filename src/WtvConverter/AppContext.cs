
namespace WtvConverter
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using VideoTools;

    public class AppContext
    {
        public string FfmpegPath { get; set; }
        public string FfprobePath { get; set; }
        public string AtomicParsleyPath { get; set; }
        public string SourcePath { get; set; }
        public string IntermediateDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string SearchPatterns { get; set; }
        public bool DeleteIntermediateFile { get; set; }

        public ConsoleClient FfmpegCommand { get; private set; }
        public ConsoleClient FfprobeCommand { get; private set; }
        public ConsoleClient AtomicParsleyCommand { get; private set; }

        public AppContext SetDefaults()
        {
            IntermediateDirectory = Path.GetTempPath();
            SearchPatterns = @"*.wtv";

            return this;
        }

        public AppContext SetFromConfiguration(NameValueCollection settings)
        {
            AtomicParsleyPath = GetSetting(settings, AtomicParsleyPath, nameof(AtomicParsleyPath));
            FfmpegPath = GetSetting(settings, FfmpegPath, nameof(FfmpegPath));
            FfprobePath = GetSetting(settings, FfprobePath, nameof(FfprobePath));

            SourcePath = GetSetting(settings, SourcePath, nameof(SourcePath));
            IntermediateDirectory = GetSetting(settings, IntermediateDirectory, nameof(IntermediateDirectory));
            TargetDirectory = GetSetting(settings, TargetDirectory, nameof(TargetDirectory));

            SearchPatterns = GetSetting(settings, SearchPatterns, nameof(SearchPatterns));

            DeleteIntermediateFile = GetSetting(settings, DeleteIntermediateFile, nameof(DeleteIntermediateFile));

            return this;
        }

        public void Initialise()
        {
            FfmpegCommand = new ConsoleClient(FfmpegPath);
            FfprobeCommand = new ConsoleClient(FfprobePath);
            AtomicParsleyCommand = new ConsoleClient(AtomicParsleyPath);
        }

        private T GetSetting<T>(NameValueCollection settings, T defaultValue, string propertyName)
        {
            T result = defaultValue;

            string[] values = settings.GetValues(propertyName);
            if (values != null && values.Length > 0)
            {
                if (typeof(T).IsArray)
                {
                    result = (T)Convert.ChangeType(values, typeof(T));
                }
                else
                {
                    result = (T)Convert.ChangeType(values[0], typeof(T));
                }
            }
            return result;
        }

        public AppContext Build()
        {
            AtomicParsleyPath = Environment.ExpandEnvironmentVariables(AtomicParsleyPath);
            FfmpegPath = Environment.ExpandEnvironmentVariables(FfmpegPath);
            FfprobePath = Environment.ExpandEnvironmentVariables(FfprobePath);
            SourcePath = Environment.ExpandEnvironmentVariables(SourcePath);
            IntermediateDirectory = Environment.ExpandEnvironmentVariables(IntermediateDirectory);
            TargetDirectory = Environment.ExpandEnvironmentVariables(TargetDirectory);

            return this;
        }
    }
}

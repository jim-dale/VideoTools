
namespace WtvConverter
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using VideoTools;

    public class AppContext
    {
        public string Ffmpeg { get; set; }
        public string Ffprobe { get; set; }
        public string AtomicParsley { get; set; }
        public string InputDirectory { get; set; }
        public string IntermediateDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string SearchPatterns { get; set; }
        public bool DeleteIntermediateFile { get; set; }
        public bool WhatIf { get; set; }

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
            AtomicParsley = settings.GetAs(AtomicParsley, nameof(AtomicParsley));
            Ffmpeg = settings.GetAs(Ffmpeg, nameof(Ffmpeg));
            Ffprobe = settings.GetAs(Ffprobe, nameof(Ffprobe));

            InputDirectory = settings.GetAs(InputDirectory, nameof(InputDirectory));
            IntermediateDirectory = settings.GetAs(IntermediateDirectory, nameof(IntermediateDirectory));
            TargetDirectory = settings.GetAs(TargetDirectory, nameof(TargetDirectory));

            SearchPatterns = settings.GetAs(SearchPatterns, nameof(SearchPatterns));

            DeleteIntermediateFile = settings.GetAs(DeleteIntermediateFile, nameof(DeleteIntermediateFile));

            WhatIf = settings.GetAs(WhatIf, nameof(WhatIf));

            return this;
        }

        public AppContext Build()
        {
            AtomicParsley = Environment.ExpandEnvironmentVariables(AtomicParsley);
            Ffmpeg = Environment.ExpandEnvironmentVariables(Ffmpeg);
            Ffprobe = Environment.ExpandEnvironmentVariables(Ffprobe);
            InputDirectory = Environment.ExpandEnvironmentVariables(InputDirectory);
            IntermediateDirectory = Environment.ExpandEnvironmentVariables(IntermediateDirectory);
            TargetDirectory = Environment.ExpandEnvironmentVariables(TargetDirectory);

            AtomicParsleyCommand = new ConsoleClient(AtomicParsley);
            FfmpegCommand = new ConsoleClient(Ffmpeg);
            FfprobeCommand = new ConsoleClient(Ffprobe);

            return this;
        }
    }
}

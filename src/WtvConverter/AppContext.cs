
namespace WtvConverter
{
    using System;
    using System.Collections.Specialized;
    using VideoTools;

    public class AppContext
    {
        public string AtomicParsley { get; set; }
        public string Ffmpeg { get; set; }
        public string Ffprobe { get; set; }
        public string SearchPatterns { get; set; }
        public string InputDirectory { get; set; }
        public string TempDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public bool DeleteTempFile { get; set; }
        public bool WhatIf { get; set; }

        public ConsoleClient AtomicParsleyCommand { get; private set; }
        public ConsoleClient FfmpegCommand { get; private set; }
        public ConsoleClient FfprobeCommand { get; private set; }

        public AppContext SetDefaults()
        {
            SearchPatterns = String.Empty;

            return this;
        }

        public AppContext SetFromConfiguration(NameValueCollection settings)
        {
            AtomicParsley = settings.GetAs(AtomicParsley, nameof(AtomicParsley));
            Ffmpeg = settings.GetAs(Ffmpeg, nameof(Ffmpeg));
            Ffprobe = settings.GetAs(Ffprobe, nameof(Ffprobe));

            SearchPatterns = settings.GetAs(SearchPatterns, nameof(SearchPatterns));
            InputDirectory = settings.GetAs(InputDirectory, nameof(InputDirectory));
            TempDirectory = settings.GetAs(TempDirectory, nameof(TempDirectory));
            OutputDirectory = settings.GetAs(OutputDirectory, nameof(OutputDirectory));
            DeleteTempFile = settings.GetAs(DeleteTempFile, nameof(DeleteTempFile));
            WhatIf = settings.GetAs(WhatIf, nameof(WhatIf));

            return this;
        }

        public AppContext Initialise()
        {
            AtomicParsley = Environment.ExpandEnvironmentVariables(AtomicParsley);
            Ffmpeg = Environment.ExpandEnvironmentVariables(Ffmpeg);
            Ffprobe = Environment.ExpandEnvironmentVariables(Ffprobe);
            InputDirectory = Environment.ExpandEnvironmentVariables(InputDirectory);
            TempDirectory = Environment.ExpandEnvironmentVariables(TempDirectory);
            OutputDirectory = Environment.ExpandEnvironmentVariables(OutputDirectory);

            AtomicParsleyCommand = new ConsoleClient(AtomicParsley);
            FfmpegCommand = new ConsoleClient(Ffmpeg);
            FfprobeCommand = new ConsoleClient(Ffprobe);

            return this;
        }
    }
}

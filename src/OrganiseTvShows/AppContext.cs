
namespace OrganiseTvShows
{
    using System;
    using System.Collections.Specialized;
    using VideoTools;

    public class AppContext
    {
        public string Ffmpeg { get; set; }
        public string Ffprobe { get; set; }
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string SearchPatterns { get; set; }
        public string ShowNameMapFile { get; set; }
        public bool WhatIf { get; set; }

        public ConsoleClient FfmpegCommand { get; private set; }
        public ConsoleClient FfprobeCommand { get; private set; }

        public ShowNameMapEntry[] ShowNameMap { get; private set; }

        public AppContext SetDefaults()
        {
            SearchPatterns = String.Empty;

            return this;
        }

        public AppContext SetFromConfiguration(NameValueCollection settings)
        {
            Ffmpeg = settings.GetAs(Ffmpeg, nameof(Ffmpeg));
            Ffprobe = settings.GetAs(Ffprobe, nameof(Ffprobe));

            InputDirectory = settings.GetAs(InputDirectory, nameof(InputDirectory));
            OutputDirectory = settings.GetAs(OutputDirectory, nameof(OutputDirectory));
            SearchPatterns = settings.GetAs(SearchPatterns, nameof(SearchPatterns));
            ShowNameMapFile = settings.GetAs(ShowNameMapFile, nameof(ShowNameMapFile));

            WhatIf = settings.GetAs(WhatIf, nameof(WhatIf));

            return this;
        }

        public AppContext Initialise()
        {
            Ffmpeg = Environment.ExpandEnvironmentVariables(Ffmpeg);
            Ffprobe = Environment.ExpandEnvironmentVariables(Ffprobe);
            InputDirectory = Environment.ExpandEnvironmentVariables(InputDirectory);
            OutputDirectory = Environment.ExpandEnvironmentVariables(OutputDirectory);
            ShowNameMapFile = Environment.ExpandEnvironmentVariables(ShowNameMapFile);

            FfmpegCommand = new ConsoleClient(Ffmpeg);
            FfprobeCommand = new ConsoleClient(Ffprobe);

            ShowNameMap = ShowNameMapEntry.LoadFromFile(ShowNameMapFile, optional: true);

            return this;
        }
    }
}

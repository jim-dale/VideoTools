
namespace WtvConverter
{
    using System;
    using System.Collections.Generic;
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
        public string OutputDirectory { get; set; }
        public string SearchPatterns { get; set; }
        public string ShowNameCorrectionsFile { get; set; }
        public bool DeleteIntermediateFile { get; set; }
        public bool WhatIf { get; set; }

        public ConsoleClient FfmpegCommand { get; private set; }
        public ConsoleClient FfprobeCommand { get; private set; }
        public ConsoleClient AtomicParsleyCommand { get; private set; }

        public IList<ShowNameCorrection> Corrections { get; private set; }

        public AppContext SetDefaults()
        {
            IntermediateDirectory = Path.GetTempPath();
            SearchPatterns = "*.wtv";

            return this;
        }

        public AppContext SetFromConfiguration(NameValueCollection settings)
        {
            AtomicParsley = settings.GetAs(AtomicParsley, nameof(AtomicParsley));
            Ffmpeg = settings.GetAs(Ffmpeg, nameof(Ffmpeg));
            Ffprobe = settings.GetAs(Ffprobe, nameof(Ffprobe));

            InputDirectory = settings.GetAs(InputDirectory, nameof(InputDirectory));
            IntermediateDirectory = settings.GetAs(IntermediateDirectory, nameof(IntermediateDirectory));
            OutputDirectory = settings.GetAs(OutputDirectory, nameof(OutputDirectory));
            SearchPatterns = settings.GetAs(SearchPatterns, nameof(SearchPatterns));
            ShowNameCorrectionsFile = settings.GetAs(ShowNameCorrectionsFile, nameof(ShowNameCorrectionsFile));

            DeleteIntermediateFile = settings.GetAs(DeleteIntermediateFile, nameof(DeleteIntermediateFile));

            WhatIf = settings.GetAs(WhatIf, nameof(WhatIf));

            return this;
        }

        public AppContext Initialise()
        {
            AtomicParsley = Environment.ExpandEnvironmentVariables(AtomicParsley);
            Ffmpeg = Environment.ExpandEnvironmentVariables(Ffmpeg);
            Ffprobe = Environment.ExpandEnvironmentVariables(Ffprobe);
            InputDirectory = Environment.ExpandEnvironmentVariables(InputDirectory);
            ShowNameCorrectionsFile = Environment.ExpandEnvironmentVariables(ShowNameCorrectionsFile);

            AtomicParsleyCommand = new ConsoleClient(AtomicParsley);
            FfmpegCommand = new ConsoleClient(Ffmpeg);
            FfprobeCommand = new ConsoleClient(Ffprobe);

            Corrections = ShowNameCorrection.LoadFromFile(ShowNameCorrectionsFile);

            return this;
        }
    }
}

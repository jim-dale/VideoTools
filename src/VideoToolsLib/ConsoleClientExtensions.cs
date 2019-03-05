
namespace VideoTools
{
    using System;

    public static partial class ConsoleClientExtensions
    {
        public static void GetMetadataAsJson(this ConsoleClient item, string source)
        {
            var args = new string[]
            {
                "-hide_banner",
                "-v quiet",
                "-print_format json",
                "-show_format",
                "-show_streams",
                $"\"{source}\"",
            };

            item.Run(args);
        }

        public static void ConvertWtvToMp4File(this ConsoleClient item, string input, string output)
        {
            var args = new string[]
            {
                "-hide_banner",         // Hide ffmpeg banner output
                "-v error",             // Suppress all but errors
                "-nostats",             // Don't display statistics
                $"-i \"{input}\"",      // Input video
                "-vf yadif",            // Deinterlace the input video
                "-y",                   // Overwrite the output file if it exists
                $"\"{output}\""         // Output video file
            };

            item.Run(args);
        }

        public static void ExtractThumbnailToFile(this ConsoleClient item, string source, int streamIndex, string target)
        {
            var args = new string[]
            {
                "-hide_banner",
                "-v quiet",
                $"-i \"{source}\"",
                $"-map :{streamIndex}",
                "-vframes 1",
                "-y",
                $"\"{target}\""
            };

            item.Run(args);
        }

        public static void SetMp4FileMetadata(this ConsoleClient item, TvEpisode episode, string source, string thumb, string target)
        {
            const int MaxDescriptionLength = 255;

            var description = episode.Description.Truncate(MaxDescriptionLength, trimSpaces: true);
            var seasonNumArg = (episode.SeasonNumber == Helpers.InvalidSeasonNumber) ? String.Empty : $"--TVSeasonNum {episode.SeasonNumber}";
            var episodeNumArg = (episode.EpisodeNumber == Helpers.InvalidEpisodeNumber) ? String.Empty : $"--TVEpisodeNum {episode.EpisodeNumber}";
            var artworkArg = (thumb == null) ? String.Empty : $"--artwork \"{thumb}\"";
            var aired = episode.AiredTime.ToString(Helpers.UtcDateTimeFormat);

            var args = new string[]
            {
                $"\"{source}\"",
                $"--title \"{episode.Title}\"",
                "--stik \"TV Show\"",
                $"--year \"{aired}\"",
                $"--genre \"{episode.Genre}\"",
                $"--TVNetwork \"{episode.Channel}\"",
                $"--TVShowName \"{episode.ShowName}\"",
                seasonNumArg,
                episodeNumArg,
                $"--description \"{description}\"",
                $"--comment \"{episode.Credits}\"",
                artworkArg,
                $"--output \"{target}\""
            };

            item.Run(args);
        }
    }
}

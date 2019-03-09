
namespace VideoTools
{
    using System;

    public static partial class ConsoleClientExtensions
    {
        //ffmpeg -hide_banner -v quiet -i "D:\Recorded TV\20th Century Battlefields_Yesterday_26_09_2009_15_00_03.dvr-ms" -map 0:v -map -V thumbnail2.jpg
        public static void GetMetadataAsJson(this ConsoleClient item, string source)
        {
            var args = new string[]
            {
                "-hide_banner",
                "-v quiet",
                "-print_format json",
                "-show_format",
                "-show_streams",
                "-show_programs",
                $"\"{source}\"",
            };

            item.Run(args);
        }

        
        public static void ConvertTransportStreamToMp4File(this ConsoleClient item, string input, string output)
        {
            var args = new string[]
            {
                "-hide_banner",         // Hide ffmpeg banner output
                "-v error",             // Suppress all but errors
                "-nostats",             // Don't display statistics
                $"-i \"{input}\"",      // Input video
                "-vf yadif",            // Deinterlace the input video
                "-map 0:V",             // include all video streams that are not attached pictures, video thumbnails or cover art
                "-map 0:a",             // include all audio streams
                "-map 0:s?",             // include all audio streams
                "-vcodec h264",         // encode video streams as H264
                "-acodec aac",          // encode audio streams as AAC
                "-scodec dvbsub",       // encode subtitle streams as dvbsub
                "-y",                   // Overwrite the output file if it exists
                $"\"{output}\""         // Output video file
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
        // ffmpeg -hide_banner -i "20th Century Battlefields_Yesterday_10_08_2009_14_56_04.dvr-ms" -map 0:m:title:"TV Thumbnail" -vframes 1 poo.jpg
        //ffmpeg -hide_banner -i "A Great British Story_BBC ONE_2012_06_12_22_30_00.wtv" -map 0:m:title:"TV Thumbnail" -vframes 1 poo.jpg
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

        public static void SetMp4FileMetadata(this ConsoleClient item, TvEpisode episode, string source, string target)
        {
            var seasonNumArg = (episode.SeasonNumber == Helpers.InvalidSeasonNumber) ? String.Empty : $"--TVSeasonNum {episode.SeasonNumber}";
            var episodeNumArg = (episode.EpisodeNumber == Helpers.InvalidEpisodeNumber) ? String.Empty : $"--TVEpisodeNum {episode.EpisodeNumber}";
            var artworkArg = (episode.ThumbnailFile == null) ? String.Empty : $"--artwork \"{episode.ThumbnailFile}\"";
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
                $"--description \"{episode.Description}\"",
                $"--comment \"{episode.Credits}\"",
                artworkArg,
                $"--output \"{target}\""
            };

            item.Run(args);
        }
    }
}

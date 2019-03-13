
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
                "-show_programs",
                $"\"{source}\"",
            };

            item.Run(args);
        }

        public static void ConvertTransportStreamToMkvFile(this ConsoleClient item, string input, string output)
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
                "-map 0:s?",            // include all subtitle streams (optional)
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
                //"-map 0:V",             // include all video streams that are not attached pictures, video thumbnails or cover art
                //"-map 0:a",             // include all audio streams
                //"-vcodec h264",         // encode video streams as H264
                //"-acodec aac",          // encode audio streams as AAC
                "-y",                   // Overwrite the output file if it exists
                $"\"{output}\""         // Output video file
            };

            item.Run(args);
        }

        /// <summary>
        /// Extract thumbnail image from video file
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input">Video file to try to extract thumbnail from</param>
        /// <param name="output">Destination path for the thumbnail</param>
        /// <remarks>
        /// There are a number of ways of extracting a static thumbnail from a video file including the following:-
        ///     This requires that the thumbnail video track has a metadata tag 'title' with the value 'TV Thumbnail'
        ///<code>
        /// ffmpeg -hide_banner -i "<paramref name="input"/>" -map 0:m:title:"TV Thumbnail" -vframes 1 "<paramref name="output"/>"
        ///</code>
        ///
        /// <code>
        ///     ffmpeg -hide_banner -i "<paramref name="input"/>" -map 0:v -map -V -vframes 1 "<paramref name="output"/>"
        /// </code>
        /// </remarks>
        public static void ExtractThumbnailToFile(this ConsoleClient item, string input, string output)
        {
            var args = new string[]
            {
                "-hide_banner",
                "-v quiet",
                $"-i \"{input}\"",
                "-map 0:m:title:\"TV Thumbnail\"",
                "-vframes 1",
                "-y",
                $"\"{output}\""
            };

            item.Run(args);
        }

        public static void SetMp4FileMetadata(this ConsoleClient item, TvEpisode episode, string input, string output)
        {
            var seasonNumArg = (episode.SeasonNumber == Helpers.InvalidSeasonNumber) ? String.Empty : $"--TVSeasonNum {episode.SeasonNumber}";
            var episodeNumArg = (episode.EpisodeNumber == Helpers.InvalidEpisodeNumber) ? String.Empty : $"--TVEpisodeNum {episode.EpisodeNumber}";
            var artworkArg = (episode.ThumbnailFile == null) ? String.Empty : $"--artwork \"{episode.ThumbnailFile}\"";
            var aired = episode.AiredTime.ToString(Helpers.UtcDateTimeFormat);

            var args = new string[]
            {
                $"\"{input}\"",
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
                $"--output \"{output}\""
            };

            item.Run(args);
        }
    }
}

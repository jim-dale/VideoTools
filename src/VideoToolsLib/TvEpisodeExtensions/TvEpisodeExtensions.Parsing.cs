
namespace VideoTools
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode FromFileName(this TvEpisode result, string fileName)
        {
            var match = Regex.Match(fileName, @"^(.+)\s+(\d{4}-\d{2}-\d{2}[\s-]+\d{2}-\d{2})");
            if (match.Success && match.Groups.Count == 3)
            {
                var showName = match.Groups[1].Value.Trim();

                result.ShowName = showName;

                string broadcastStr = match.Groups[2].Value.Trim();
                if (DateTime.TryParseExact(broadcastStr, "yyyy-MM-dd HH-mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime broadcastDate))
                {
                    result.AiredTime = broadcastDate;
                }
                else if (DateTime.TryParseExact(broadcastStr, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out broadcastDate))
                {
                    result.AiredTime = broadcastDate;
                }
            }

            return result;
        }

        public static void TrySetEpisodeName(this TvEpisode item, string s)
        {
            if (String.IsNullOrEmpty(s) == false)
            {
                var match = Regex.Match(s, @"^(.+):(.*)$");
                if (match.Success && match.Groups.Count == 3)
                {
                    var episodeTitle = match.Groups[1].Value.Trim();
                    var plot = match.Groups[2].Value.Trim();

                    item.Title = episodeTitle;
                }
            }
        }

        public static void TrySetEpisodeNumber(this TvEpisode item, string s)
        {
            if (String.IsNullOrEmpty(s) == false)
            {
                var match = Regex.Match(s, Helpers.EpisodeNumberPatternBBC);
                if (match.Success && match.Groups.Count == 2)
                {
                    if (int.TryParse(match.Groups[1].Value, out int episodeNumber))
                    {
                        item.EpisodeNumber = episodeNumber;
                    }
                }
            }
        }

        public static void TrySetSeriesAndEpisodeNumbers(this TvEpisode item, string s)
        {
            if (String.IsNullOrEmpty(s) == false)
            {
                var match = Regex.Match(s, @"S(\d+) Ep(\d+)(?:/(?:\d+))?");
                if (match.Success && match.Groups.Count == 3)
                {
                    if (int.TryParse(match.Groups[1].Value, out int seriesNumber)
                        && int.TryParse(match.Groups[2].Value, out int episodeNumber))
                    {
                        item.SeasonNumber = seriesNumber;
                        item.EpisodeNumber = episodeNumber;
                    }
                }
            }
        }
    }
}

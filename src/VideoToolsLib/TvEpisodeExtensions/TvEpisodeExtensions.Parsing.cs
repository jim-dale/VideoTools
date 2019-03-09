
namespace VideoTools
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static partial class TvEpisodeExtensions
    {
        private static readonly string[] _dateTimeFormats = { "yyyy-MM-dd-HH-mm", "yyyy-MM-dd HH-mm", "dd_MM_yyyy_HH_mm_ss" };

        public static TvEpisode FromFileName(this TvEpisode result, string fileName)
        {
            if (TrySetFromFileNameVariant1(result, fileName) == false)
            {
                if (TrySetFileNameVariant2(result, fileName) == false)
                {
                    _ = TrySetFromKodiCompatibleFileName(result, fileName);
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

        private static bool TrySetFromKodiCompatibleFileName(TvEpisode item, string fileName)
        {
            var match = Regex.Match(fileName, @"^(.+)s(\d+)e(\d+)", RegexOptions.IgnoreCase);
            bool result = match.Success && match.Groups.Count == 4;
            if (result)
            {
                item.ShowName = match.Groups[1].Value.Trim();
                item.SeasonNumber = int.Parse(match.Groups[2].Value);
                item.EpisodeNumber = int.Parse(match.Groups[3].Value);
            }

            return result;
        }

        private static bool TrySetFromFileNameVariant1(TvEpisode item, string fileName)
        {
            var match = Regex.Match(fileName, @"^(.+)(\d{4}-\d{2}-\d{2}[\s-]+\d{2}-\d{2})");
            bool result = match.Success && match.Groups.Count == 3;
            if (result)
            {
                item.ShowName = match.Groups[1].Value.Trim();

                var recordTime = match.Groups[2].Value.Trim();
                if (DateTime.TryParseExact(recordTime, _dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime broadcastDate))
                {
                    item.AiredTime = broadcastDate;
                }
            }

            return result;
        }

        private static bool TrySetFileNameVariant2(TvEpisode item, string fileName)
        {
            var match = Regex.Match(fileName, @"^(.+)_(.+)_(\d{2}_\d{2}_\d{4}_\d{2}_\d{2}_\d{2})");
            bool result = match.Success && match.Groups.Count == 4;
            if (result)
            {
                item.ShowName = match.Groups[1].Value.Trim();
                item.Channel = match.Groups[2].Value.Trim();

                var recordTime = match.Groups[3].Value.Trim();
                if (DateTime.TryParseExact(recordTime, _dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime broadcastDate))
                {
                    item.AiredTime = broadcastDate;
                }
            }

            return result;
        }
    }
}

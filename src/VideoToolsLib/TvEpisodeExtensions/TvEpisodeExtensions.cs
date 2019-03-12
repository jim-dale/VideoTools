
namespace VideoTools
{
    using System;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode Fix(this TvEpisode result)
        {
            const string Prefix = "New_";
            const string Replacement = "New:";

            // Order is important - don't change
            if (String.IsNullOrEmpty(result.Title))
            {
                string aired = result.AiredTime.ToString("yyyy-MM-dd HH:mm");
                result.Title = $"{result.ShowName} {aired}";
            }
            if (string.IsNullOrEmpty(result.ShowName) == false)
            {
                result.ShowName = result.ShowName.RemoveOptionalPrefix(Prefix).Trim();
                result.ShowName = result.ShowName.RemoveOptionalPrefix(Replacement).Trim();

                result.SafeShowName = Helpers.MakeFileNameSafe(result.ShowName);
            }
            if (String.IsNullOrEmpty(result.Title) == false)
            {
                if (result.Title.StartsWith(Prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Title = Replacement + result.Title.Substring(Prefix.Length);
                }
            }
            return result;
        }

        public static TvEpisode FromDescription(this TvEpisode result)
        {
            result.TrySetSeriesAndEpisodeNumbers(result.Description);

            //if (String.IsNullOrWhiteSpace(result.Channel) == false)
            //{
            //    if (result.Channel.StartsWith("More 4", StringComparison.CurrentCultureIgnoreCase)
            //        || result.Channel.StartsWith("E4", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        //string summary = episode.Description.RemoveOptionalPrefix("Brand new series - ");
            //        //episode.TrySetEpisodeName(summary);

            //        result.TrySetSeriesAndEpisodeNumbers(result.Description);
            //    }
            //    if (result.Channel.StartsWith("ITV", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        //episode.TrySetEpisodeName(episode.Summary);

            //        result.TrySetSeriesAndEpisodeNumbers(result.Description);
            //    }
            //    if (result.Channel.StartsWith("BBC", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        //var summary = Helpers.TryRemoveEpisodeNumberBBC(episode.Summary).Trim();
            //        //episode.TrySetEpisodeName(summary);
            //        //episode.TrySetEpisodeNumber(episode.Summary);
            //    }
            //}

            return result;
        }
    }
}

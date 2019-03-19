
namespace VideoTools
{
    using System;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode FixTitleAndShowName(this TvEpisode result)
        {
            const string Prefix = "New_";
            const string Replacement = "New:";

            // Order is important - don't change
            if (String.IsNullOrEmpty(result.Title))
            {
                result.Title = result.ShowName;
            }
            if (String.IsNullOrEmpty(result.OriginalTitle))
            {
                result.OriginalTitle = result.ShowName;
            }
            if (String.IsNullOrEmpty(result.Title) == false)
            {
                if (result.Title.StartsWith(Prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Title = Replacement + result.Title.Substring(Prefix.Length);
                }
            }
            if (string.IsNullOrEmpty(result.ShowName) == false)
            {
                result.ShowName = result.ShowName.RemoveOptionalPrefix(Prefix).Trim();
                result.ShowName = result.ShowName.RemoveOptionalPrefix(Replacement).Trim();

                result.ShowName = result.ShowName.RemoveOptionalPostfix("...").Trim();
                result.ShowName = result.ShowName.TrimEnd(':').Trim();
            }
            if (result.AiredTime != DateTime.MinValue)
            {
                string aired = result.AiredTime.ToString("yyyy-MM-dd HH:mm");
                result.Title = $"{result.Title} {aired}";
            }

            return result;
        }

        public static TvEpisode SetSafeShowName(this TvEpisode result)
        {
            result.SafeShowName = Helpers.MakeFileNameSafe(result.ShowName);

            return result;
        }

        public static TvEpisode FromDescription(this TvEpisode result)
        {
            result.TrySetSeriesAndEpisodeNumbers(result.Description);

            return result;
        }
    }
}


namespace VideoTools
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode MapShowName(this TvEpisode result, ShowNameMapEntry[] map)
        {
            var name = result.ShowName;

            if (map != null && map.Any() && String.IsNullOrWhiteSpace(name) == false)
            {
                foreach (var correction in map)
                {
                    var match = Regex.Match(name, correction.Regex, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (String.IsNullOrWhiteSpace(correction.Name) == false)
                        {
                            result.ShowName = correction.Name;
                        }
                        if (String.IsNullOrWhiteSpace(correction.KodiUrl) == false)
                        {
                            result.KodiUrl = correction.KodiUrl;
                        }
                        break;
                    }
                }
            }

            return result;
        }
    }
}


namespace VideoTools
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode MapShowName(this TvEpisode result, ShowNameMapEntry[] map)
        {
            if (map != null && map.Any() && String.IsNullOrWhiteSpace(result.ShowName) == false)
            {
                var name = result.ShowName;

                foreach (var correction in map)
                {
                    var match = Regex.Match(name, correction.Regex);
                    if (match.Success)
                    {
                        result.ShowName = correction.Name;
                        break;
                    }
                }
            }

            return result;
        }
    }
}


namespace VideoTools
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode ApplyCorrections(this TvEpisode result, IList<ShowNameCorrection> corrections)
        {
            if (corrections != null && corrections.Any())
            {
                var name = result.ShowName;
                foreach (var correction in corrections)
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

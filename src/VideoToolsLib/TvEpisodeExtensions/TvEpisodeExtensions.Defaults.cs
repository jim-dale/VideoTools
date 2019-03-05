
namespace VideoTools
{
    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode Defaults(this TvEpisode result)
        {
            result.SeasonNumber = Helpers.InvalidSeasonNumber;
            result.EpisodeNumber = Helpers.InvalidEpisodeNumber;

            return result;
        }
    }
}

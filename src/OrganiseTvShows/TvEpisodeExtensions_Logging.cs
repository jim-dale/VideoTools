
namespace WtvConverter
{
    using Serilog;
    using VideoTools;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode Log(this TvEpisode result, ILogger log, string name)
        {
            log.Information("{Name}:{@Episode}", name, result);

            return result;
        }
    }
}

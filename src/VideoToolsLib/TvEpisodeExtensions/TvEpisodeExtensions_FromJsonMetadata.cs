
namespace VideoTools
{
    using System;
    using System.Globalization;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode FromJsonMetadata(this TvEpisode result, JsonPayload json)
        {
            if (json.TryGetValue<string>("format.tags.DATE_BROADCASTED", out string broadcastDateStr))
            {
                result.AiredTime = DateTime.Parse(broadcastDateStr, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
            }
            if (json.TryGetValue<string>("format.tags.title", out string title))
            {
                result.Title = title;
            }
            if (json.TryGetValue<string>("format.tags.SUMMARY", out string summary))
            {
                result.Description = summary;
            }
            if (json.TryGetValue<string>("format.tags.TVCHANNEL", out string channel))
            {
                result.Channel = channel;
            }
            if (json.TryGetValue<string>("programs.tags.service_name", out channel))
            {
                result.Channel = channel;
            }
            if (json.TryGetValue<string>("format.tags.CONTENT_TYPE", out string contentType))
            {
                result.Genre = contentType;
            }

            return result;
        }
    }
}

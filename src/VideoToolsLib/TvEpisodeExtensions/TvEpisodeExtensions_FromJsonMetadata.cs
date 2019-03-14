
namespace VideoTools
{
    using System;
    using System.Globalization;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode FromJsonMetadata(this TvEpisode result, JsonPayload json)
        {
            if (json.TryGetValue<string>("format.tags.DATE_BROADCASTED", out string broadcastDateStr) == false)
            {
                _ = json.TryGetValue<string>("format.tags.date", out broadcastDateStr);
            }
            if (String.IsNullOrWhiteSpace(broadcastDateStr) == false)
            {
                if (DateTime.TryParse(broadcastDateStr, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out DateTime date))
                {
                    result.AiredTime = date;
                }
            }
            if (json.TryGetValue<string>("format.tags.title", out string title))
            {
                result.Title = title;
                result.ShowName = title;
            }
            if (json.TryGetValue<string>("format.tags.show", out string show))
            {
                result.ShowName = show;
            }
            if (json.TryGetValue<string>("format.tags.SUMMARY", out string description) == false)
            {
                _ = json.TryGetValue<string>("format.tags.description", out description);
            }
            if (String.IsNullOrWhiteSpace(description) == false)
            {
                result.Description = description;
            }
            if (json.TryGetValue<double>("format.duration", out double duration))
            {
                result.Duration = TimeSpan.FromSeconds(duration);
            }
            if (json.TryGetValue<string>("format.tags.TVCHANNEL", out string channel) == false)
            {
                if (json.TryGetValue<string>("format.tags.network", out channel) == false)
                {
                    _ = json.TryGetValue<string>("programs.tags.service_name", out channel);
                }
            }
            if (String.IsNullOrWhiteSpace(channel) == false)
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

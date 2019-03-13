
namespace VideoTools
{
    using System;
    using System.Collections.Generic;
    using DirectShowLib.SBE;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode FromMediaCenterAttributes(this TvEpisode result, IDictionary<string, object> attributes)
        {
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.Title, out string title))
            {
                result.ShowName = title;
            }
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.Subtitle, out string subtitle))
            {
                result.Title = subtitle;
            }
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.SubtitleDescription, out string description))
            {
                result.Description = description;
            }
            if (attributes.TryGetValueAs<long>(StreamBufferRecording.EncodeTime, out long encodeTime))
            {
                result.AiredTime = new DateTime(encodeTime, DateTimeKind.Utc).ToLocalTime();
            }
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.BroadcastDateTime, out string dateStr))
            {
                var date = DateTime.Parse(dateStr);
                if (date != DateTime.MinValue)
                {
                    result.OriginalAirDate = date.ToLocalTime();
                }
            }
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.StationCallSign, out string channel))
            {
                result.Channel = channel;
            }
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.MediaCredits, out string credits))
            {
                if (String.IsNullOrWhiteSpace(credits) == false)
                {
                    string[] parts = credits.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    result.Credits = String.Join(";", parts);
                }
            }
            if (attributes.TryGetValueAs<long>(StreamBufferRecording.Duration, out long duration))
            {
                result.Duration = new TimeSpan(duration);
            }
            if (attributes.TryGetValueAs<string>(StreamBufferRecording.Genre, out string genre))
            {
                result.Genre = genre;
            }

            return result;
        }
    }
}

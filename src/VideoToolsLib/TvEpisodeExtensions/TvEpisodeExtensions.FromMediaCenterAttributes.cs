
namespace VideoTools
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public static partial class TvEpisodeExtensions
    {
        public static TvEpisode FromMediaCenterAttributes(this TvEpisode result, IDictionary<string, object> attributes)
        {
            if (TryGetValue<string>(attributes, "Title", out string title))
            {
                result.ShowName = title;
            }
            if (TryGetValue<string>(attributes, "WM/SubTitle", out string subtitle))
            {
                result.Title = subtitle;
            }
            if (TryGetValue<string>(attributes, "WM/SubTitleDescription", out string description))
            {
                result.Description = description;
            }
            if (TryGetValue<long>(attributes, "WM/WMRVEncodeTime", out long encodeTime))
            {
                result.AiredTime = new DateTime(encodeTime, DateTimeKind.Utc).ToLocalTime();
            }
            if (TryGetValue<string>(attributes, "WM/MediaOriginalBroadcastDateTime", out string dateStr))
            {
                var date = DateTime.Parse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
                if (date > DateTime.MinValue)
                {
                    result.OriginalAirDate = date.ToLocalTime();
                }
            }
            if (TryGetValue<string>(attributes, "WM/MediaStationCallSign", out string channel))
            {
                result.Channel = channel;
            }
            if (TryGetValue<string>(attributes, "WM/MediaCredits", out string credits))
            {
                if (String.IsNullOrWhiteSpace(credits) == false)
                {
                    string[] parts = credits.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    result.Credits = String.Join(";", parts);
                }
            }
            if (TryGetValue<long>(attributes, "Duration", out long duration))
            {
                result.Duration = new TimeSpan(duration);
            }
            if (TryGetValue<string>(attributes, "WM/Genre", out string genre))
            {
                result.Genre = genre;
            }

            string aired = result.AiredTime.ToString("yyyy-MM-dd HH:mm");

            return result;
        }

        private static bool TryGetValue<T>(IDictionary<string, object> attributes, string name, out T value)
        {
            value = default(T);
            var result = false;

            if (attributes.TryGetValue(name, out object obj))
            {
                value = (T)Convert.ChangeType(obj, typeof(T));
                result = true;
            }

            return result;
        }
    }
}

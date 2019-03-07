
namespace VideoTools
{
    using System;
    using System.Collections.Specialized;

    public static partial class NameValueCollectionExtensions
    {
        public static T GetAs<T>(this NameValueCollection settings, T defaultValue, string propertyName)
        {
            T result = defaultValue;

            string[] values = settings.GetValues(propertyName);
            if (values != null && values.Length > 0)
            {
                if (typeof(T).IsArray)
                {
                    result = (T)Convert.ChangeType(values, typeof(T));
                }
                else
                {
                    result = (T)Convert.ChangeType(values[0], typeof(T));
                }
            }
            return result;
        }
    }
}

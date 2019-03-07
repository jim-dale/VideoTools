
namespace VideoTools
{
    using System;
    using System.Collections.Generic;

    public static partial class IDictionaryExtensions
    {
        public static bool TryGetValueAs<T>(this IDictionary<string, object> attributes, string name, out T value)
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

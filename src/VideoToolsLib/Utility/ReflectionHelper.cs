
namespace VideoTools
{
    using System;

    public static partial class ReflectionHelper
    {
        public static bool TryGetPropertyValue(Object obj, string propertyName, out object result)
        {
            bool success = false;
            result = default(object);

            var propInfo = obj.GetType().GetProperty(propertyName);
            if (propInfo != null)
            {
                success = true;
                result = propInfo.GetValue(obj, null);
            }
            return success;
        }

        public static object GetPropertyValue(Object obj, string propertyName)
        {
            var propInfo = obj.GetType().GetProperty(propertyName);
            return propInfo?.GetValue(obj, null);
        }
    }
}

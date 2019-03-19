
namespace VideoTools
{
    using System;

    public static partial class StringExtensions
    {
        public static string RemoveOptionalPrefix(this string s, string prefix)
        {
            if (String.IsNullOrEmpty(s) == false)
            {
                if (s.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    s = s.Remove(0, prefix.Length);
                }
            }
            return s;
        }

        public static string RemoveOptionalPostfix(this string s, string postfix)
        {
            if (String.IsNullOrEmpty(s) == false)
            {
                if (s.EndsWith(postfix, StringComparison.CurrentCultureIgnoreCase))
                {
                    s = s.Remove(s.Length - postfix.Length);
                }
            }
            return s;
        }
    }
}

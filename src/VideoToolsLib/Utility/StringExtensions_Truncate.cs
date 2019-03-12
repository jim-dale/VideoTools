
namespace VideoTools
{
    public static partial class StringExtensions
    {
        public static string Truncate(this string s, int maxLength, bool trimSpaces = false)
        {
            const string Ellipsis = "...";

            var result = (trimSpaces) ?  s.Trim() : s;
            if (result.Length > maxLength)
            {
                result = result.Substring(0, maxLength - Ellipsis.Length);
                result += Ellipsis;
            }

            return result;
        }
    }
}

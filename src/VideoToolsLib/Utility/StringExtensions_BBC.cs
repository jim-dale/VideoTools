
namespace VideoTools
{
    using System.Text;

    public static partial class StringExtensions
    {
        public static string FixBBCEncoding(this string str)
        {
            StringBuilder builder = new StringBuilder(str.Length);

            foreach (var ch in str)
            {
                char c = (ch == 0x19) ? '\'' : ch;

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}

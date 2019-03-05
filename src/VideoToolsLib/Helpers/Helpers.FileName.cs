
namespace VideoTools
{
    using System.IO;
    using System.Text.RegularExpressions;

    public static partial class Helpers
    {
        public static bool IsFileNameKodiCompatible(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            var match = Regex.Match(fileName, @"^(.+)\s+s\d+e\d+", RegexOptions.IgnoreCase);
            bool result = (match.Success && match.Groups.Count == 2);

            return result;
        }

        public static string MakeFileNameSafe(string fileName, char newChar)
        {
            var result = fileName;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(c, newChar);
            }

            return result;
        }

        public static string MakeFileNameSafe(string fileName, string newValue = "")
        {
            var result = fileName;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(c.ToString(), newValue);
            }

            return result;
        }
    }
}


namespace VideoTools
{
    using System;
    using System.Text.RegularExpressions;

    public static partial class Helpers
    {
        public const string EpisodeNumberPatternBBC = @"^(\d+)\/(?:\d+)\.";

        public static string TryRemoveEpisodeNumberBBC(string s)
        {
            var result = Regex.Replace(s, EpisodeNumberPatternBBC, String.Empty);

            return result;
        }
    }
}

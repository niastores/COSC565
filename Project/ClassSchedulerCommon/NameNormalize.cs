using System.Text.RegularExpressions;

namespace ClassSchedulerCommon.Common
{

    /*
    Class Name: NameNormalizer
    Module Name: Common (static library)
    Description: Produces a comparable key from lecturer names so "Dr. J. Smith" and "smith, j" can be matched.
    Method List:
    Normalize(string name) : string
    */
    public static class NameNormalizer
    {
        private static readonly Regex Titles =
            new(@"\b(dr|prof|professor|mr|mrs|ms)\.?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /*
        Class Name: NameNormalizer
        Module Name: Common (static library)
        Description: Lowercases, strips titles and punctuation, and reorders "Last, First" to "first last".
        Variable List:
        string name - raw lecturer name from an input file
        string (return) - normalized matching key
        */
        public static string Normalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            string s = name.Trim();
            int comma = s.IndexOf(',');
            if (comma > 0) s = $"{s[(comma + 1)..]} {s[..comma]}";

            s = Titles.Replace(s, " ");
            s = Regex.Replace(s, @"[^\p{L}\s]", " ");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s.ToLowerInvariant();
        }
    }
}
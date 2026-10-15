using System;
using System.Collections.Generic;
using System.Text;

namespace ClassSchedulerCommon.Common
{
    /*
    Class Name: NameMatcher
    Module Name: Common (static library)
    Description: Tolerant lecturer-name comparison for names written inconsistently ("Bemley", "Bemley, J", "Bemley, Jesse").
    Method List:
    IsLikelyMatch(string a, string b) : bool
    */
    public static class NameMatcher
    {
        /*
        Class Name: NameMatcher
        Module Name: Common (static library)
        Description: Surnames must be equal. If both names have a given name or initial, the first initials must agree. A surname-only name matches any given name, so callers must treat multiple matches as ambiguous.
        Variable List:
        string a, b - raw names from the input files
        bool (return) - true if the names plausibly refer to the same person
        */
        public static bool IsLikelyMatch(string a, string b)
        {
            // NameNormalizer reorders "Last, First" to "first last", so the surname is always last.
            var ta = NameNormalizer.Normalize(a).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var tb = NameNormalizer.Normalize(b).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (ta.Length == 0 || tb.Length == 0) return false;
            if (ta[^1] != tb[^1]) return false;                 // surnames differ
            if (ta.Length == 1 || tb.Length == 1) return true;  // one side has only a surname
            return ta[0][0] == tb[0][0];                        // first initials must agree
        }
    }
}

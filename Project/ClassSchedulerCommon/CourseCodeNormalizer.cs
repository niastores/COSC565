using System;
using System.Collections.Generic;
using System.Text;

namespace ClassSchedulerCommon.Common
{
    
    /*
    Class Name: CourseCodeNormalizer
    Module Name: Common (static library)
    Description: Makes course codes comparable ("COSC 631" and "cosc631" match).
    Method List:
    Normalize(string code) : string
    */
    public static class CourseCodeNormalizer
    {
        /*
        Class Name: CourseCodeNormalizer
        Module Name: Common (static library)
        Description: Removes all whitespace and upper-cases the code.
        Variable List:
        string code - course code as written in an input file
        string (return) - comparison key
        */
        public static string Normalize(string code)
            => new string((code ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
    }
}

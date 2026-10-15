using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: FacultyParser
    Module Name: Input Handling
    Description: Parses the faculty information file into professor records.
    Method List:
    Parse(string path) : ParseResult<List<Professor>> - reads the faculty file
    */
    public class FacultyParser : IFileParser<List<Professor>>
    {
        /*
        Class Name: FacultyParser
        Module Name: Input Handling
        Description: Reads the faculty information file and builds Professor objects, including departments, courses taught, and lecturer-provided availability.
        Variable List:
        string path - location of the faculty file
        List<Professor> data - professors successfully parsed
        List<ParseError> errors - rows that failed validation or conversion
        */
        public ParseResult<List<Professor>> Parse(string path)
        => throw new NotImplementedException();
    }
}
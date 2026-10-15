using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: SyllabusParser
    Module Name: Input Handling
    Description: Parses the syllabus/class-schedule file into course sections.
    Method List:
    Parse(string path) : ParseResult<List<CourseSection>> - reads the syllabus file
    */
    public class SyllabusParser : IFileParser<List<CourseSection>>
    {
        /*
        Class Name: SyllabusParser
        Module Name: Input Handling
        Description: Reads the syllabus/class-schedule file and builds CourseSection objects (course, lecturer, days, times, room).
        Variable List:
        string path - location of the syllabus file
        List<CourseSection> data - sections successfully parsed
        List<ParseError> errors - rows that failed validation or conversion
        */
        public ParseResult<List<CourseSection>> Parse(string path)
        => throw new NotImplementedException();
    }
}
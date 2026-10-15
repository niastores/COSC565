using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: JsonSyllabusParser
    Module Name: Input Handling
    Description: Parses a JSON syllabus/class-schedule file into course sections.
    Method List:
    Parse(string path) : ParseResult<List<CourseSection>>
    */
    public class JsonSyllabusParser : IFileParser<List<CourseSection>>
    {
        /*
        Class Name: JsonSyllabusParser
        Module Name: Input Handling
        Description: Flattens the JSON to a cell grid and maps it to sections.
        Variable List:
        string path - JSON file location
        */
        public ParseResult<List<CourseSection>> Parse(string path)
        {
            try { return SyllabusRowMapper.MapRows(JsonTableReader.ReadRows(path)); }
            catch (Exception ex) { return ParseResult<List<CourseSection>>.Failure($"Cannot read JSON: {ex.Message}", new()); }
        }
    }
}

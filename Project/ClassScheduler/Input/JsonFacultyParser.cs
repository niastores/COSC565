using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: JsonFacultyParser
    Module Name: Input Handling
    Description: Parses a JSON faculty information file into professor records.
    Method List:
    Parse(string path) : ParseResult<List<Professor>>
    */
    public class JsonFacultyParser : IFileParser<List<Professor>>
    {
        /*
        Class Name: JsonFacultyParser
        Module Name: Input Handling
        Description: Flattens the JSON to a cell grid and maps it to professors.
        Variable List:
        string path - JSON file location
        */
        public ParseResult<List<Professor>> Parse(string path)
        {
            try { return FacultyRowMapper.MapRows(JsonTableReader.ReadRows(path)); }
            catch (Exception ex) { return ParseResult<List<Professor>>.Failure($"Cannot read JSON: {ex.Message}", new()); }
        }
    }
}
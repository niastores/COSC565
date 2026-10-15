using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: CsvFacultyParser
    Module Name: Input Handling
    Description: Parses a CSV faculty information file into professor records.
    Method List:
    Parse(string path) : ParseResult<List<Professor>>
    */
    public class CsvFacultyParser : IFileParser<List<Professor>>
    {
        /*
        Class Name: CsvFacultyParser
        Module Name: Input Handling
        Description: Reads the CSV into a cell grid and maps it to professors.
        Variable List:
        string path - CSV file location
        */
        public ParseResult<List<Professor>> Parse(string path)
        {
            try { return FacultyRowMapper.MapRows(CsvTableReader.ReadRows(path)); }
            catch (Exception ex) { return ParseResult<List<Professor>>.Failure($"Cannot read CSV: {ex.Message}", new()); }
        }
    }
}
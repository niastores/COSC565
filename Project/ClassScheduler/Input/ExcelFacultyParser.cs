using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: ExcelFacultyParser
    Module Name: Input Handling
    Description: Parses an Excel faculty information workbook into professor records.
    Method List:
    Parse(string path) : ParseResult<List<Professor>>
    */
    public class ExcelFacultyParser : IFileParser<List<Professor>>
    {
        /*
        Class Name: ExcelFacultyParser
        Module Name: Input Handling
        Description: Reads the first worksheet into a cell grid and maps it to professors.
        Variable List:
        string path - .xlsx file location
        */
        public ParseResult<List<Professor>> Parse(string path)
        {
            try { return FacultyRowMapper.MapRows(ExcelTableReader.ReadRows(path)); }
            catch (Exception ex) { return ParseResult<List<Professor>>.Failure($"Cannot read Excel file: {ex.Message}", new()); }
        }
    }
}
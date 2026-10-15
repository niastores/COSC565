using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: ExcelSyllabusParser
    Module Name: Input Handling
    Description: Parses an Excel syllabus/class-schedule workbook into course sections.
    Method List:
    Parse(string path) : ParseResult<List<CourseSection>>
    */
    public class ExcelSyllabusParser : IFileParser<List<CourseSection>>
    {
        /*
        Class Name: ExcelSyllabusParser
        Module Name: Input Handling
        Description: Reads the first worksheet into a cell grid and maps it to sections.
        Variable List:
        string path - .xlsx file location
        */
        public ParseResult<List<CourseSection>> Parse(string path)
        {
            try { return SyllabusRowMapper.MapRows(ExcelTableReader.ReadRows(path)); }
            catch (Exception ex) { return ParseResult<List<CourseSection>>.Failure($"Cannot read Excel file: {ex.Message}", new()); }
        }
    }
}
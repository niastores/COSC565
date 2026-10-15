using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
     Class Name: CsvSyllabusParser
     Module Name: Input Handling
     Description: Parses a CSV syllabus/class-schedule file.
     Method List:
     Parse(string path) : ParseResult<List<CourseSection>>
     */
    public class CsvSyllabusParser : IFileParser<List<CourseSection>>
    {
        /*
        Class Name: CsvSyllabusParser
        Module Name: Input Handling
        Description: Reads the CSV into a cell grid and maps it to sections. I/O failures become a file-level ParseError.
        Variable List:
        string path - CSV file location
        */
        public ParseResult<List<CourseSection>> Parse(string path)
        {
            try { return SyllabusRowMapper.MapRows(CsvTableReader.ReadRows(path)); }
            catch (Exception ex) { return ParseResult<List<CourseSection>>.Failure($"Cannot read CSV: {ex.Message}", new()); }
        }
    }
}
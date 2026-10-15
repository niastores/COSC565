using ClassScheduler.Models;
using ClassScheduler.Input;

namespace ClassScheduler.Input
{
    /*
    Class Name: FileParserFactory
    Module Name: Input Handling
    Description: Chooses a parser by data type and file extension. Syllabus files support CSV, JSON, XLSX, DOCX, and PDF; faculty files support CSV, JSON, and XLSX.
    Method List:
    Create<T>(string extension) : IFileParser<T>
    IsSupported(Type dataType, string extension) : bool
    GetSupportedExtensions(Type dataType) : IReadOnlyList<string>
    */
    public static class FileParserFactory
    {
        private static readonly Dictionary<(Type, string), Func<object>> Registry = new()
        {
            [(typeof(List<CourseSection>), ".csv")] = () => new CsvSyllabusParser(),
            [(typeof(List<CourseSection>), ".json")] = () => new JsonSyllabusParser(),
            [(typeof(List<CourseSection>), ".xlsx")] = () => new ExcelSyllabusParser(),
            [(typeof(List<Professor>), ".docx")] = () => new FacultyDocxParser(),
            [(typeof(List<Professor>), ".pdf")] = () => new FacultyPdfParser(),
            [(typeof(List<Professor>), ".csv")] = () => new CsvFacultyParser(),
            [(typeof(List<Professor>), ".json")] = () => new JsonFacultyParser(),
            [(typeof(List<Professor>), ".xlsx")] = () => new ExcelFacultyParser(),
        };

        /*
        Class Name: FileParserFactory
        Module Name: Input Handling
        Description: Returns the parser for data type T and the extension. Throws NotSupportedException if none.
        Variable List:
        string extension - extension with dot, case-insensitive
        IFileParser<T> (return) - matching parser
        */
        public static IFileParser<T> Create<T>(string extension)
        {
            var key = (typeof(T), extension.ToLowerInvariant());
            if (!Registry.TryGetValue(key, out var make))
                throw new NotSupportedException(
                    $"No parser for '{extension}'. Supported: {string.Join(", ", GetSupportedExtensions(typeof(T)))}");
            return (IFileParser<T>)make();
        }

        /*
        Class Name: FileParserFactory
        Module Name: Input Handling
        Description: Tests whether a parser exists for this data type and extension.
        Variable List:
        Type dataType - typeof(List<CourseSection>) or typeof(List<Professor>)
        string extension - extension to test
        bool (return) - true if supported
        */
        public static bool IsSupported(Type dataType, string extension)
            => Registry.ContainsKey((dataType, extension.ToLowerInvariant()));

        /*
        Class Name: FileParserFactory
        Module Name: Input Handling
        Description: Lists supported extensions for a data type, for messages and help text.
        Variable List:
        Type dataType - data type to list extensions for
        IReadOnlyList<string> (return) - sorted extensions
        */
        public static IReadOnlyList<string> GetSupportedExtensions(Type dataType)
            => Registry.Keys.Where(k => k.Item1 == dataType).Select(k => k.Item2).OrderBy(e => e).ToList();
    }
}
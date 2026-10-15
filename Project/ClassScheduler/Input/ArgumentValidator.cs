using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: ArgumentValidator
    Module Name: Input Handling
    Description: Verifies the two input file locations, whether they came from command-line arguments or from a prompt.
    Method List:
    Validate(string[] args) : CommandLineArgs
    ValidatePaths(string syllabusPath, string facultyPath) : CommandLineArgs
    CleanPath(string raw) : string
    TryCheckFile(string path, out string error) : bool
    */
    public static class ArgumentValidator
    {
        /*
        Class Name: ArgumentValidator
        Module Name: Input Handling
        Description: Requires exactly two arguments, then validates them as file paths.
        Variable List:
        string[] args - raw arguments: syllabus path, faculty path
        CommandLineArgs (return) - validated paths
        */
        public static CommandLineArgs Validate(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException(
                    $"Expected 2 file paths but received {args.Length}. " +
                    "Usage: <syllabus-file> <faculty-file>, or run with no arguments to be prompted.");

            return ValidatePaths(args[0], args[1]);
        }
        /*
        Class Name: ArgumentValidator
        Module Name: Input Handling
        Description: Cleans and validates both paths. Throws on the first problem found.
        Variable List:
        string syllabusPath - location of the syllabus/class-schedule file
        string facultyPath - location of the faculty information file
        CommandLineArgs (return) - validated, cleaned, absolute paths
        */
        public static CommandLineArgs ValidatePaths(string syllabusPath, string facultyPath)
        {
            string syllabus = CleanPath(syllabusPath);
            string faculty = CleanPath(facultyPath);

            if (!TryCheckFile(syllabus, typeof(List<CourseSection>), out var e1)) throw new ArgumentException($"Syllabus: {e1}");
            if (!TryCheckFile(faculty, typeof(List<Professor>), out var e2)) throw new ArgumentException($"Faculty: {e2}");

            if (string.Equals(syllabus, faculty, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The syllabus and faculty files must be two different files.");

            return new CommandLineArgs { SyllabusPath = syllabus, FacultyPath = faculty };
        }

        /*
        Class Name: ArgumentValidator
        Module Name: Input Handling
        Description: Trims whitespace and surrounding quotes (common when a path is pasted from Explorer or a shell), then converts to an absolute path.
        Variable List:
        string raw - path as entered
        string (return) - cleaned absolute path, or empty string if input was blank
        */
        public static string CleanPath(string raw)
        {
            string trimmed = (raw ?? "").Trim().Trim('"', '\'');
            return trimmed.Length == 0 ? "" : Path.GetFullPath(trimmed);
        }

        /*
        Class Name: ArgumentValidator
        Module Name: Input Handling
        Description: Checks that a path is non-empty, exists, and has an extension supported for the given data type.
        Variable List:
        string path - cleaned path to check
        Type dataType - typeof(List<CourseSection>) for the syllabus, typeof(List<Professor>) for the faculty file
        out string error - reason for failure, empty on success
        bool (return) - true if the file is usable
        */
        public static bool TryCheckFile(string path, Type dataType, out string error)
        {
            error = "";
            string ext = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(path))
                error = "No path was provided.";
            else if (!File.Exists(path))
                error = $"File not found: {path}";
            else if (!FileParserFactory.IsSupported(dataType, ext))
                error = $"Unsupported format '{ext}'. Supported: {string.Join(", ", FileParserFactory.GetSupportedExtensions(dataType))}";
            return error.Length == 0;
        }
    }
}
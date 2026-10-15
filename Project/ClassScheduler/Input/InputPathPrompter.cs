using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;

namespace ClassScheduler.Input
{

    /*
    Class Name: InputPathPrompter
    Module Name: Input Handling
    Description: Interactively asks the user for the two input file locations and re-prompts on invalid entries. Each file is validated against the formats supported for its role.
    Method List:
    Prompt(TextReader input, TextWriter output, int maxAttempts) : CommandLineArgs
    AskForFile(string label, Type dataType, TextReader input, TextWriter output, int maxAttempts) : string (private)
    */
    public static class InputPathPrompter
    {
        /*
        Class Name: InputPathPrompter
        Module Name: Input Handling
        Description: Shows the supported formats for each file, prompts for the syllabus file, then the faculty file, and validates the pair.
        Variable List:
        TextReader input - source of user input (Console.In in production)
        TextWriter output - destination for prompts and messages (Console.Out in production)
        int maxAttempts - invalid entries allowed per file before giving up
        CommandLineArgs (return) - validated paths
        */
        public static CommandLineArgs Prompt(TextReader input, TextWriter output, int maxAttempts = 3)
        {
            var syllabusType = typeof(List<CourseSection>);
            var facultyType = typeof(List<Professor>);

            output.WriteLine($"Syllabus formats: {string.Join(", ", FileParserFactory.GetSupportedExtensions(syllabusType))}");
            output.WriteLine($"Faculty formats:  {string.Join(", ", FileParserFactory.GetSupportedExtensions(facultyType))}");

            string syllabus = AskForFile("Syllabus / class-schedule file", syllabusType, input, output, maxAttempts);
            string faculty = AskForFile("Faculty information file", facultyType, input, output, maxAttempts);

            return ArgumentValidator.ValidatePaths(syllabus, faculty);
        }

        /*
        Class Name: InputPathPrompter
        Module Name: Input Handling
        Description: Asks for one file path until a valid one is entered or attempts run out.
        Variable List:
        string label - description shown in the prompt
        Type dataType - typeof(List<CourseSection>) or typeof(List<Professor>); selects the allowed extensions
        TextReader input - user input source
        TextWriter output - prompt destination
        int maxAttempts - allowed invalid entries
        string (return) - cleaned, valid path
        */
        private static string AskForFile(string label, Type dataType, TextReader input, TextWriter output, int maxAttempts)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                output.Write($"{label}: ");
                string path = ArgumentValidator.CleanPath(input.ReadLine() ?? "");

                if (ArgumentValidator.TryCheckFile(path, dataType, out var error))
                    return path;

                output.WriteLine($"  {error}");
            }
            throw new ArgumentException($"No valid path for '{label}' after {maxAttempts} attempts.");
        }
    }
}

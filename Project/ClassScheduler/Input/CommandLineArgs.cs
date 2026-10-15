namespace ClassScheduler.Input
{
    /*
    Class Name: CommandLineArgs
    Module Name: Input Handling
    Description: Data container holding the two validated input file paths.
    Method List:
    None (data container only)
    */
    public class CommandLineArgs
    {
        public string SyllabusPath { get; set; } = "";
        public string FacultyPath { get; set; } = "";
    }
}
namespace ClassScheduler.Input
{
    /*
     Class Name: ParseError
     Module Name: Input Handling
     Description: One problem found while parsing an input file.
     Method List:
     None (data container only)
     */
    public class ParseError
    {
        public int Line { get; set; }
        public string Field { get; set; } = "";
        public string Message { get; set; } = "";
        public ParseSeverity Severity { get; set; } = ParseSeverity.Error;
    }
}
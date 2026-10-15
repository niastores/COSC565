namespace ClassScheduler.Input
{

    /*
    Class Name: ParseResult<T>
    Module Name: Input Handling
    Description: Parsed data plus any parse errors.
    Method List:
    None (data container only)
    */
    public class ParseResult<T>
    {
        public T Data { get; set; } = default!;
        public List<ParseError> Errors { get; set; } = new();
        /*
        Class Name: ParseResult<T>
        Module Name: Input Handling
        Description: Builds a result for a file that could not be read at all.
        Variable List:
        string message - what went wrong
        T empty - empty data value (e.g. an empty list) so callers never see null
        ParseResult<T> (return) - result carrying one file-level error
        */
        public static ParseResult<T> Failure(string message, T empty)
            => new() { Data = empty, Errors = { new ParseError { Line = 0, Field = "file", Message = message } } };
    }

}
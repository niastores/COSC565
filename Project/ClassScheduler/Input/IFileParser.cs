namespace ClassScheduler.Input
{
    /*
Class Name: IFileParser<T>
Module Name: Input Handling
Description: Interface that every input file parser implements.
Method List:
Parse(string path) : ParseResult<T> - reads a file and returns typed data plus errors
*/
    public interface IFileParser<T>
    {
        /*
        Class Name: IFileParser<T>
        Module Name: Input Handling
        Description: Contract for reading a file and converting its contents into a typed list, with any row-level errors collected.
        Variable List:
        string path - location of the file to parse
        ParseResult<T> (return) - parsed data plus a list of parse errors
        */
        ParseResult<T> Parse(string path);
    }
}
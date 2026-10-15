namespace ClassScheduler.App
{
    /*
    Class Name: ILogger
    Module Name: Cross-Cutting
    Description: Logging interface with three severity levels.
    Method List:
    Info(string m) : void
    Warn(string m) : void
    Error(string m) : void
    */
    public interface ILogger
    {
        /* Class Name: ILogger | Module Name: Cross-Cutting | Description: Logs an informational message. | Variable List: string m - message text */
        void Info(string m);

        /* Class Name: ILogger | Module Name: Cross-Cutting | Description: Logs a non-fatal warning. | Variable List: string m - message text */
        void Warn(string m);

        /* Class Name: ILogger | Module Name: Cross-Cutting | Description: Logs an error. | Variable List: string m - message text */
        void Error(string m);
    }

    /*
    Class Name: ConsoleLogger
    Module Name: Cross-Cutting
    Description: ILogger implementation that writes to the console.
    Method List:
    Info(string m) : void
    Warn(string m) : void
    Error(string m) : void
    */
    public class ConsoleLogger : ILogger
    {
        /* Class Name: ConsoleLogger | Module Name: Cross-Cutting | Description: Writes an info line. | Variable List: string m - message text */
        public void Info(string m) => Console.WriteLine($"[INFO ] {m}");

        /* Class Name: ConsoleLogger | Module Name: Cross-Cutting | Description: Writes a warning line. | Variable List: string m - message text */
        public void Warn(string m) => Console.WriteLine($"[WARN ] {m}");

        /* Class Name: ConsoleLogger | Module Name: Cross-Cutting | Description: Writes an error line to stderr. | Variable List: string m - message text */
        public void Error(string m) => Console.Error.WriteLine($"[ERROR] {m}");
    }
}

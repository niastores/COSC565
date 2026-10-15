using ClassScheduler.Input;
using ClassScheduler.App;

namespace ClassScheduler.App
{
    /*
    Class Name: Program
    Module Name: Orchestration
    Description: Application entry point.
    Method List:
    Main(string[] args) : int
    */
    public class Program
    {
        /*
        Class Name: Program
        Module Name: Orchestration
        Description: Takes file locations from the arguments if given, otherwise prompts for them, then runs the service and returns an exit code.
        Variable List:
        string[] args - raw arguments: none (prompt) or exactly two file paths
        int (return) - 0 success, 1 validation errors, 2 fatal failure
        */
        public static int Main(string[] args)
        {
            ILogger log = new ConsoleLogger();
            try
            {
                CommandLineArgs parsed = args.Length == 0
                    ? InputPathPrompter.Prompt(Console.In, Console.Out)
                    : ArgumentValidator.Validate(args);

                log.Info($"Syllabus: {parsed.SyllabusPath}");
                log.Info($"Faculty:  {parsed.FacultyPath}");

                var report = new ScheduleGenerationService(log, new AppConfig()).Run(parsed);
                return report.Errors.Count == 0 ? 0 : 1;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return 2;
            }
        }
    }
}
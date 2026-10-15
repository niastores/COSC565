using ClassScheduler.Scheduling;

namespace ClassScheduler.App
{
    /*
    Class Name: AppConfig
    Module Name: Cross-Cutting
    Description: Application-wide settings.
    Method List:
    None (data container only)
    */
    public class AppConfig
    {
        public OfficeHoursPolicy DefaultPolicy { get; set; } = new();
        public string OutputDirectory { get; set; } = "output";
        public string ScheduleNote { get; set; } = "NOTE:  Instructors are not to be interrupted during class times.";
    }
}
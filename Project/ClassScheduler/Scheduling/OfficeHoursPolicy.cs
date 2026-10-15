namespace ClassScheduler.Scheduling
{
    /*
    Class Name: OfficeHoursPolicy
    Module Name: Office-Hours Scheduling
    Description: Configurable rules for office-hour placement.
    Method List:
    None (data container only)
    */
    public class OfficeHoursPolicy
    {
        public int MinHoursPerWeek { get; set; } = 3;            // used when a professor has no required hours
        public int SlotLengthMinutes { get; set; } = 60;         // building-block size for office hours
        public TimeSpan DayStart { get; set; } = TimeSpan.FromHours(8);
        public TimeSpan DayEnd { get; set; } = TimeSpan.FromHours(18);
        public int BufferAroundClassMinutes { get; set; } = 0;   // gap kept before/after classes; 0 allows back-to-back
        public int MaxPerDay { get; set; } = 3;                  // maximum slots per day
        public DayOfWeek[] WorkDays { get; set; } =
            { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
    }
}

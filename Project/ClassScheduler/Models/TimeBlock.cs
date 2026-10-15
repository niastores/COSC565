namespace ClassScheduler.Models
{
    /*
    Class Name: TimeBlock
    Module Name: Domain Model
    Description: Represents a span of time on a given weekday.
    Method List:
    None (data container only)
    */
    public class TimeBlock
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

}
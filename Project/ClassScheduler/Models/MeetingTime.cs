using ClassSchedulerCommon.Common;

namespace ClassScheduler.Models;

public class MeetingTime
{
    public DayOfWeek Day { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }

    /*
    Class Name: MeetingTime
    Module Name: Domain Model
    Description: Determines whether this meeting overlaps another on the same day, using TimeMath from the static library.
    Variable List:
    MeetingTime other - meeting to compare against
    bool (return) - true if the two time ranges overlap
    */
    public bool OverlapsWith(MeetingTime other)
        => Day == other.Day && TimeMath.Overlaps(Start, End, other.Start, other.End);
}
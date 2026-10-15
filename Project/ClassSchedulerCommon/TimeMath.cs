using System;
using System.Collections.Generic;
using System.Text;

namespace ClassSchedulerCommon.Common
{
    /*
    Class Name: TimeMath
    Module Name: Common (static library)
    Description: Stateless time-range helpers shared by all modules.
    Method List:
    Overlaps(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB) : bool
    DurationMinutes(TimeSpan start, TimeSpan end) : int
    SnapUp(TimeSpan time, int slotMinutes) : TimeSpan
    */
    public static class TimeMath
    {
        /*
        Class Name: TimeMath
        Module Name: Common (static library)
        Description: Tests whether two time ranges on the same day overlap. Ranges that only touch at an endpoint do not overlap.
        Variable List:
        TimeSpan startA, endA - first range
        TimeSpan startB, endB - second range
        bool (return) - true if the ranges overlap
        */
        public static bool Overlaps(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB)
            => startA < endB && startB < endA;

        /*
        Class Name: TimeMath
        Module Name: Common (static library)
        Description: Returns the length of a range in whole minutes.
        Variable List:
        TimeSpan start - range start
        TimeSpan end - range end
        int (return) - minutes between start and end
        */
        public static int DurationMinutes(TimeSpan start, TimeSpan end)
            => (int)(end - start).TotalMinutes;

        /*
        Class Name: TimeMath
        Module Name: Common (static library)
        Description: Rounds a time up to the next slot boundary so office hours start on clean times.
        Variable List:
        TimeSpan time - time to round
        int slotMinutes - slot length in minutes (must be > 0)
        TimeSpan (return) - rounded time
        */
        public static TimeSpan SnapUp(TimeSpan time, int slotMinutes)
        {
            double slots = Math.Ceiling(time.TotalMinutes / slotMinutes);
            return TimeSpan.FromMinutes(slots * slotMinutes);
        }
    }
}
using ClassSchedulerCommon.Common;
using ClassScheduler.Models;

namespace ClassScheduler.Scheduling
{
    /*
    Class Name: FreeTimeCalculator
    Module Name: Office-Hours Scheduling
    Description: Computes when a professor is free and available.
    Method List:
    ComputeFreeBlocks(ProfessorSchedule teaching, OfficeHoursPolicy policy) : List<TimeBlock>
    IntersectWithAvailability(List<TimeBlock> free, List<TimeBlock> availability) : List<TimeBlock>
    */
    public class FreeTimeCalculator
    {
        /*
        Class Name: FreeTimeCalculator
        Module Name: Office-Hours Scheduling
        Description: For each work day, sweeps left to right through the day's classes (widened by the buffer) and emits the gaps between them within DayStart..DayEnd.
        Variable List:
        ProfessorSchedule teaching - schedule with classes
        OfficeHoursPolicy policy - work days, day bounds and buffer
        List<TimeBlock> (return) - free blocks
        */
        public List<TimeBlock> ComputeFreeBlocks(ProfessorSchedule teaching, OfficeHoursPolicy policy)
        {
            var free = new List<TimeBlock>();
            var buffer = TimeSpan.FromMinutes(policy.BufferAroundClassMinutes);
            foreach (var day in policy.WorkDays)
            {
                // Busy intervals for this day, sorted by start.
                var busy = teaching.Teaching
                    .SelectMany(s => s.Meetings)
                    .Where(m => m.Day == day)
                    .Select(m => (Start: m.Start - buffer, End: m.End + buffer))
                    .OrderBy(b => b.Start)
                    .ToList();
                // "cursor" is the earliest time not yet known to be busy.
                var cursor = policy.DayStart;
                foreach (var b in busy)
                {
                    if (cursor >= policy.DayEnd) break;
                    // A gap exists between the cursor and the next class.
                    if (b.Start > cursor)
                        free.Add(new TimeBlock { Day = day, Start = cursor, End = b.Start < policy.DayEnd ? b.Start : policy.DayEnd });
                    // Overlapping classes simply push the cursor forward.
                    if (b.End > cursor) cursor = b.End;
                }
                // Whatever remains after the last class is free.
                if (cursor < policy.DayEnd)
                    free.Add(new TimeBlock { Day = day, Start = cursor, End = policy.DayEnd });
            }
            return free;
        }
        /*
        Class Name: FreeTimeCalculator
        Module Name: Office-Hours Scheduling
        Description: Keeps only the parts of free blocks that fall inside the lecturer's availability on the same day.
        Variable List:
        List<TimeBlock> free - free blocks
        List<TimeBlock> availability - lecturer availability table
        List<TimeBlock> (return) - blocks both free and available
        */
        public List<TimeBlock> IntersectWithAvailability(List<TimeBlock> free, List<TimeBlock> availability)
        {
            var result = new List<TimeBlock>();
            foreach (var f in free)
                foreach (var a in availability.Where(a => a.Day == f.Day))
                {
                    if (!TimeMath.Overlaps(f.Start, f.End, a.Start, a.End)) continue;
                    // The intersection runs from the later start to the earlier end.
                    result.Add(new TimeBlock
                    {
                        Day = f.Day,
                        Start = f.Start > a.Start ? f.Start : a.Start,
                        End = f.End < a.End ? f.End : a.End,
                    });
                }
            return result.OrderBy(b => ((int)b.Day + 6) % 7).ThenBy(b => b.Start).ToList();
        }
    }
}

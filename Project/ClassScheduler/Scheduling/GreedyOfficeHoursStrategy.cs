using ClassSchedulerCommon.Common;
using ClassScheduler.Models;

namespace ClassScheduler.Scheduling
{
    /*
    Class Name: GreedyOfficeHoursStrategy
    Module Name: Office-Hours Scheduling
    Description: Default strategy. Cuts usable time into fixed-length slots, then repeatedly picks the slot on the least-used day, preferring slots adjacent to a class. Touching slots are merged into one block.
    Method List:
    Generate(Professor p, ProfessorSchedule teaching, OfficeHoursPolicy policy) : List<OfficeHourSlot>
    */
    public class GreedyOfficeHoursStrategy : IOfficeHoursStrategy
    {
        private readonly FreeTimeCalculator _calc = new();
        // A possible slot, tagged with the class it sits next to (if any).
        private sealed record Candidate(DayOfWeek Day, TimeSpan Start, TimeSpan End, string? Code);
        /*
        Class Name: GreedyOfficeHoursStrategy
        Module Name: Office-Hours Scheduling
        Description: Computes usable time, builds candidate slots, greedily selects enough of them, and merges adjacent selections. Returns fewer hours than required if the schedule cannot fit them; the validator reports that.
        Variable List:
        Professor p - professor receiving office hours
        ProfessorSchedule teaching - teaching schedule
        OfficeHoursPolicy policy - placement rules
        List<OfficeHourSlot> (return) - selected, merged slots
        */
        public List<OfficeHourSlot> Generate(Professor p, ProfessorSchedule teaching, OfficeHoursPolicy policy)
        {
            // 1. Usable time = free time, restricted to availability when the professor provided any.
            var free = _calc.ComputeFreeBlocks(teaching, policy);
            var usable = p.AvailabilityTable.Count > 0
                ? _calc.IntersectWithAvailability(free, p.AvailabilityTable)
                : free;
            // 2. Build candidates. Slots start at the beginning of each usable block so a block
            //    that starts when a class ends yields a slot touching that class.
            var buffer = TimeSpan.FromMinutes(policy.BufferAroundClassMinutes);
            var slotLength = TimeSpan.FromMinutes(policy.SlotLengthMinutes);
            var classMeetings = teaching.Teaching
                .SelectMany(s => s.Meetings.Select(m => (Code: s.Course.Code, Meeting: m)))
                .ToList();
            var candidates = new List<Candidate>();
            foreach (var block in usable)
                for (var start = block.Start; start + slotLength <= block.End; start += slotLength)
                {
                    var end = start + slotLength;
                    // Adjacent = starts right after a class, or ends right before one.
                    string? after = classMeetings.FirstOrDefault(c => c.Meeting.Day == block.Day && c.Meeting.End + buffer == start).Code;
                    string? before = classMeetings.FirstOrDefault(c => c.Meeting.Day == block.Day && c.Meeting.Start - buffer == end).Code;
                    candidates.Add(new Candidate(block.Day, start, end, after ?? before));
                }
            // 3. Greedy selection.
            int required = p.RequiredOfficeHoursPerWeek > 0 ? p.RequiredOfficeHoursPerWeek : policy.MinHoursPerWeek;
            int slotsNeeded = (int)Math.Ceiling(required * 60.0 / policy.SlotLengthMinutes);
            var chosen = new List<Candidate>();
            var perDay = policy.WorkDays.ToDictionary(d => d, _ => 0);
            while (chosen.Count < slotsNeeded)
            {
                var best = candidates
                    .Where(c => perDay.GetValueOrDefault(c.Day) < policy.MaxPerDay)                    // daily cap
                    .Where(c => !chosen.Any(x => x.Day == c.Day && TimeMath.Overlaps(x.Start, x.End, c.Start, c.End)))
                    .OrderBy(c => perDay.GetValueOrDefault(c.Day))   // spread across days first
                    .ThenBy(c => c.Code is null ? 1 : 0)             // then prefer slots next to a class
                    .ThenBy(c => ((int)c.Day + 6) % 7)               // then earliest day of the week
                    .ThenBy(c => c.Start)                            // then earliest time
                    .FirstOrDefault();
                if (best is null) break;   // nothing else fits
                chosen.Add(best);
                perDay[best.Day] = perDay.GetValueOrDefault(best.Day) + 1;
            }
            // 4. Merge touching slots on the same day into one block; keep the first slot's course tag.
            var merged = new List<OfficeHourSlot>();
            foreach (var c in chosen.OrderBy(c => ((int)c.Day + 6) % 7).ThenBy(c => c.Start))
            {
                var last = merged.LastOrDefault();
                if (last is not null && last.Block.Day == c.Day && last.Block.End == c.Start)
                    last.Block.End = c.End;
                else
                    merged.Add(new OfficeHourSlot
                    {
                        Professor = p,
                        CourseCode = c.Code,
                        Block = new TimeBlock { Day = c.Day, Start = c.Start, End = c.End },
                    });
            }
            return merged;
        }
    }
}
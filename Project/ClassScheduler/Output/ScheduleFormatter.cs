using ClassScheduler.Models;
using System.Text;

namespace ClassScheduler.Output
{
    /*
Class Name: ScheduleFormatter
Module Name: Output
Description: Formats a schedule as a plain-text weekly grid.
Method List:
ToWeeklyGrid(ProfessorSchedule s) : string
*/
    public class ScheduleFormatter
    {
        private static readonly DayOfWeek[] Days =
            { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        /*
        Class Name: ScheduleFormatter
        Module Name: Output
        Description: Renders rows of 30 minutes by Monday..Friday columns. A cell shows the course code of a class overlapping that row, or "Office Hrs".
        Variable List:
        ProfessorSchedule s - schedule to format
        string (return) - formatted grid
        */
        public string ToWeeklyGrid(ProfessorSchedule s)
        {
            var classes = s.Teaching.SelectMany(t => t.Meetings.Select(m => (Code: t.Course.Code, M: m))).ToList();
            var blocks = classes.Select(c => (c.M.Start, c.M.End))
                .Concat(s.OfficeHours.Select(o => (o.Block.Start, o.Block.End))).ToList();
            if (blocks.Count == 0) return "(nothing scheduled)";
            // Round the visible range out to whole hours.
            var first = TimeSpan.FromHours(Math.Floor(blocks.Min(b => b.Start).TotalHours));
            var last = TimeSpan.FromHours(Math.Ceiling(blocks.Max(b => b.End).TotalHours));
            var step = TimeSpan.FromMinutes(30);
            var sb = new StringBuilder();
            sb.Append("Time".PadRight(10));
            foreach (var d in Days) sb.Append(d.ToString().PadRight(14));
            sb.AppendLine();
            for (var t = first; t < last; t += step)
            {
                sb.Append(ScheduleText.FormatTime(t).PadRight(10));
                foreach (var d in Days)
                {
                    // A class or office hour "covers" this row if it overlaps [t, t + 30 min).
                    string? code = classes.FirstOrDefault(c => c.M.Day == d && c.M.Start < t + step && t < c.M.End).Code;
                    bool office = s.OfficeHours.Any(o => o.Block.Day == d && o.Block.Start < t + step && t < o.Block.End);
                    sb.Append((code ?? (office ? "Office Hrs" : "")).PadRight(14));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using ClassScheduler.Models;

namespace ClassScheduler.Output
{
    /*
    Class Name: ScheduleText
    Module Name: Output
    Description: Static helpers that turn schedule data into the display text used in the sample layout. Used by the Word, HTML, Excel and CSV exporters.
    Method List:
    FormatTime(TimeSpan t) : string
    FormatDays(IEnumerable<DayOfWeek> days) : string
    FormatClassTimes(CourseSection s) : List<string>
    FormatLocation(Room? room) : string
    FormatOfficeHours(ProfessorSchedule s) : List<string>
    */
    public static class ScheduleText
    {
        /*
        Class Name: ScheduleText
        Module Name: Output
        Description: Formats a time of day as "h:mm AM/PM".
        Variable List:
        TimeSpan t - time of day
        string (return) - e.g. "5:00 PM"
        */
        public static string FormatTime(TimeSpan t)
            => DateTime.Today.Add(t).ToString("h:mm tt", CultureInfo.InvariantCulture);

        /*
        Class Name: ScheduleText
        Module Name: Output
        Description: Joins day names: "Monday", "Tuesday & Thursday", "Monday, Wednesday & Friday".
        Variable List:
        IEnumerable<DayOfWeek> days - days to join (week order is applied, Monday first)
        string (return) - joined text
        */
        public static string FormatDays(IEnumerable<DayOfWeek> days)
        {
            var names = days.Distinct().OrderBy(d => ((int)d + 6) % 7).Select(d => d.ToString()).ToList();
            return names.Count <= 2
                ? string.Join(" & ", names)
                : string.Join(", ", names.Take(names.Count - 1)) + " & " + names[^1];
        }

        /*
        Class Name: ScheduleText
        Module Name: Output
        Description: Groups a section's meetings by identical start/end and returns one line per group, e.g. "Tuesday & Thursday  9:30 AM to 10:45 AM". Sections with no meetings show "TIME NEEDS REVIEW" (times unreadable) or "No scheduled meeting times" (asynchronous). Repaired sections get a trailing verification line.
        Variable List:
        CourseSection s - section to describe
        List<string> (return) - one line per distinct time range
        */
        public static List<string> FormatClassTimes(CourseSection s)
        {
            // No meetings: either an asynchronous course, or the times could not be read.
            if (s.Meetings.Count == 0)
                return new List<string> { s.Notes.Count > 0 ? "TIME NEEDS REVIEW" : "No scheduled meeting times" };

            var lines = s.Meetings
                .GroupBy(m => (m.Start, m.End))
                .OrderBy(g => g.Key.Start)
                .Select(g => $"{FormatDays(g.Select(m => m.Day))}  {FormatTime(g.Key.Start)} to {FormatTime(g.Key.End)}")
                .ToList();

            // Times were repaired from questionable source data: tell the reader.
            if (s.Notes.Count > 0) lines.Add("(times repaired - verify; see review notes)");
            return lines;
        }

        /*
        Class Name: ScheduleText
        Module Name: Output
        Description: Formats a room as "Building Number" (either part may be empty).
        Variable List:
        Room? room - room, may be null
        string (return) - e.g. "CS 313", or empty
        */
        public static string FormatLocation(Room? room)
            => room is null ? "" : $"{room.Building} {room.Number}".Trim();

        /*
        Class Name: ScheduleText
        Module Name: Output
        Description: Builds office-hour lines. The first block of a day is "Day: start – end (COURSE)". Further blocks that day are "& start – end (COURSE)". The start omits AM/PM when it matches the end.
        Variable List:
        ProfessorSchedule s - schedule whose office hours are formatted
        List<string> (return) - display lines in week order
        */
        public static List<string> FormatOfficeHours(ProfessorSchedule s)
        {
            var lines = new List<string>();
            foreach (var day in s.OfficeHours.GroupBy(o => o.Block.Day).OrderBy(g => ((int)g.Key + 6) % 7))
            {
                bool first = true;
                foreach (var o in day.OrderBy(o => o.Block.Start))
                {
                    string start = FormatTime(o.Block.Start), end = FormatTime(o.Block.End);
                    if (start[^2..] == end[^2..]) start = start[..^3];   // drop shared AM/PM
                    string tag = string.IsNullOrEmpty(o.CourseCode) ? "" : $" ({o.CourseCode})";
                    lines.Add(first ? $"{day.Key}: {start} – {end}{tag}" : $"& {start} – {end}{tag}");
                    first = false;
                }
            }
            return lines;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;
using System.Text.RegularExpressions;
using System.Globalization;


namespace ClassScheduler.Input
{
    /*
    Class Name: SyllabusRowMapper
    Module Name: Input Handling
    Description: Converts a grid of text cells (from CSV, JSON, Excel, Word or PDF) into CourseSection objects. Never drops a row that has content: unreadable or inconsistent values are repaired when safe, otherwise the section is kept without meeting times and flagged.
    Method List:
    FindHeaderRow(IReadOnlyList<string[]> rows) : int
    MapRows(IReadOnlyList<string[]> rows) : ParseResult<List<CourseSection>>
    MapTable(IReadOnlyList<string[]> rows, int headerIndex) : ParseResult<List<CourseSection>>
    ParseDays(string text) : List<DayOfWeek>
    TryParseTimeRange(string text, out TimeSpan start, out TimeSpan end) : bool
    TryParseClock(string text, out TimeSpan time) : bool
    TryRepairTimes(ref TimeSpan start, ref TimeSpan end, out string note) : bool (private)
    SplitCourseCode(string raw) : (string, string) (private)
    ParseRoom(string text) : Room? (private)
    BuildColumnMap(string[] header) : Dictionary<string,int> (private)
    */
    public static class SyllabusRowMapper
    {
        // Header aliases. Header text is lower-cased and stripped of non-letters before matching,
        // so "Instructor 2025" matches "instructor" and "Enroll #" matches nothing (ignored).
        private static readonly Dictionary<string, string[]> Aliases = new()
        {
            ["code"] = new[] { "coursecode", "code", "course", "coursenumber", "crn", "id" },
            ["title"] = new[] { "title", "coursetitle", "coursename", "name" },
            ["credits"] = new[] { "credits", "credithours", "units", "cr" },
            ["department"] = new[] { "department", "dept" },
            ["section"] = new[] { "section", "sectionid", "sec" },
            ["lecturer"] = new[] { "lecturer", "instructor", "professor", "faculty", "teacher" },
            ["days"] = new[] { "days", "day", "meetingdays" },
            ["time"] = new[] { "time", "times", "meetingtime", "hours" },
            ["start"] = new[] { "start", "starttime", "begin" },
            ["end"] = new[] { "end", "endtime", "finish" },
            ["room"] = new[] { "room", "location", "roomnumber", "building" },
        };

        private static readonly Regex DayToken = new(
            "MONDAY|TUESDAY|WEDNESDAY|THURSDAY|FRIDAY|SATURDAY|SUNDAY|MON|TUE|WED|THU|FRI|SAT|SUN|TH|TU|SA|SU|M|T|W|R|F|S|U",
            RegexOptions.Compiled);

        private static readonly Regex TimeRange = new(
            @"(\d{1,2})(?::(\d{2}))?\s*([ap]m?)?\s*(?:-|–|—|to)\s*(\d{1,2})(?::(\d{2}))?\s*([ap]m?)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // "CTEC 350.170" -> code "CTEC 350", section "170".
        private static readonly Regex CodeWithSection =
            new(@"^(?<code>.+?)\s*\.\s*(?<sec>\d+)$", RegexOptions.Compiled);

        // Trailing session tag in a title, e.g. "Database Administration (7R2)".
        private static readonly Regex TitleSession =
            new(@"\s*\(\s*(?<s>\d+[A-Za-z]\d*)\s*\)\s*$", RegexOptions.Compiled);

        // Days cell that says the course has no fixed meeting times.
        private static readonly Regex AsyncText =
            new(@"asynchronous|on-?line|\bTBA\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Finds the first row that looks like a schedule header (course code, lecturer, days, and a time column or start/end columns).
        Variable List:
        IReadOnlyList<string[]> rows - table cells, row by row
        int (return) - index of the header row, or -1 if none
        */
        public static int FindHeaderRow(IReadOnlyList<string[]> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var map = BuildColumnMap(rows[i]);
                bool hasTime = map.ContainsKey("time") || (map.ContainsKey("start") && map.ContainsKey("end"));
                if (map.ContainsKey("code") && map.ContainsKey("lecturer") && map.ContainsKey("days") && hasTime)
                    return i;
            }
            return -1;
        }

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Finds the header row in a grid of cells and maps the rows below it. Used by the CSV, JSON and Excel parsers.
        Variable List:
        IReadOnlyList<string[]> rows - cell grid; the first matching row is the header
        ParseResult<List<CourseSection>> (return) - sections and warnings
        */
        public static ParseResult<List<CourseSection>> MapRows(IReadOnlyList<string[]> rows)
        {
            int header = FindHeaderRow(rows);
            if (header < 0)
                return ParseResult<List<CourseSection>>.Failure(
                    "No schedule header found. Expected columns: course code (or ID), lecturer (or Instructor), days, and times (or Begin/End).", new());
            return MapTable(rows, header);
        }

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Converts rows after the header into sections. Splits "CTEC 350.170" into code and section, pulls the session tag out of the title, keeps asynchronous courses as sections with no meetings, and repairs or flags bad values instead of skipping rows. Rows with the same code, section and session merge into one section.
        Variable List:
        IReadOnlyList<string[]> rows - table cells, row by row
        int headerIndex - index of the header row
        ParseResult<List<CourseSection>> (return) - sections plus warnings; Line is the 1-based row number in the source table
        */
        public static ParseResult<List<CourseSection>> MapTable(IReadOnlyList<string[]> rows, int headerIndex)
        {
            var result = new ParseResult<List<CourseSection>> { Data = new() };
            var map = BuildColumnMap(rows[headerIndex]);
            var byKey = new Dictionary<string, CourseSection>();

            string Cell(string[] row, string field) =>
                map.TryGetValue(field, out int c) && c < row.Length ? row[c].Trim() : "";

            for (int i = headerIndex + 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.All(string.IsNullOrWhiteSpace)) continue;   // truly blank rows carry no data
                int line = i + 1;
                var notes = new List<string>();

                // --- Course code and section number ---
                var (code, codeSection) = SplitCourseCode(Cell(row, "code"));
                if (code.Length == 0)
                {
                    code = $"UNKNOWN-R{line}";   // unique per row so it never merges with another row
                    notes.Add("Course code missing in source.");
                }
                string sectionId = Cell(row, "section").Length > 0 ? Cell(row, "section") : codeSection;

                // --- Title and session tag ---
                string rawTitle = Cell(row, "title");
                var titleMatch = TitleSession.Match(rawTitle);
                string title = titleMatch.Success ? TitleSession.Replace(rawTitle, "") : rawTitle;
                string session = titleMatch.Success ? titleMatch.Groups["s"].Value.ToUpperInvariant() : "";

                // --- Meeting times ---
                var meetings = new List<MeetingTime>();
                string daysText = Cell(row, "days");

                if (!AsyncText.IsMatch(daysText))   // asynchronous courses legitimately have no meetings
                {
                    var days = ParseDays(daysText);
                    TimeSpan start = default, end = default;
                    bool parsed = map.ContainsKey("time")
                        ? TryParseTimeRange(Cell(row, "time"), out start, out end)
                        : TryParseClock(Cell(row, "start"), out start) & TryParseClock(Cell(row, "end"), out end);
                    string raw = map.ContainsKey("time") ? Cell(row, "time") : $"{Cell(row, "start")} to {Cell(row, "end")}";

                    if (days.Count == 0)
                        notes.Add($"Could not read days '{daysText}'.");
                    else if (!parsed)
                        notes.Add($"Could not read times '{raw}'.");
                    else
                    {
                        // A reversed or empty range is a common typing mistake: try to repair it.
                        if (end <= start)
                        {
                            if (TryRepairTimes(ref start, ref end, out var fix)) notes.Add(fix);
                            else notes.Add($"Invalid times '{raw}' (end is not after start); no meeting times recorded.");
                        }
                        if (end > start)
                            meetings = days.Select(d => new MeetingTime { Day = d, Start = start, End = end }).ToList();
                    }
                }

                // --- Merge rows describing the same section, otherwise create it ---
                string key = $"{code}|{sectionId}|{session}".ToUpperInvariant();
                if (!byKey.TryGetValue(key, out var section))
                {
                    int.TryParse(Cell(row, "credits"), out int credits);
                    section = new CourseSection
                    {
                        Course = new Course { Code = code, Title = title, Credits = credits, Department = Cell(row, "department") },
                        SectionId = sectionId,
                        LecturerId = Cell(row, "lecturer"),
                        Room = ParseRoom(Cell(row, "room")),
                        Session = session,
                    };
                    byKey[key] = section;
                    result.Data.Add(section);
                }
                section.Meetings.AddRange(meetings);

                // Record each note on the section (for output) and as a warning (for the log).
                foreach (var n in notes)
                {
                    section.Notes.Add(n);
                    result.Errors.Add(new ParseError
                    {
                        Line = line,
                        Field = "row",
                        Severity = ParseSeverity.Warning,
                        Message = $"{code} {sectionId}: {n}",
                    });
                }
            }
            return result;
        }

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Parses day text such as "MWF", "TR", "TTh" or "Mon/Wed". Returns an empty list unless the ENTIRE text is recognized, so text like "Online" is never misread as days. T=Tuesday, R or Th=Thursday, S=Saturday, U=Sunday.
        Variable List:
        string text - day cell text
        List<DayOfWeek> (return) - distinct days, empty if the text is not purely days
        */
        public static List<DayOfWeek> ParseDays(string text)
        {
            string s = Regex.Replace(text.ToUpperInvariant(), "[^A-Z]", "");
            var days = new List<DayOfWeek>();
            int consumed = 0;

            foreach (Match m in DayToken.Matches(s))
            {
                consumed += m.Length;
                DayOfWeek d = m.Value switch
                {
                    "MONDAY" or "MON" or "M" => DayOfWeek.Monday,
                    "TUESDAY" or "TUE" or "TU" or "T" => DayOfWeek.Tuesday,
                    "WEDNESDAY" or "WED" or "W" => DayOfWeek.Wednesday,
                    "THURSDAY" or "THU" or "TH" or "R" => DayOfWeek.Thursday,
                    "FRIDAY" or "FRI" or "F" => DayOfWeek.Friday,
                    "SATURDAY" or "SAT" or "SA" or "S" => DayOfWeek.Saturday,
                    _ => DayOfWeek.Sunday,
                };
                if (!days.Contains(d)) days.Add(d);
            }
            // Leftover characters mean the cell is not a day list.
            return consumed == s.Length ? days : new List<DayOfWeek>();
        }

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Parses ranges such as "9:00-10:15", "9:00 AM - 10:15 AM", or "1-2:30 PM". If only the end has AM/PM, the start inherits it unless that would put the start after the end. Returns true when a range was READ; callers check end > start and may repair it.
        Variable List:
        string text - time cell text
        out TimeSpan start, end - parsed times
        bool (return) - true if a range was read
        */
        public static bool TryParseTimeRange(string text, out TimeSpan start, out TimeSpan end)
        {
            start = end = default;
            var m = TimeRange.Match(text);
            if (!m.Success) return false;

            string startMer = m.Groups[3].Value, endMer = m.Groups[6].Value;
            TimeSpan To24(string h, string min, string mer)
            {
                int hour = int.Parse(h) % 12;
                if (mer.StartsWith("p", StringComparison.OrdinalIgnoreCase)) hour += 12;
                return new TimeSpan(hour, min.Length > 0 ? int.Parse(min) : 0, 0);
            }

            if (startMer.Length > 0 || endMer.Length > 0)
            {
                end = To24(m.Groups[4].Value, m.Groups[5].Value, endMer);
                string effective = startMer.Length > 0 ? startMer : endMer;
                start = To24(m.Groups[1].Value, m.Groups[2].Value, effective);
                // "11-12:15 PM": the start cannot be PM if that puts it after the end, so flip it.
                if (startMer.Length == 0 && start >= end)
                    start = To24(m.Groups[1].Value, m.Groups[2].Value,
                                 effective.StartsWith("p", StringComparison.OrdinalIgnoreCase) ? "am" : "pm");
            }
            else
            {
                // No AM/PM anywhere: treat as 24-hour time.
                start = new TimeSpan(int.Parse(m.Groups[1].Value), m.Groups[2].Length > 0 ? int.Parse(m.Groups[2].Value) : 0, 0);
                end = new TimeSpan(int.Parse(m.Groups[4].Value), m.Groups[5].Length > 0 ? int.Parse(m.Groups[5].Value) : 0, 0);
            }
            return true;
        }

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Parses a single clock time such as "13:30" or "1:30 PM".
        Variable List:
        string text - cell text
        out TimeSpan time - parsed time of day
        bool (return) - true if parsed
        */
        public static bool TryParseClock(string text, out TimeSpan time)
        {
            if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out time)) return true;
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                time = dt.TimeOfDay;
                return true;
            }
            time = default;
            return false;
        }

        /*
        Class Name: SyllabusRowMapper
        Module Name: Input Handling
        Description: Attempts to repair a time range whose end is not after its start. If the end is in the morning, assumes it was meant to be PM, accepted only if the class is then 8 hours or less. Equal times cannot be repaired.
        Variable List:
        ref TimeSpan start - class start (unchanged)
        ref TimeSpan end - class end; replaced with the repaired value on success
        out string note - description of the repair, for the user to verify
        bool (return) - true if the range was repaired
        */
        internal static bool TryRepairTimes(ref TimeSpan start, ref TimeSpan end, out string note)
        {
            note = "";
            if (end == start) return false;   // zero-length class: nothing to infer

            var flipped = end + TimeSpan.FromHours(12);
            bool plausible = end < TimeSpan.FromHours(12) && flipped > start && flipped - start <= TimeSpan.FromHours(8);
            if (!plausible) return false;

            note = $"End time {Clock(end)} is before start {Clock(start)}; assumed {Clock(flipped)}. Please verify.";
            end = flipped;
            return true;
        }

        // Short 12-hour display used in repair messages.
        private static string Clock(TimeSpan t)
            => DateTime.Today.Add(t).ToString("h:mm tt", CultureInfo.InvariantCulture);

        // Splits "CTEC 350.170" into ("CTEC 350", "170"); codes without a suffix return an empty section.
        private static (string Code, string Section) SplitCourseCode(string raw)
        {
            var m = CodeWithSection.Match(raw.Trim());
            return m.Success ? (m.Groups["code"].Value.Trim(), m.Groups["sec"].Value) : (raw.Trim(), "");
        }

        // "CS 313" -> building "CS", number "313"; "108", "Online", "201/112" stay as a number only.
        private static Room? ParseRoom(string text)
        {
            if (text.Length == 0) return null;
            var m = Regex.Match(text, @"^(?<b>[A-Za-z]+)\s+(?<n>\S+)$");
            return m.Success
                ? new Room { Building = m.Groups["b"].Value, Number = m.Groups["n"].Value }
                : new Room { Number = text };
        }

        // Matches header cells to known fields, ignoring case, spaces, digits and punctuation.
        private static Dictionary<string, int> BuildColumnMap(string[] header)
        {
            var map = new Dictionary<string, int>();
            for (int c = 0; c < header.Length; c++)
            {
                string h = Regex.Replace(header[c].ToLowerInvariant(), "[^a-z]", "");
                foreach (var (field, names) in Aliases)
                    if (!map.ContainsKey(field) && names.Contains(h)) { map[field] = c; break; }
            }
            return map;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ClassScheduler.Models;

namespace ClassScheduler.Input
{
    /*
    Class Name: SyllabusHeaderReader
    Module Name: Input Handling
    Description: Reads a single-course syllabus whose schedule is written as labeled lines ("Instructor:", "Class Hours:", "Office Hours:") instead of a table. Used by the Word parser when no schedule table is found.
    Method List:
    Read(IEnumerable<string> paragraphs) : ParseResult<List<CourseSection>>
    TryParseDayTime(string text, out List<DayOfWeek> days, out TimeSpan start, out TimeSpan end) : bool (private)
    */
    public static class SyllabusHeaderReader
    {
        // "COSC 369 (3 Credit) Software Engineering I Spring Semester 2026 ..."
        private static readonly Regex CourseLine = new(
            @"^(?<code>[A-Za-z]{2,5}\s*\d{3}[A-Za-z]?)\s*\(\s*(?<cr>\d+)\s*credits?\s*\)\s*(?<title>.+?)\s*(?:(?:Spring|Summer|Fall|Winter)\b.*)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // "Instructor: Dr. Appolo Tankeh"
        private static readonly Regex Field = new(
            @"^(?<k>Instructor|Professor|Office Location|Email|Class Hours|Office Hours)\s*:\s*(?<v>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // "Tue/Thursday: 9:30 AM to 10:45 AM" -> days part, time part
        private static readonly Regex DayTime = new(
            @"^(?<d>[^0-9]*?)\s*:?\s*(?<t>\d.*)$", RegexOptions.Compiled);
        // "Department of Computer Science" -> "Computer Science"
        private static readonly Regex DepartmentLine = new(
            @"^Department of (?<d>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /*
        Class Name: SyllabusHeaderReader
        Module Name: Input Handling
        Description: Reads professor information from a syllabus (Word or PDF text): name, email, department, office location, stated office hours, and the course code from the title line. Class days, times and rooms are deliberately ignored; they come from the schedule Excel.
        Variable List:
        IEnumerable<string> paragraphs - paragraph text in document order
        ParseResult<List<Professor>> (return) - one professor, or an error if no Instructor line exists
        */
        public static ParseResult<List<Professor>> ReadProfessor(IEnumerable<string> paragraphs)
        {
            var result = new ParseResult<List<Professor>> { Data = new() };
            var lines = paragraphs.Select(p => Regex.Replace(p, @"\s+", " ").Trim()).Where(p => p.Length > 0).ToList();

            var (name, email) = FindInstructor(lines);
            if (name.Length == 0)
                return ParseResult<List<Professor>>.Failure("No 'Instructor:' line found in the syllabus.", new());

            var prof = new Professor
            {
                Id = ClassSchedulerCommon.Common.NameNormalizer.Normalize(name),
                Name = name,
                Email = email,
            };

            foreach (var line in lines)
            {
                // Department heading.
                var d = DepartmentLine.Match(line);
                if (d.Success && prof.Department.Length == 0) prof.Department = d.Groups["d"].Value.Trim();

                // Course taught, from the title line (only the code is kept).
                var c = CourseLine.Match(line);
                if (c.Success && prof.CourseCodes.Count == 0)
                    prof.CourseCodes.Add(Regex.Replace(c.Groups["code"].Value, @"^([A-Za-z]+)\s*(\d)", "$1 $2").ToUpperInvariant());
            }

            // Office hours stated in the syllabus become the availability window.
            var oh = lines.Select(l => Field.Match(l))
                          .FirstOrDefault(m => m.Success && m.Groups["k"].Value.Equals("Office Hours", StringComparison.OrdinalIgnoreCase));
            if (oh is not null)
            {
                if (TryParseDayTime(oh.Groups["v"].Value, out var days, out var start, out var end))
                    prof.AvailabilityTable = days.Select(x => new TimeBlock { Day = x, Start = start, End = end }).ToList();
                else
                    result.Errors.Add(new ParseError
                    {
                        Line = 0,
                        Field = "Office Hours",
                        Severity = ParseSeverity.Warning,
                        Message = $"{name}: could not read office hours '{oh.Groups["v"].Value}'; ignored."
                    });
            }

            result.Data.Add(prof);
            return result;
        }

        /*
        Class Name: SyllabusHeaderReader
        Module Name: Input Handling
        Description: Finds the course line and the labeled fields among the document's paragraphs and builds one CourseSection. Only the first occurrence of each field is used. Whitespace (including tabs) is collapsed first, so tab-separated labels work. Unreadable class hours keep the section with no meetings and a note.
        Variable List:
        IEnumerable<string> paragraphs - paragraph text in document order
        ParseResult<List<CourseSection>> (return) - zero or one section, plus warnings
        */
        public static ParseResult<List<CourseSection>> Read(IEnumerable<string> paragraphs)
        {
            var result = new ParseResult<List<CourseSection>> { Data = new() };
            Match? course = null;
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in paragraphs)
            {
                string text = Regex.Replace(raw, @"\s+", " ").Trim();
                if (text.Length == 0) continue;

                if (course is null)
                {
                    var c = CourseLine.Match(text);
                    if (c.Success) { course = c; continue; }
                }
                var f = Field.Match(text);
                if (f.Success) fields.TryAdd(f.Groups["k"].Value, f.Groups["v"].Value.Trim());
            }

            if (course is null) return result;   // not a header-style syllabus; caller reports the error

            // Normalize "COSC369" / "cosc  369" to "COSC 369".
            string code = Regex.Replace(course.Groups["code"].Value, @"^([A-Za-z]+)\s*(\d)", "$1 $2").ToUpperInvariant();
            int.TryParse(course.Groups["cr"].Value, out int credits);

            var section = new CourseSection
            {
                Course = new Course { Code = code, Title = course.Groups["title"].Value.Trim(), Credits = credits },
                LecturerId = fields.GetValueOrDefault("Instructor") ?? fields.GetValueOrDefault("Professor") ?? "",
                LecturerEmail = fields.GetValueOrDefault("Email") ?? "",
            };

            // Class meeting times.
            if (fields.TryGetValue("Class Hours", out var classHours) &&
                TryParseDayTime(classHours, out var days, out var start, out var end))
            {
                section.Meetings = days.Select(d => new MeetingTime { Day = d, Start = start, End = end }).ToList();
            }
            else
            {
                string why = fields.ContainsKey("Class Hours")
                    ? $"Could not read class hours '{fields["Class Hours"]}'."
                    : "No 'Class Hours' line found.";
                section.Notes.Add(why);
                result.Errors.Add(new ParseError { Line = 0, Field = "Class Hours", Severity = ParseSeverity.Warning, Message = $"{code}: {why}" });
            }

            // Office hours stated by the lecturer become their availability window.
            if (fields.TryGetValue("Office Hours", out var oh))
            {
                if (TryParseDayTime(oh, out var ohDays, out var ohStart, out var ohEnd))
                    section.LecturerOfficeHours = ohDays.Select(d => new TimeBlock { Day = d, Start = ohStart, End = ohEnd }).ToList();
                else
                    result.Errors.Add(new ParseError
                    {
                        Line = 0,
                        Field = "Office Hours",
                        Severity = ParseSeverity.Warning,
                        Message = $"{code}: could not read office hours '{oh}'; ignored."
                    });
            }

            // The syllabus gives an office, not a classroom: use it as the room, but say so.
            if (fields.TryGetValue("Office Location", out var loc))
            {
                var m = Regex.Match(loc, @"^(?<b>[A-Za-z]+)\s+(?<n>\S+)$");
                section.Room = m.Success ? new Room { Building = m.Groups["b"].Value, Number = m.Groups["n"].Value } : new Room { Number = loc };
                result.Errors.Add(new ParseError
                {
                    Line = 0,
                    Field = "Office Location",
                    Severity = ParseSeverity.Warning,
                    Message = $"{code}: no classroom listed; using office location '{loc}' as the room."
                });
            }

            result.Data.Add(section);
            return result;
        }

        /*
        Class Name: SyllabusHeaderReader
        Module Name: Input Handling
        Description: Splits "Tue/Thursday: 9:30 AM to 10:45 AM" into days and a time range, repairing a reversed range when SyllabusRowMapper can.
        Variable List:
        string text - field value
        out List<DayOfWeek> days - parsed days
        out TimeSpan start, end - parsed times
        bool (return) - true if days and a valid range were read
        */
        private static bool TryParseDayTime(string text, out List<DayOfWeek> days, out TimeSpan start, out TimeSpan end)
        {
            days = new(); start = end = default;
            var m = DayTime.Match(text);
            if (!m.Success) return false;

            days = SyllabusRowMapper.ParseDays(m.Groups["d"].Value);
            if (days.Count == 0 || !SyllabusRowMapper.TryParseTimeRange(m.Groups["t"].Value, out start, out end)) return false;

            if (end <= start && !SyllabusRowMapper.TryRepairTimes(ref start, ref end, out _)) return false;
            return end > start;
        }
        /*
        Class Name: SyllabusHeaderReader
        Module Name: Input Handling
        Description: Finds the first "Instructor:" (or "Professor:") and "Email:" lines in a document.
        Variable List:
        IEnumerable<string> paragraphs - paragraph text in document order
        (string Name, string Email) (return) - empty strings if not found
        */
        public static (string Name, string Email) FindInstructor(IEnumerable<string> paragraphs)
        {
            string name = "", email = "";
            foreach (var raw in paragraphs)
            {
                // Collapse tabs and repeated spaces so "Instructor:<tab>Dr. X" matches the Field regex.
                var f = Field.Match(Regex.Replace(raw, @"\s+", " ").Trim());
                if (!f.Success) continue;

                string key = f.Groups["k"].Value.ToLowerInvariant();
                if (name.Length == 0 && (key == "instructor" || key == "professor")) name = f.Groups["v"].Value.Trim();
                if (email.Length == 0 && key == "email") email = f.Groups["v"].Value.Trim();
            }
            return (name, email);
        }
    }
}

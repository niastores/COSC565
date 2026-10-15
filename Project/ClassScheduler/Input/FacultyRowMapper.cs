using System;
using System.Collections.Generic;
using System.Text;
using ClassSchedulerCommon.Common;
using ClassScheduler.Models;
using System.Text.RegularExpressions;

namespace ClassScheduler.Input
{
    /*
    Class Name: FacultyRowMapper
    Module Name: Input Handling
    Description: Converts a grid of cells from the faculty file into Professor objects. Expected columns (aliases accepted): Id, Name, Department, Email, Courses (separated by ; or ,), OfficeHours (hours per week), Availability (e.g. "Mon/Wed 9:00-12:00; Fri 1:00 PM-3:00 PM"). Rows are never dropped: missing names and duplicate ids are repaired and reported as warnings.
    Method List:
    MapRows(IReadOnlyList<string[]> rows) : ParseResult<List<Professor>>
    ParseAvailability(string text, int line, List<ParseError> errors) : List<TimeBlock> (private)
    BuildColumnMap(string[] header) : Dictionary<string,int> (private)
    */
    public static class FacultyRowMapper
    {
        private static readonly Dictionary<string, string[]> Aliases = new()
        {
            ["id"] = new[] { "id", "facultyid", "employeeid", "lecturerid" },
            ["name"] = new[] { "name", "fullname", "professor", "lecturer", "instructor" },
            ["department"] = new[] { "department", "dept" },
            ["email"] = new[] { "email", "emailaddress" },
            ["courses"] = new[] { "courses", "coursestaught", "coursecodes", "taught" },
            ["hours"] = new[] { "officehours", "requiredofficehours", "officehoursperweek", "hours" },
            ["availability"] = new[] { "availability", "available", "officehouravailability" },
        };

        /*
        Class Name: FacultyRowMapper
        Module Name: Input Handling
        Description: Finds the header row (first row with a name column) and builds one Professor per data row. A missing Id falls back to the normalized name; a missing name falls back to the id or email; a duplicate id gets a numeric suffix.
        Variable List:
        IReadOnlyList<string[]> rows - cell grid
        ParseResult<List<Professor>> (return) - professors and warnings
        */
        public static ParseResult<List<Professor>> MapRows(IReadOnlyList<string[]> rows)
        {
            var result = new ParseResult<List<Professor>> { Data = new() };

            // Locate the header row.
            int header = -1;
            for (int i = 0; i < rows.Count && header < 0; i++)
                if (BuildColumnMap(rows[i]).ContainsKey("name")) header = i;
            if (header < 0)
                return ParseResult<List<Professor>>.Failure("No faculty header found. A 'Name' column is required.", new());

            var map = BuildColumnMap(rows[header]);
            string Cell(string[] r, string f) => map.TryGetValue(f, out int c) && c < r.Length ? r[c].Trim() : "";

            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = header + 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.All(string.IsNullOrWhiteSpace)) continue;
                int line = i + 1;

                // Keep the row even without a name: fall back to the id or email.
                string name = Cell(row, "name");
                if (name.Length == 0)
                {
                    name = Cell(row, "id").Length > 0 ? Cell(row, "id") : Cell(row, "email");
                    if (name.Length == 0) name = $"Unnamed professor (row {line})";
                    result.Errors.Add(new ParseError
                    {
                        Line = line,
                        Field = "name",
                        Severity = ParseSeverity.Warning,
                        Message = $"Missing professor name; using '{name}'."
                    });
                }

                string id = Cell(row, "id");
                if (id.Length == 0) id = NameNormalizer.Normalize(name);

                // A duplicate id would merge two people; keep both by adding a suffix.
                string baseId = id;
                for (int n = 2; !seenIds.Add(id); n++) id = $"{baseId}#{n}";
                if (id != baseId)
                    result.Errors.Add(new ParseError
                    {
                        Line = line,
                        Field = "id",
                        Severity = ParseSeverity.Warning,
                        Message = $"Duplicate id '{baseId}'; this row was given id '{id}'."
                    });

                int.TryParse(Cell(row, "hours"), out int hours);   // 0 => policy default is used later

                result.Data.Add(new Professor
                {
                    Id = id,
                    Name = name,
                    Department = Cell(row, "department"),
                    Email = Cell(row, "email"),
                    CourseCodes = Cell(row, "courses")
                        .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList(),
                    RequiredOfficeHoursPerWeek = hours,
                    AvailabilityTable = ParseAvailability(Cell(row, "availability"), line, result.Errors),
                });
            }
            return result;
        }

        /*
        Class Name: FacultyRowMapper
        Module Name: Input Handling
        Description: Parses availability text. Entries are separated by ';'. Each entry is days then a time range; the text is split at the first digit so "AM"/"PM" is never mistaken for a day. An unreadable entry is reported and ignored, which leaves the professor more available, not less.
        Variable List:
        string text - availability cell
        int line - source line for error reporting
        List<ParseError> errors - receives problems with individual entries
        List<TimeBlock> (return) - one block per day per entry
        */
        private static List<TimeBlock> ParseAvailability(string text, int line, List<ParseError> errors)
        {
            var blocks = new List<TimeBlock>();
            foreach (var entry in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int digit = entry.IndexOfAny("0123456789".ToCharArray());
                var days = digit > 0 ? SyllabusRowMapper.ParseDays(entry[..digit]) : new List<DayOfWeek>();

                if (days.Count == 0 ||
                    !SyllabusRowMapper.TryParseTimeRange(entry[digit..], out var start, out var end) || end <= start)
                {
                    errors.Add(new ParseError
                    {
                        Line = line,
                        Field = "availability",
                        Severity = ParseSeverity.Warning,
                        Message = $"Could not read availability '{entry}'; ignored."
                    });
                    continue;
                }
                foreach (var d in days)
                    blocks.Add(new TimeBlock { Day = d, Start = start, End = end });
            }
            return blocks;
        }

        // Matches header cells to known fields, ignoring case, spaces and punctuation.
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

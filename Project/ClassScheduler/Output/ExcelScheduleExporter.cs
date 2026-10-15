using ClassScheduler.Models;
using ClosedXML.Excel;

namespace ClassScheduler.Output
{
    /*
    Class Name: ExcelScheduleExporter
    Module Name: Output
    Description: Exports schedules to an Excel workbook with one worksheet per professor.
    Method List:
    Export(IEnumerable<ProfessorSchedule> s, string path) : void
    UniqueSheetName(string name, HashSet<string> used) : string (private)
    */
    public class ExcelScheduleExporter : IScheduleExporter
    {
        /*
        Class Name: ExcelScheduleExporter
        Module Name: Output
        Description: Writes the class table (Course, Title, Times, Location) and the office-hour lines on each sheet.
        Variable List:
        IEnumerable<ProfessorSchedule> s - schedules to export
        string path - output .xlsx path
        */
        public void Export(IEnumerable<ProfessorSchedule> s, string path)
        {
            using var wb = new XLWorkbook();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var sched in s)
            {
                var ws = wb.Worksheets.Add(UniqueSheetName(sched.Professor.Name, used));
                int r = 1;
                ws.Cell(r, 1).Value = sched.Professor.Name;
                ws.Cell(r, 1).Style.Font.Bold = true;
                r += 2;
                ws.Cell(r++, 1).Value = "CLASS SCHEDULE";
                foreach (var (h, c) in new[] { "Course", "Title", "Times", "Location" }.Select((h, i) => (h, i + 1)))
                {
                    ws.Cell(r, c).Value = h;
                    ws.Cell(r, c).Style.Font.Bold = true;
                }
                r++;
                foreach (var sec in sched.Teaching.OrderBy(t => t.Course.Code))
                {
                    ws.Cell(r, 1).Value = sec.Course.Code;
                    ws.Cell(r, 2).Value = sec.Course.Title;
                    ws.Cell(r, 3).Value = string.Join("\n", ScheduleText.FormatClassTimes(sec));   // one line per time group
                    ws.Cell(r, 3).Style.Alignment.WrapText = true;
                    ws.Cell(r, 4).Value = ScheduleText.FormatLocation(sec.Room);
                    r++;
                }
                r++;
                ws.Cell(r++, 1).Value = "OFFICE HOURS";
                var lines = ScheduleText.FormatOfficeHours(sched);
                //if (lines.Count == 0) ws.Cell(r, 1).Value = "No office hours scheduled.";
                foreach (var line in lines) ws.Cell(r++, 1).Value = line;
                ws.Columns().AdjustToContents();
            }
            // A workbook with no sheets cannot be saved.
            if (!wb.Worksheets.Any()) wb.Worksheets.Add("Empty");
            wb.SaveAs(path);
        }
        // Excel sheet names: max 31 chars, no : \ / ? * [ ], must be unique.
        private static string UniqueSheetName(string name, HashSet<string> used)
        {
            string clean = new string(name.Where(c => !":\\/?*[]".Contains(c)).ToArray()).Trim();
            if (clean.Length == 0) clean = "Sheet";
            if (clean.Length > 31) clean = clean[..31];
            string candidate = clean;
            for (int n = 2; !used.Add(candidate); n++)
            {
                string suffix = $" ({n})";
                candidate = clean[..Math.Min(clean.Length, 31 - suffix.Length)] + suffix;
            }
            return candidate;
        }
    }

}
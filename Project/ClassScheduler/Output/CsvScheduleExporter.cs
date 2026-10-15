using ClassScheduler.Models;
using System.Text;

namespace ClassScheduler.Output
{
    /*
    Class Name: CsvScheduleExporter
    Module Name: Output
    Description: Exports schedules as one CSV, one row per class meeting or office-hour slot.
    Method List:
    Export(IEnumerable<ProfessorSchedule> s, string path) : void
    Escape(string value) : string (private)
    */
    public class CsvScheduleExporter : IScheduleExporter
    {
        /*
        Class Name: CsvScheduleExporter
        Module Name: Output
        Description: Writes Professor, Type (Class/OfficeHours), Course, Title, Section, Day, Start, End, Location.
        Variable List:
        IEnumerable<ProfessorSchedule> s - schedules to export
        string path - output CSV path
        */
        public void Export(IEnumerable<ProfessorSchedule> s, string path)
        {
            using var w = new StreamWriter(path, false, new UTF8Encoding(true));   // BOM so Excel reads UTF-8
            w.WriteLine("Professor,Type,Course,Title,Section,Day,Start,End,Location");
            foreach (var sched in s)
            {
                string prof = sched.Professor.Name;
                foreach (var sec in sched.Teaching)
                    foreach (var m in sec.Meetings.OrderBy(m => ((int)m.Day + 6) % 7).ThenBy(m => m.Start))
                        w.WriteLine(string.Join(",", new[]
                        {
                        Escape(prof), "Class", Escape(sec.Course.Code), Escape(sec.Course.Title), Escape(sec.SectionId),
                        m.Day.ToString(), ScheduleText.FormatTime(m.Start), ScheduleText.FormatTime(m.End),
                        Escape(ScheduleText.FormatLocation(sec.Room)),
                    }));
                foreach (var o in sched.OfficeHours.OrderBy(o => ((int)o.Block.Day + 6) % 7).ThenBy(o => o.Block.Start))
                    w.WriteLine(string.Join(",", new[]
                    {
                    Escape(prof), "OfficeHours", Escape(o.CourseCode ?? ""), "", "",
                    o.Block.Day.ToString(), ScheduleText.FormatTime(o.Block.Start), ScheduleText.FormatTime(o.Block.End),
                    Escape(ScheduleText.FormatLocation(o.Location)),
                }));
            }
        }
        // Quotes a value if it contains a comma, quote or newline; doubles embedded quotes.
        private static string Escape(string value)
            => value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
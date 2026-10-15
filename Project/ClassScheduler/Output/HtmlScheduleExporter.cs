using System.Net;
using System.Text;
using ClassScheduler.Models;

namespace ClassScheduler.Output
{
    /*
    Class Name: HtmlScheduleExporter
    Module Name: Output
    Description: Exports schedules as one HTML page with a section per professor.
    Method List:
    Export(IEnumerable<ProfessorSchedule> s, string path) : void
    */
    public class HtmlScheduleExporter : IScheduleExporter
    {
        private readonly string _note;
        /*
        Class Name: HtmlScheduleExporter
        Module Name: Output
        Description: Stores the footer note shown under each professor.
        Variable List:
        string note - note text
        */
        public HtmlScheduleExporter(string note = "") => _note = note;
        /*
        Class Name: HtmlScheduleExporter
        Module Name: Output
        Description: Writes a class table and an office-hours list per professor. All text is HTML-encoded.
        Variable List:
        IEnumerable<ProfessorSchedule> s - schedules to export
        string path - output HTML path
        */
        public void Export(IEnumerable<ProfessorSchedule> s, string path)
        {
            string E(string t) => WebUtility.HtmlEncode(t);   // encode everything that came from input files
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Faculty Schedules</title>");
            sb.AppendLine("<style>body{font-family:sans-serif;margin:2em}table{border-collapse:collapse;width:100%}" +
                          "td,th{border:1px solid #333;padding:6px;text-align:center}.oh{border:2px solid #333;padding:8px}" +
                          "section{page-break-after:always;margin-bottom:3em}</style></head><body>");
            foreach (var sched in s)
            {
                sb.AppendLine($"<section><h2>{E(sched.Professor.Name)}</h2><h3>CLASS SCHEDULE</h3>");
                sb.AppendLine("<table><tr><th>Course</th><th>Times</th><th>Location</th></tr>");
                foreach (var sec in sched.Teaching.OrderBy(t => t.Course.Code))
                {
                    string title = string.IsNullOrWhiteSpace(sec.Course.Title) ? "" : $"<br>({E(sec.Course.Title)})";
                    string times = string.Join("<br>", ScheduleText.FormatClassTimes(sec).Select(E));
                    sb.AppendLine($"<tr><td><b>{E(sec.Course.Code)}</b>{title}</td><td>{times}</td><td>{E(ScheduleText.FormatLocation(sec.Room))}</td></tr>");
                }
                sb.AppendLine("</table><h3>OFFICE HOURS</h3><div class=\"oh\">");
                var lines = ScheduleText.FormatOfficeHours(sched);
                sb.AppendLine(lines.Count == 0 ? "&nbsp;" : string.Join("<br>", lines.Select(E)));
                sb.AppendLine("</div>");
                if (_note.Length > 0) sb.AppendLine($"<p><b>{E(_note)}</b></p>");
                sb.AppendLine("</section>");
            }
            sb.AppendLine("</body></html>");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
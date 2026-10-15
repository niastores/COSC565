using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClassScheduler.Output
{
  
    /*
    Class Name: DocxScheduleExporter
    Module Name: Output
    Description: Writes each professor's schedule as a Word page matching the sample: CLASS SCHEDULE table, OFFICE HOURS box, and a note.
    Method List:
    DocxScheduleExporter(string note) : constructor
    Export(IEnumerable<ProfessorSchedule> s, string path) : void
    BuildClassTable(ProfessorSchedule s) : Table (private)
    BuildOfficeHoursBox(ProfessorSchedule s) : Table (private)
    Para(string text, bool bold, bool center, int halfPts) : Paragraph (private)
    Cell(int widthDxa, bool center, params Paragraph[] paras) : TableCell (private)
    Borders() : TableProperties (private)
    */
    public class DocxScheduleExporter : IScheduleExporter
    {
        private readonly string _note;

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Stores the footer note text.
        Variable List:
        string note - note shown under the office-hours box
        */
        public DocxScheduleExporter(string note) => _note = note;

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Creates the document and appends one page per professor.
        Variable List:
        IEnumerable<ProfessorSchedule> s - schedules to export
        string path - output .docx path
        */
        public void Export(IEnumerable<ProfessorSchedule> s, string path)
        {
            using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            var main = doc.AddMainDocumentPart();
            main.Document = new Document(new Body());
            var body = main.Document.Body!;

            bool first = true;
            foreach (var sched in s)
            {
                if (!first) body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
                first = false;

                body.Append(Para(sched.Professor.Name, true, false, 28));
                body.Append(Para("CLASS SCHEDULE", true, false, 24));
                body.Append(BuildClassTable(sched));
                body.Append(Para("", false, false, 24));
                body.Append(Para("OFFICE HOURS", true, false, 24));
                body.Append(BuildOfficeHoursBox(sched));
                body.Append(Para(_note, true, false, 22));
            }
            main.Document.Save();
        }

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Builds the 3-column table (Course, Times, Location) with one row per section.
        Variable List:
        ProfessorSchedule s - schedule to render
        Table (return) - bordered table
        */
        private Table BuildClassTable(ProfessorSchedule s)
        {
            var table = new Table(Borders());
            table.Append(new TableRow(
                Cell(2400, true, Para("Course", true, true, 28)),
                Cell(5600, true, Para("Times", true, true, 28)),
                Cell(1800, true, Para("Location", true, true, 28))));

            foreach (var sec in s.Teaching.OrderBy(t => t.Course.Code))
            {
                var course = new List<Paragraph> { Para(sec.Course.Code, true, true, 28) };
                if (!string.IsNullOrWhiteSpace(sec.Course.Title))
                    course.Add(Para($"({sec.Course.Title})", false, true, 24));

                var times = ScheduleText.FormatClassTimes(sec).Select(l => Para(l, true, false, 28)).ToArray();

                table.Append(new TableRow(
                    Cell(2400, true, course.ToArray()),
                    Cell(5600, false, times),
                    Cell(1800, true, Para(ScheduleText.FormatLocation(sec.Room), true, true, 28))));
            }
            return table;
        }

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Builds the single-cell bordered box holding the office-hour lines.
        Variable List:
        ProfessorSchedule s - schedule to render
        Table (return) - bordered one-cell table
        */
        private Table BuildOfficeHoursBox(ProfessorSchedule s)
        {
            var lines = ScheduleText.FormatOfficeHours(s);
            // Empty box when there are no office hours (keeps the layout of the sample).
            var paras = lines.Count == 0
                ? new[] { Para("", false, false, 28) }
                : lines.Select(l => Para(l, true, false, 32)).ToArray();
            return new Table(Borders(), new TableRow(Cell(9800, false, paras)));
        }

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Creates a paragraph with one run.
        Variable List:
        string text - paragraph text
        bool bold - bold on/off
        bool center - centered alignment
        int halfPts - font size in half-points (24 = 12 pt)
        Paragraph (return) - formatted paragraph
        */
        private static Paragraph Para(string text, bool bold, bool center, int halfPts)
        {
            var rp = new RunProperties();
            if (bold) rp.Append(new Bold());
            rp.Append(new FontSize { Val = halfPts.ToString() });
            var p = new Paragraph(new Run(rp, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
            if (center)
                p.PrependChild(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
            return p;
        }

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Creates a table cell with a fixed width.
        Variable List:
        int widthDxa - width in twentieths of a point
        bool center - vertically center the content
        Paragraph[] paras - cell content
        TableCell (return) - cell
        */
        private static TableCell Cell(int widthDxa, bool center, params Paragraph[] paras)
        {
            var props = new TableCellProperties(new TableCellWidth { Width = widthDxa.ToString(), Type = TableWidthUnitValues.Dxa });
            if (center) props.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
            var cell = new TableCell(props);
            cell.Append(paras);
            return cell;
        }

        /*
        Class Name: DocxScheduleExporter
        Module Name: Output
        Description: Table properties with single-line borders on all sides and between cells.
        Variable List:
        TableProperties (return) - border settings
        */
        private static TableProperties Borders()
            => new(new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 12 },
                new LeftBorder { Val = BorderValues.Single, Size = 12 },
                new BottomBorder { Val = BorderValues.Single, Size = 12 },
                new RightBorder { Val = BorderValues.Single, Size = 12 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }));
    }
}

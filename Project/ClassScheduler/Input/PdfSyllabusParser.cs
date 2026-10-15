using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ClassScheduler.Input
{

    /*
    Class Name: PdfSyllabusParser
    Module Name: Input Handling
    Description: Parses a text-based PDF syllabus by reconstructing table rows from word positions. Scanned (image-only) PDFs are not supported.
    Method List:
    Parse(string path) : ParseResult<List<CourseSection>>
    ExtractRows(Page page) : List<string[]> (private)
    */
    public class PdfSyllabusParser : IFileParser<List<CourseSection>>
    {
        private const double LineTolerance = 3.0;   // points; words within this vertical distance share a line
        private const double CellGap = 8.0;         // points; a gap wider than this starts a new cell

        /*
        Class Name: PdfSyllabusParser
        Module Name: Input Handling
        Description: Extracts rows from every page, finds the header row, and maps the rows that follow. Reports an error if no text or no schedule header is found.
        Variable List:
        string path - location of the .pdf file
        ParseResult<List<CourseSection>> (return) - sections and errors
        */
        public ParseResult<List<CourseSection>> Parse(string path)
        {
            var result = new ParseResult<List<CourseSection>> { Data = new() };
            try
            {
                using var pdf = PdfDocument.Open(path);
                var rows = pdf.GetPages().SelectMany(ExtractRows).ToList();

                if (rows.Count == 0)
                {
                    result.Errors.Add(new ParseError
                    {
                        Line = 0,
                        Field = "file",
                        Message = "No text found. The PDF may be scanned; OCR is required."
                    });
                    return result;
                }

                int header = SyllabusRowMapper.FindHeaderRow(rows);
                if (header < 0)
                {
                    result.Errors.Add(new ParseError
                    {
                        Line = 0,
                        Field = "table",
                        Message = "No schedule header found. Expected columns: course code, lecturer, days, time."
                    });
                    return result;
                }

                var mapped = SyllabusRowMapper.MapTable(rows, header);
                result.Data.AddRange(mapped.Data);
                result.Errors.AddRange(mapped.Errors);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ParseError { Line = 0, Field = "file", Message = $"Cannot read PDF: {ex.Message}" });
            }
            return result;
        }

        /*
        Class Name: PdfSyllabusParser
        Module Name: Input Handling
        Description: Groups a page's words into lines by vertical position (top to bottom), then splits each line into cells at wide horizontal gaps.
        Variable List:
        Page page - PDF page to read
        List<string[]> (return) - one array of cell texts per line
        */
        internal static List<string[]> ExtractRows(Page page)
        {
            var lines = new List<List<Word>>();
            foreach (var w in page.GetWords().OrderByDescending(w => w.BoundingBox.Bottom))
            {
                var line = lines.FirstOrDefault(l => Math.Abs(l[0].BoundingBox.Bottom - w.BoundingBox.Bottom) <= LineTolerance);
                if (line is null) lines.Add(new List<Word> { w });
                else line.Add(w);
            }

            var rows = new List<string[]>();
            foreach (var line in lines)
            {
                var cells = new List<string>();
                var current = new List<string>();
                Word? prev = null;
                foreach (var w in line.OrderBy(w => w.BoundingBox.Left))
                {
                    if (prev is not null && w.BoundingBox.Left - prev.BoundingBox.Right > CellGap)
                    {
                        cells.Add(string.Join(" ", current));
                        current.Clear();
                    }
                    current.Add(w.Text);
                    prev = w;
                }
                cells.Add(string.Join(" ", current));
                rows.Add(cells.ToArray());
            }
            return rows;
        }
    }
}

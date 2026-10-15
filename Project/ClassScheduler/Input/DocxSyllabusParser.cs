using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClassScheduler.Input
{
    /*
    Class Name: DocxSyllabusParser
    Module Name: Input Handling
    Description: Parses a Word (.docx) syllabus by reading its tables.
    Method List:
    Parse(string path) : ParseResult<List<CourseSection>>
    */
    public class DocxSyllabusParser : IFileParser<List<CourseSection>>
    {
        /*
        Class Name: DocxSyllabusParser
        Module Name: Input Handling
        Description: Opens the document read-only, converts each table to a cell grid, and maps every table whose header looks like a schedule. Reports an error if no such table exists.
        Variable List:
        string path - location of the .docx file
        ParseResult<List<CourseSection>> (return) - sections from all qualifying tables, plus errors
        */
        public ParseResult<List<CourseSection>> Parse(string path)
        {
            var result = new ParseResult<List<CourseSection>> { Data = new() };
            try
            {
                using var doc = WordprocessingDocument.Open(path, false);
                var tables = doc.MainDocumentPart?.Document.Body?.Descendants<Table>() ?? Enumerable.Empty<Table>();
                int found = 0;

                foreach (var table in tables)
                {
                    var rows = table.Elements<TableRow>()
                        .Select(r => r.Elements<TableCell>().Select(c => c.InnerText.Trim()).ToArray())
                        .ToList();

                    int header = SyllabusRowMapper.FindHeaderRow(rows);
                    if (header < 0) continue;

                    found++;
                    var mapped = SyllabusRowMapper.MapTable(rows, header);
                    result.Data.AddRange(mapped.Data);
                    result.Errors.AddRange(mapped.Errors);
                }

                if (found == 0)
                {
                    // No schedule table: try the labeled-lines layout ("Instructor:", "Class Hours:", ...).
                    var paragraphs = doc.MainDocumentPart?.Document.Body?.Descendants<Paragraph>().Select(p => p.InnerText)
                                     ?? Enumerable.Empty<string>();
                    var header = SyllabusHeaderReader.Read(paragraphs);

                    if (header.Data.Count > 0)
                    {
                        result.Data.AddRange(header.Data);
                        result.Errors.AddRange(header.Errors);
                    }
                    else
                    {
                        result.Errors.Add(new ParseError
                        {
                            Line = 0,
                            Field = "table",
                            Message = $"'{Path.GetFileName(path)}' has no schedule table and no course header line " +
                                      "(e.g. 'COSC 369 (3 Credit) Software Engineering I') with an 'Instructor:' / 'Class Hours:' line."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ParseError { Line = 0, Field = "file", Message = $"Cannot read Word file: {ex.Message}" });
            }
            return result;
        }
    }
}

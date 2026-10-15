using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;
using UglyToad.PdfPig;

namespace ClassScheduler.Input
{
    /*
    Class Name: FacultyPdfParser
    Module Name: Input Handling
    Description: Reads professor information from a text-based PDF syllabus.
    Method List:
    Parse(string path) : ParseResult<List<Professor>>
    */
    public class FacultyPdfParser : IFileParser<List<Professor>>
    {
        /*
        Class Name: FacultyPdfParser
        Module Name: Input Handling
        Description: Rebuilds lines with PdfSyllabusParser.ExtractRows, joins each line's cells, and passes them to SyllabusHeaderReader.ReadProfessor. Scanned PDFs need OCR and are not supported.
        Variable List:
        string path - .pdf file location
        */
        public ParseResult<List<Professor>> Parse(string path)
        {
            try
            {
                using var pdf = PdfDocument.Open(path);
                var lines = pdf.GetPages().SelectMany(PdfSyllabusParser.ExtractRows).Select(r => string.Join(" ", r)).ToList();
                if (lines.Count == 0)
                    return ParseResult<List<Professor>>.Failure("No text found. The PDF may be scanned; OCR is required.", new());
                return SyllabusHeaderReader.ReadProfessor(lines);
            }
            catch (Exception ex)
            {
                return ParseResult<List<Professor>>.Failure($"Cannot read PDF: {ex.Message}", new());
            }
        }
    }
}

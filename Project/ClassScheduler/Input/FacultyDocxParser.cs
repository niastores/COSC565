using System;
using System.Collections.Generic;
using System.Text;
using ClassScheduler.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClassScheduler.Input
{
    
    /*
    Class Name: FacultyDocxParser
    Module Name: Input Handling
    Description: Reads professor information from a Word syllabus.
    Method List:
    Parse(string path) : ParseResult<List<Professor>>
    */
    public class FacultyDocxParser : IFileParser<List<Professor>>
    {
        /*
        Class Name: FacultyDocxParser
        Module Name: Input Handling
        Description: Extracts all paragraph text (including table cells) and passes it to SyllabusHeaderReader.ReadProfessor.
        Variable List:
        string path - .docx file location
        */
        public ParseResult<List<Professor>> Parse(string path)
        {
            try
            {
                using var doc = WordprocessingDocument.Open(path, false);
                var paragraphs = doc.MainDocumentPart?.Document.Body?.Descendants<Paragraph>().Select(p => p.InnerText).ToList()
                                 ?? new List<string>();
                return SyllabusHeaderReader.ReadProfessor(paragraphs);
            }
            catch (Exception ex)
            {
                return ParseResult<List<Professor>>.Failure($"Cannot read Word file: {ex.Message}", new());
            }
        }
    }
}

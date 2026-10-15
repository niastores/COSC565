using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScheduler.Input
{
    /*
    Class Name: ExcelTableReader
    Module Name: Input Handling
    Description: Reads the first worksheet of an .xlsx file as a grid of text cells.
    Method List:
    ReadRows(string path) : List<string[]>
    */
    public static class ExcelTableReader
    {
        /*
        Class Name: ExcelTableReader
        Module Name: Input Handling
        Description: Returns the used range of the first worksheet. Cells are read as displayed text, so time-formatted cells come out as "9:00 AM".
        Variable List:
        string path - workbook location
        List<string[]> (return) - one array of cell text per row
        */
        public static List<string[]> ReadRows(string path)
        {
            using var workbook = new XLWorkbook(path);
            var range = workbook.Worksheets.First().RangeUsed();
            if (range is null) return new List<string[]>();   // empty sheet
            int columns = range.ColumnCount();
            var rows = new List<string[]>();
            foreach (var row in range.Rows())
                rows.Add(Enumerable.Range(1, columns).Select(c => row.Cell(c).GetFormattedString().Trim()).ToArray());
            return rows;
        }
    }
}

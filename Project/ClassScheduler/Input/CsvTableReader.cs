using System;
using System.Collections.Generic;
using System.Text;

namespace ClassScheduler.Input
{
    public static class CsvTableReader
    {
        /*
        Class Name: CsvTableReader
        Module Name: Input Handling
        Description: Reads a CSV file into rows of cells. Blank lines are skipped.
        Variable List:
        string path - CSV file location
        List<string[]> (return) - one array of cell text per row (header included)
        */
        public static List<string[]> ReadRows(string path)
        {
            string text = File.ReadAllText(path);
            var rows = new List<string[]>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool inQuotes = false;
            // Finishes the current row; rows whose cells are all blank are dropped.
            void EndRow()
            {
                row.Add(cell.ToString());
                cell.Clear();
                if (row.Any(c => c.Trim().Length > 0)) rows.Add(row.ToArray());
                row.Clear();
            }
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // A doubled quote inside a quoted field is a literal quote.
                        if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else cell.Append(c);
                }
                else if (c == '"') inQuotes = true;
                else if (c == ',') { row.Add(cell.ToString()); cell.Clear(); }
                else if (c == '\r') { /* ignore; the \n ends the row */ }
                else if (c == '\n') EndRow();
                else cell.Append(c);
            }
            if (cell.Length > 0 || row.Count > 0) EndRow();   // file without trailing newline
            return rows;
        }
    }
}

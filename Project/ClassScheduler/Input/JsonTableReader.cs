using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ClassScheduler.Input
{
    /*
    Class Name: JsonTableReader
    Module Name: Input Handling
    Description: Reads a JSON array of flat objects as a table: property names become the header row.
    Method List:
    ReadRows(string path) : List<string[]>
    */
    public static class JsonTableReader
    {
        /*
        Class Name: JsonTableReader
        Module Name: Input Handling
        Description: Converts [{"code":"COSC 631", ...}, ...] into a header row plus one row per object. Arrays become "a; b; c". Missing properties become empty cells.
        Variable List:
        string path - JSON file location
        List<string[]> (return) - header row followed by data rows
        */
        public static List<string[]> ReadRows(string path)
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("Expected a JSON array of objects.");
            // Header = every property name seen, in first-seen order.
            var headers = new List<string>();
            foreach (var obj in doc.RootElement.EnumerateArray())
                if (obj.ValueKind == JsonValueKind.Object)
                    foreach (var prop in obj.EnumerateObject())
                        if (!headers.Contains(prop.Name)) headers.Add(prop.Name);
            var rows = new List<string[]> { headers.ToArray() };
            foreach (var obj in doc.RootElement.EnumerateArray())
            {
                if (obj.ValueKind != JsonValueKind.Object) continue;
                rows.Add(headers.Select(h => obj.TryGetProperty(h, out var v) ? ToText(v) : "").ToArray());
            }
            return rows;
        }
        // Flattens a JSON value to cell text.
        private static string ToText(JsonElement v) => v.ValueKind switch
        {
            JsonValueKind.String => v.GetString() ?? "",
            JsonValueKind.Null => "",
            JsonValueKind.Array => string.Join("; ", v.EnumerateArray().Select(ToText)),
            _ => v.GetRawText(),
        };
    }
}

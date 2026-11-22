namespace LibraryManagement.ConsoleApp.Output;

/// <summary>Writes rows as aligned plain-text columns.</summary>
public static class Table
{
    /// <summary>Cells longer than this are shortened with "..." so tables stay readable.</summary>
    public const int MaxCellWidth = 40;

    public static void Write(TextWriter output, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        var cells = rows.Select(row => row.Select(Shorten).ToArray()).ToList();
        var widths = headers
            .Select((header, column) => cells
                .Select(row => column < row.Length ? row[column].Length : 0)
                .Append(header.Length)
                .Max())
            .ToArray();

        WriteRow(output, headers, widths);
        WriteRow(output, widths.Select(width => new string('-', width)).ToArray(), widths);
        foreach (var row in cells)
        {
            WriteRow(output, row, widths);
        }
    }

    private static string Shorten(string cell) =>
        cell.Length <= MaxCellWidth ? cell : string.Concat(cell.AsSpan(0, MaxCellWidth - 3), "...");

    private static void WriteRow(TextWriter output, IReadOnlyList<string> row, int[] widths)
    {
        var padded = widths.Select((width, column) => (column < row.Count ? row[column] : string.Empty).PadRight(width));
        output.WriteLine($"  {string.Join("  ", padded).TrimEnd()}");
    }
}

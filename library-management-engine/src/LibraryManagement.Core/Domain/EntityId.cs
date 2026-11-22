using System.Globalization;

namespace LibraryManagement.Core.Domain;

/// <summary>Shared parsing and formatting for prefixed identifiers such as "M-0001".</summary>
internal static class EntityId
{
    public static string Format(char prefix, int value) =>
        string.Create(CultureInfo.InvariantCulture, $"{prefix}-{value:D4}");

    /// <summary>Accepts "M-0001", "m-1", "M1" or just "1".</summary>
    public static bool TryParse(string? input, char prefix, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var text = input.Trim();
        if (char.ToUpperInvariant(text[0]) == prefix)
        {
            text = text[1..];
            if (text.StartsWith('-'))
            {
                text = text[1..];
            }
        }

        return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value > 0;
    }
}

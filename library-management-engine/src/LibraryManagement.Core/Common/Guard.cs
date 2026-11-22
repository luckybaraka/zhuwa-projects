namespace LibraryManagement.Core.Common;

/// <summary>
/// Guard clauses that protect entity invariants. A failing guard means a caller skipped
/// validation, which is a bug, so these throw rather than return a <see cref="Result"/>.
/// </summary>
internal static class Guard
{
    /// <summary>Returns the trimmed text, or throws if it is blank or too long.</summary>
    public static string NotBlank(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be blank.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value must be at most {maxLength} characters.", paramName);
        }

        return trimmed;
    }

    public static int InRange(int value, int min, int max, string paramName) =>
        value < min || value > max
            ? throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.")
            : value;

    public static decimal NotNegative(decimal value, string paramName) =>
        value < 0
            ? throw new ArgumentOutOfRangeException(paramName, value, "Value must not be negative.")
            : value;
}

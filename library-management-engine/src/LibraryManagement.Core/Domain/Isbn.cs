using System.Diagnostics.CodeAnalysis;

namespace LibraryManagement.Core.Domain;

/// <summary>
/// An International Standard Book Number, validated by its check digit.
/// </summary>
/// <remarks>
/// ISBN-10 input is converted to its ISBN-13 equivalent, so "0-262-03384-4" and
/// "978-0-262-03384-8" are the same book. Without this, one title could be catalogued
/// twice and its copies would be split across two records.
/// </remarks>
public sealed record Isbn
{
    private Isbn(string value) => Value = value;

    /// <summary>The canonical 13-digit form without separators.</summary>
    public string Value { get; }

    public static bool TryParse([NotNullWhen(true)] string? input, [NotNullWhen(true)] out Isbn? isbn)
    {
        isbn = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var compact = string.Concat(input.Where(c => c is not ('-' or ' '))).ToUpperInvariant();
        string? canonical = compact.Length switch
        {
            10 when IsValidIsbn10(compact) => ConvertIsbn10To13(compact),
            13 when IsValidIsbn13(compact) => compact,
            _ => null,
        };

        if (canonical is null)
        {
            return false;
        }

        isbn = new Isbn(canonical);
        return true;
    }

    public static Isbn Parse(string input) =>
        TryParse(input, out var isbn)
            ? isbn
            : throw new FormatException($"'{input}' is not a valid ISBN-10 or ISBN-13.");

    public override string ToString() => Value;

    // ISBN-10: weights 10..1, the last character may be 'X' (= 10), sum divisible by 11.
    private static bool IsValidIsbn10(string s)
    {
        var sum = 0;
        for (var i = 0; i < 10; i++)
        {
            int digit;
            if (char.IsAsciiDigit(s[i]))
            {
                digit = s[i] - '0';
            }
            else if (s[i] == 'X' && i == 9)
            {
                digit = 10;
            }
            else
            {
                return false;
            }

            sum += (10 - i) * digit;
        }

        return sum % 11 == 0;
    }

    // ISBN-13: alternating weights 1 and 3, sum divisible by 10.
    private static bool IsValidIsbn13(string s) =>
        s.All(char.IsAsciiDigit) && WeightedSum13(s, 13) % 10 == 0;

    private static string ConvertIsbn10To13(string isbn10)
    {
        var body = "978" + isbn10[..9];
        var check = (10 - (WeightedSum13(body, 12) % 10)) % 10;
        return body + (char)('0' + check);
    }

    private static int WeightedSum13(string s, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += (s[i] - '0') * (i % 2 == 0 ? 1 : 3);
        }

        return sum;
    }
}

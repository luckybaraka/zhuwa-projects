using System.Diagnostics.CodeAnalysis;

namespace LibraryManagement.Core.Domain;

/// <summary>
/// A member's e-mail address. It is the natural key that stops the same person
/// registering twice.
/// </summary>
/// <remarks>
/// Validation is deliberately pragmatic (one '@', a dotted domain, no spaces) rather than
/// full RFC 5322. The only real proof an address works is sending mail to it.
/// Addresses are stored lower-case so uniqueness checks are case-insensitive.
/// </remarks>
public sealed record EmailAddress
{
    public const int MaxLength = 254;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static bool TryParse([NotNullWhen(true)] string? input, [NotNullWhen(true)] out EmailAddress? email)
    {
        email = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var candidate = input.Trim();
        if (candidate.Length > MaxLength || candidate.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var at = candidate.IndexOf('@', StringComparison.Ordinal);
        if (at <= 0 || at != candidate.LastIndexOf('@'))
        {
            return false;
        }

        var domain = candidate[(at + 1)..];
        var dot = domain.LastIndexOf('.');
        if (dot <= 0 || dot == domain.Length - 1 || domain.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        email = new EmailAddress(candidate.ToLowerInvariant());
        return true;
    }

    public static EmailAddress Parse(string input) =>
        TryParse(input, out var email)
            ? email
            : throw new FormatException($"'{input}' is not a valid e-mail address.");

    public override string ToString() => Value;
}

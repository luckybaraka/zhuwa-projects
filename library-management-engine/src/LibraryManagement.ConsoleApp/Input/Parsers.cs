using System.Globalization;
using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.ConsoleApp.Input;

/// <summary>
/// Pure functions that turn raw text into domain values or a friendly error. They hold no
/// I/O, so every validation message can be unit-tested without a console.
/// </summary>
public static class Parsers
{
    public static ParseOutcome<string> Text(string input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ParseOutcome.Invalid<string>("This cannot be blank.");
        }

        var trimmed = input.Trim();
        return trimmed.Length <= maxLength
            ? ParseOutcome.Valid(trimmed)
            : ParseOutcome.Invalid<string>($"Please use at most {maxLength} characters (you typed {trimmed.Length}).");
    }

    public static ParseOutcome<int> WholeNumber(string input, int min, int max) =>
        int.TryParse(input, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out var number) && number >= min && number <= max
            ? ParseOutcome.Valid(number)
            : ParseOutcome.Invalid<int>($"Enter a whole number from {min} to {max}.");

    /// <summary>A positive money amount with at most two decimal places, not above <paramref name="max"/>.</summary>
    public static ParseOutcome<decimal> Amount(string input, decimal max)
    {
        if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            return ParseOutcome.Invalid<decimal>("Enter a positive amount such as 50 or 12.50.");
        }

        return amount <= max
            ? ParseOutcome.Valid(amount)
            : ParseOutcome.Invalid<decimal>($"That is more than the {Money.Format(max)} owed.");
    }

    public static ParseOutcome<Isbn> Isbn(string input) =>
        Core.Domain.Isbn.TryParse(input, out var isbn)
            ? ParseOutcome.Valid(isbn)
            : ParseOutcome.Invalid<Isbn>("That is not a valid ISBN-10 or ISBN-13 (check the digits).");

    public static ParseOutcome<EmailAddress> Email(string input) =>
        EmailAddress.TryParse(input, out var email)
            ? ParseOutcome.Valid(email)
            : ParseOutcome.Invalid<EmailAddress>("That does not look like an e-mail address (e.g. name@example.org).");

    public static ParseOutcome<MemberId> MemberId(string input) =>
        Core.Domain.MemberId.TryParse(input, out var id)
            ? ParseOutcome.Valid(id)
            : ParseOutcome.Invalid<MemberId>("Enter a member id such as M-0001.");

    public static ParseOutcome<LoanId> LoanId(string input) =>
        Core.Domain.LoanId.TryParse(input, out var id)
            ? ParseOutcome.Valid(id)
            : ParseOutcome.Invalid<LoanId>("Enter a loan id such as L-0001.");

    /// <summary>Accepts the option's number (1-based) or its name, ignoring case.</summary>
    public static ParseOutcome<TEnum> Choice<TEnum>(string input)
        where TEnum : struct, Enum
    {
        var options = Enum.GetValues<TEnum>();
        var text = input.Trim();

        if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number) &&
            number >= 1 && number <= options.Length)
        {
            return ParseOutcome.Valid(options[number - 1]);
        }

        foreach (var option in options)
        {
            if (string.Equals(option.ToString(), text, StringComparison.OrdinalIgnoreCase))
            {
                return ParseOutcome.Valid(option);
            }
        }

        return ParseOutcome.Invalid<TEnum>($"Choose 1-{options.Length} or type the name.");
    }
}

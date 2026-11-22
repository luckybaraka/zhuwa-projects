using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.ConsoleApp.Tests.Input;

public class ParsersTests
{
    [Theory]
    [InlineData("Amina", "Amina")]
    [InlineData("  Amina Otieno  ", "Amina Otieno")]
    public void Text_Valid_IsTrimmed(string input, string expected)
    {
        var outcome = Parsers.Text(input, 20);

        Assert.True(outcome.IsValid);
        Assert.Equal(expected, outcome.Value);
    }

    [Theory]
    [InlineData("", "cannot be blank")]
    [InlineData("    ", "cannot be blank")]
    [InlineData("abcdefghijk", "at most 10 characters")]
    public void Text_Invalid_ExplainsWhy(string input, string expectedError)
    {
        var outcome = Parsers.Text(input, 10);

        Assert.False(outcome.IsValid);
        Assert.Contains(expectedError, outcome.Error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData(" 5 ", true)]
    [InlineData("10", true)]
    [InlineData("0", false)]
    [InlineData("11", false)]
    [InlineData("-3", false)]
    [InlineData("2.5", false)]
    [InlineData("two", false)]
    [InlineData("", false)]
    public void WholeNumber_EnforcesRange(string input, bool valid)
    {
        Assert.Equal(valid, Parsers.WholeNumber(input, 1, 10).IsValid);
    }

    [Theory]
    [InlineData("50", 50)]
    [InlineData("12.50", 12.5)]
    [InlineData("0.01", 0.01)]
    [InlineData("100", 100)]
    public void Amount_Valid(string input, decimal expected)
    {
        var outcome = Parsers.Amount(input, 100m);

        Assert.True(outcome.IsValid);
        Assert.Equal(expected, outcome.Value);
    }

    [Theory]
    [InlineData("0", "positive amount")]
    [InlineData("-5", "positive amount")]
    [InlineData("1.005", "positive amount")]
    [InlineData("abc", "positive amount")]
    [InlineData("100.01", "more than the KES 100.00 owed")]
    public void Amount_Invalid_ExplainsWhy(string input, string expectedError)
    {
        var outcome = Parsers.Amount(input, 100m);

        Assert.False(outcome.IsValid);
        Assert.Contains(expectedError, outcome.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Isbn_AcceptsHyphenatedAndRejectsBadCheckDigit()
    {
        Assert.Equal("9780132350884", Parsers.Isbn("978-0-13-235088-4").Value!.Value);
        Assert.False(Parsers.Isbn("978-0-13-235088-5").IsValid);
    }

    [Fact]
    public void Email_ValidAndInvalid()
    {
        Assert.True(Parsers.Email("a@example.org").IsValid);
        Assert.Contains("e-mail", Parsers.Email("not-an-email").Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Ids_ParseFriendlyForms()
    {
        Assert.Equal(new MemberId(3), Parsers.MemberId("m-3").Value);
        Assert.Equal(new LoanId(12), Parsers.LoanId("L-0012").Value);
        Assert.False(Parsers.MemberId("L-0001").IsValid);
        Assert.False(Parsers.LoanId("x").IsValid);
    }

    [Theory]
    [InlineData("1", BookCategory.General)]
    [InlineData("2", BookCategory.ShortLoan)]
    [InlineData("3", BookCategory.Reference)]
    [InlineData("shortloan", BookCategory.ShortLoan)]
    [InlineData(" REFERENCE ", BookCategory.Reference)]
    public void Choice_AcceptsNumberOrName(string input, BookCategory expected)
    {
        Assert.Equal(expected, Parsers.Choice<BookCategory>(input).Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("4")]
    [InlineData("magazine")]
    [InlineData("")]
    public void Choice_Invalid_ListsTheRange(string input)
    {
        var outcome = Parsers.Choice<BookCategory>(input);

        Assert.False(outcome.IsValid);
        Assert.Contains("Choose 1-3", outcome.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Then_ChainsOnlyValidOutcomes()
    {
        var even = Parsers.WholeNumber("4", 1, 10)
            .Then(n => n % 2 == 0 ? ParseOutcome.Valid(n * 10) : ParseOutcome.Invalid<int>("odd"));
        var odd = Parsers.WholeNumber("3", 1, 10)
            .Then(n => n % 2 == 0 ? ParseOutcome.Valid(n * 10) : ParseOutcome.Invalid<int>("odd"));
        var invalid = Parsers.WholeNumber("x", 1, 10)
            .Then(n => ParseOutcome.Valid(n));

        Assert.Equal(40, even.Value);
        Assert.Equal("odd", odd.Error);
        Assert.Contains("whole number", invalid.Error, StringComparison.Ordinal);
    }
}

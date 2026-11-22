using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Tests.Domain;

public class IsbnTests
{
    [Theory]
    [InlineData("9780132350884", "9780132350884")]      // plain ISBN-13
    [InlineData("978-0-13-235088-4", "9780132350884")]  // hyphenated
    [InlineData("978 0 13 235088 4", "9780132350884")]  // spaced
    [InlineData("0262033844", "9780262033848")]         // ISBN-10 converted to ISBN-13
    [InlineData("0-8044-2957-X", "9780804429573")]      // ISBN-10 with X check digit
    [InlineData("0-8044-2957-x", "9780804429573")]      // lower-case x accepted
    public void TryParse_ValidInput_ReturnsCanonicalIsbn13(string input, string expected)
    {
        Assert.True(Isbn.TryParse(input, out var isbn));
        Assert.Equal(expected, isbn.Value);
        Assert.Equal(expected, isbn.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("9780132350885")]   // wrong ISBN-13 check digit
    [InlineData("0262033845")]      // wrong ISBN-10 check digit
    [InlineData("978013235088")]    // 12 digits
    [InlineData("97801323508841")]  // 14 digits
    [InlineData("97801323508X4")]   // X is only legal in ISBN-10
    [InlineData("X262033844")]      // X is only legal as the last ISBN-10 character
    [InlineData("978013235088A")]   // letter
    [InlineData("978_0132350884")]  // unsupported separator
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(Isbn.TryParse(input, out var isbn));
        Assert.Null(isbn);
    }

    [Fact]
    public void Isbn10AndIsbn13OfTheSameBook_AreEqual()
    {
        Assert.Equal(Isbn.Parse("0-262-03384-4"), Isbn.Parse("978-0-262-03384-8"));
    }

    [Fact]
    public void Parse_Invalid_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Isbn.Parse("not-an-isbn"));
    }
}

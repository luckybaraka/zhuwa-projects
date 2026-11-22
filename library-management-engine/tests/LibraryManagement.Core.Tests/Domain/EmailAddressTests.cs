using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Tests.Domain;

public class EmailAddressTests
{
    [Theory]
    [InlineData("amina@example.org", "amina@example.org")]
    [InlineData("  Amina.Otieno@Example.ORG ", "amina.otieno@example.org")]
    [InlineData("a+library@sub.example.co.ke", "a+library@sub.example.co.ke")]
    public void TryParse_Valid_TrimsAndNormalisesToLowerCase(string input, string expected)
    {
        Assert.True(EmailAddress.TryParse(input, out var email));
        Assert.Equal(expected, email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("plainaddress")]
    [InlineData("@example.org")]
    [InlineData("amina@")]
    [InlineData("amina@example")]
    [InlineData("amina@.org")]
    [InlineData("amina@example.")]
    [InlineData("amina@example..org")]
    [InlineData("am ina@example.org")]
    [InlineData("a@b@example.org")]
    public void TryParse_Invalid_ReturnsFalse(string? input)
    {
        Assert.False(EmailAddress.TryParse(input, out _));
    }

    [Fact]
    public void TryParse_TooLong_ReturnsFalse()
    {
        var input = new string('a', EmailAddress.MaxLength) + "@example.org";

        Assert.False(EmailAddress.TryParse(input, out _));
    }

    [Fact]
    public void Equality_IsCaseInsensitiveThroughNormalisation()
    {
        Assert.Equal(EmailAddress.Parse("A@Example.org"), EmailAddress.Parse("a@example.ORG"));
    }

    [Fact]
    public void Parse_Invalid_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => EmailAddress.Parse("nope"));
    }
}

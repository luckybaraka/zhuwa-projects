using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Tests.Domain;

public class EntityIdTests
{
    [Theory]
    [InlineData("M-0001", 1)]
    [InlineData("m-1", 1)]
    [InlineData("M12", 12)]
    [InlineData(" 7 ", 7)]
    public void MemberId_TryParse_AcceptsCommonForms(string input, int expected)
    {
        Assert.True(MemberId.TryParse(input, out var id));
        Assert.Equal(expected, id.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("M-")]
    [InlineData("M-0")]
    [InlineData("-5")]
    [InlineData("M--5")]
    [InlineData("L-0001")]   // a loan id is not a member id
    [InlineData("M-1.5")]
    [InlineData("abc")]
    public void MemberId_TryParse_RejectsInvalid(string? input)
    {
        Assert.False(MemberId.TryParse(input, out _));
    }

    [Fact]
    public void Ids_FormatWithPrefixAndPadding()
    {
        Assert.Equal("M-0042", new MemberId(42).ToString());
        Assert.Equal("L-0007", new LoanId(7).ToString());
    }

    [Fact]
    public void LoanId_TryParse_RoundTrips()
    {
        Assert.True(LoanId.TryParse(new LoanId(15).ToString(), out var id));
        Assert.Equal(new LoanId(15), id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ids_RejectNonPositiveValues(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemberId(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LoanId(value));
    }
}

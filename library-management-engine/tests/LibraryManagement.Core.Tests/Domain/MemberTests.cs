using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Policies;

namespace LibraryManagement.Core.Tests.Domain;

public class MemberTests
{
    [Fact]
    public void Constructor_SetsValuesAndStartsWithNoFines()
    {
        var member = TestData.Member(MembershipPolicy.Staff, id: 7, name: "  Brian Mwangi ");

        Assert.Equal(new MemberId(7), member.Id);
        Assert.Equal("Brian Mwangi", member.Name);
        Assert.Equal(MembershipType.Staff, member.MembershipType);
        Assert.Equal(0m, member.OutstandingFines);
        Assert.False(member.IsBlockedByFines);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_BlankName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => TestData.Member(name: name));
    }

    [Fact]
    public void Constructor_NameTooLong_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestData.Member(name: new string('a', Member.MaxNameLength + 1)));
    }

    [Fact]
    public void ChargeAndPayFine_UpdateBalance()
    {
        var member = TestData.Member();

        member.ChargeFine(80m);
        member.ChargeFine(40m);
        member.PayFine(50m);

        Assert.Equal(70m, member.OutstandingFines);
    }

    [Fact]
    public void ChargeFine_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestData.Member().ChargeFine(-1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(10.01)]
    public void PayFine_NonPositiveOrMoreThanOwed_Throws(decimal amount)
    {
        var member = TestData.Member();
        member.ChargeFine(10m);

        Assert.Throws<ArgumentOutOfRangeException>(() => member.PayFine(amount));
        Assert.Equal(10m, member.OutstandingFines);
    }

    [Fact]
    public void IsBlockedByFines_OnlyWhenAboveThePolicyThreshold()
    {
        var student = TestData.Member(MembershipPolicy.Student);

        student.ChargeFine(MembershipPolicy.Student.FineBlockThreshold);
        Assert.False(student.IsBlockedByFines);

        student.ChargeFine(0.01m);
        Assert.True(student.IsBlockedByFines);
    }

    [Fact]
    public void Guest_IsBlockedByAnyFine()
    {
        var guest = TestData.Member(MembershipPolicy.Guest);

        guest.ChargeFine(1m);

        Assert.True(guest.IsBlockedByFines);
    }
}

using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Policies;

namespace LibraryManagement.Core.Tests.Policies;

public class MembershipPolicyTests
{
    [Theory]
    [InlineData(MembershipType.Student, typeof(StudentPolicy))]
    [InlineData(MembershipType.Staff, typeof(StaffPolicy))]
    [InlineData(MembershipType.Guest, typeof(GuestPolicy))]
    public void For_ReturnsMatchingPolicy(MembershipType type, Type expected)
    {
        var policy = MembershipPolicy.For(type);

        Assert.IsType(expected, policy);
        Assert.Equal(type, policy.Type);
    }

    [Fact]
    public void For_UnknownType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MembershipPolicy.For((MembershipType)42));
    }

    [Theory]
    [InlineData(MembershipType.Student, 3, 14, 1)]
    [InlineData(MembershipType.Staff, 10, 28, 3)]
    [InlineData(MembershipType.Guest, 1, 7, 0)]
    public void Limits_DifferByMembership(MembershipType type, int maxLoans, int days, int renewals)
    {
        var policy = MembershipPolicy.For(type);

        Assert.Equal(maxLoans, policy.MaxActiveLoans);
        Assert.Equal(days, policy.LoanPeriodDays);
        Assert.Equal(renewals, policy.MaxRenewals);
    }

    [Theory]
    [InlineData(MembershipType.Student, BookCategory.General, 14)]
    [InlineData(MembershipType.Student, BookCategory.ShortLoan, MembershipPolicy.ShortLoanPeriodDays)]
    [InlineData(MembershipType.Staff, BookCategory.General, 28)]
    [InlineData(MembershipType.Staff, BookCategory.ShortLoan, MembershipPolicy.ShortLoanPeriodDays)]
    [InlineData(MembershipType.Guest, BookCategory.General, 7)]
    public void LoanPeriodFor_ShortLoanCategoryCapsThePeriod(MembershipType type, BookCategory category, int expected)
    {
        Assert.Equal(expected, MembershipPolicy.For(type).LoanPeriodFor(category));
    }

    [Theory]
    [InlineData(MembershipType.Student, BookCategory.General, true)]
    [InlineData(MembershipType.Student, BookCategory.ShortLoan, true)]
    [InlineData(MembershipType.Student, BookCategory.Reference, false)]
    [InlineData(MembershipType.Staff, BookCategory.General, true)]
    [InlineData(MembershipType.Staff, BookCategory.ShortLoan, true)]
    [InlineData(MembershipType.Staff, BookCategory.Reference, false)]
    [InlineData(MembershipType.Guest, BookCategory.General, true)]
    [InlineData(MembershipType.Guest, BookCategory.ShortLoan, false)]
    [InlineData(MembershipType.Guest, BookCategory.Reference, false)]
    public void MayBorrow_DependsOnMembershipAndCategory(MembershipType type, BookCategory category, bool expected)
    {
        Assert.Equal(expected, MembershipPolicy.For(type).MayBorrow(category));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 10)]
    [InlineData(5, 50)]
    [InlineData(30, 300)]
    [InlineData(31, 300)]    // capped
    [InlineData(365, 300)]   // capped
    public void Student_CalculateFine_IsDailyRateCapped(int daysOverdue, decimal expected)
    {
        Assert.Equal(expected, MembershipPolicy.Student.CalculateFine(daysOverdue));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(StaffPolicy.GracePeriodDays, 0)]          // still within grace
    [InlineData(StaffPolicy.GracePeriodDays + 1, 5)]      // first charged day
    [InlineData(10, 35)]                                  // (10 - 3) * 5
    [InlineData(200, 300)]                                // capped
    public void Staff_CalculateFine_AppliesGracePeriod(int daysOverdue, decimal expected)
    {
        Assert.Equal(expected, MembershipPolicy.Staff.CalculateFine(daysOverdue));
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(4, 80)]
    public void Guest_CalculateFine_UsesHigherDailyRate(int daysOverdue, decimal expected)
    {
        Assert.Equal(expected, MembershipPolicy.Guest.CalculateFine(daysOverdue));
    }

    [Fact]
    public void CalculateFine_NegativeDays_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MembershipPolicy.Student.CalculateFine(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MembershipPolicy.Staff.CalculateFine(-1));
    }

    [Fact]
    public void ToString_IsTheMembershipName()
    {
        Assert.Equal("Staff", MembershipPolicy.Staff.ToString());
    }
}

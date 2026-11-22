using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Tests.Services;

public class RenewFineAndReportTests : LibraryServiceTestBase
{
    private (Member Member, Loan Loan) Borrow(
        MembershipType type = MembershipType.Student,
        BookCategory category = BookCategory.General)
    {
        AddBook(category: category);
        var member = Register(type);
        return (member, Library.Borrow(member.Id, TestData.CleanCode).Value);
    }

    // ---------------------------------------------------------------- Renew

    [Fact]
    public void Renew_ExtendsFromTheCurrentDueDate()
    {
        var (_, loan) = Borrow(MembershipType.Student);
        AdvanceDays(2);                               // renewing early...

        var renewed = Library.Renew(loan.Id).Value;

        Assert.Equal(TestData.Today.AddDays(28), renewed.DueOn);   // ...loses no days
        Assert.Equal(1, renewed.RenewalCount);
    }

    [Fact]
    public void Renew_OnTheDueDate_IsAllowed()
    {
        var (_, loan) = Borrow();
        AdvanceDays(14);

        Assert.True(Library.Renew(loan.Id).IsSuccess);
    }

    [Fact]
    public void Renew_StudentSecondTime_IsRefused()
    {
        var (_, loan) = Borrow(MembershipType.Student);
        Library.Renew(loan.Id);

        var result = Library.Renew(loan.Id);

        Assert.Equal([ErrorCodes.RenewalLimitReached], result.Errors.Select(e => e.Code));
        Assert.Equal(1, loan.RenewalCount);
    }

    [Fact]
    public void Renew_StaffUpToThreeTimes()
    {
        var (_, loan) = Borrow(MembershipType.Staff);

        Assert.True(Library.Renew(loan.Id).IsSuccess);
        Assert.True(Library.Renew(loan.Id).IsSuccess);
        Assert.True(Library.Renew(loan.Id).IsSuccess);
        Assert.True(Library.Renew(loan.Id).HasError(ErrorCodes.RenewalLimitReached));
        Assert.Equal(TestData.Today.AddDays(28 * 4), loan.DueOn);
    }

    [Fact]
    public void Renew_Guest_IsNeverAllowed()
    {
        var (_, loan) = Borrow(MembershipType.Guest);

        var result = Library.Renew(loan.Id);

        Assert.True(result.HasError(ErrorCodes.RenewalLimitReached));
        Assert.Contains("Guest members cannot renew", result.Errors[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renew_Overdue_IsRefused()
    {
        var (_, loan) = Borrow();
        AdvanceDays(15);

        Assert.True(Library.Renew(loan.Id).HasError(ErrorCodes.RenewalOverdue));
    }

    [Fact]
    public void Renew_ShortLoan_IsRefused()
    {
        var (_, loan) = Borrow(MembershipType.Staff, BookCategory.ShortLoan);

        Assert.True(Library.Renew(loan.Id).HasError(ErrorCodes.RenewalNotAllowedForCategory));
    }

    [Fact]
    public void Renew_ReportsEveryReasonAtOnce()
    {
        var (_, loan) = Borrow(MembershipType.Guest);
        AdvanceDays(8);

        var codes = Library.Renew(loan.Id).Errors.Select(e => e.Code);

        Assert.Equal([ErrorCodes.RenewalOverdue, ErrorCodes.RenewalLimitReached], codes);
    }

    [Fact]
    public void Renew_ReturnedOrUnknownLoan_IsRefused()
    {
        var (_, loan) = Borrow();
        Library.Return(loan.Id);

        Assert.True(Library.Renew(loan.Id).HasError(ErrorCodes.LoanAlreadyReturned));
        Assert.True(Library.Renew(new LoanId(77)).HasError(ErrorCodes.LoanNotFound));
    }

    // ---------------------------------------------------------------- Pay fine

    private Member MemberOwing(decimal amount)
    {
        var (member, loan) = Borrow(MembershipType.Student);
        AdvanceDays(14 + (int)(amount / 10));
        Library.Return(loan.Id);
        Assert.Equal(amount, member.OutstandingFines);
        return member;
    }

    [Fact]
    public void PayFine_PartialThenFull_ReducesBalanceToZero()
    {
        var member = MemberOwing(50m);

        Assert.Equal(20m, Library.PayFine(member.Id, 30m).Value);
        Assert.Equal(0m, Library.PayFine(member.Id, 20m).Value);
        Assert.Equal(0m, member.OutstandingFines);
    }

    [Fact]
    public void PayFine_MoreThanOwed_IsRefusedAndBalanceUnchanged()
    {
        var member = MemberOwing(50m);

        var result = Library.PayFine(member.Id, 50.01m);

        Assert.True(result.HasError(ErrorCodes.OverPayment));
        Assert.Equal(50m, member.OutstandingFines);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(1.005)]
    public void PayFine_InvalidAmount_IsRefused(decimal amount)
    {
        var member = MemberOwing(50m);

        Assert.True(Library.PayFine(member.Id, amount).HasError(ErrorCodes.InvalidPaymentAmount));
    }

    [Fact]
    public void PayFine_UnknownMember_IsRefused()
    {
        Assert.True(Library.PayFine(new MemberId(5), 10m).HasError(ErrorCodes.MemberNotFound));
    }

    // ---------------------------------------------------------------- Overdue report

    [Fact]
    public void OverdueLoans_ListsOnlyLateActiveLoans_MostOverdueFirst()
    {
        AddBook(TestData.CleanCode, copies: 3);
        AddBook(TestData.DesignPatterns, title: "Design Patterns");
        var student = Register(MembershipType.Student, "Student");
        var staff = Register(MembershipType.Staff, "Staff");
        var guest = Register(MembershipType.Guest, "Guest");

        var studentLoan = Library.Borrow(student.Id, TestData.CleanCode).Value;     // due day 14
        Library.Borrow(staff.Id, TestData.CleanCode);                              // due day 28
        var guestLoan = Library.Borrow(guest.Id, TestData.DesignPatterns).Value;   // due day 7

        AdvanceDays(20);
        var report = Library.OverdueLoans();

        Assert.Equal([guestLoan.Id, studentLoan.Id], report.Select(r => r.Loan.Id));
        Assert.Equal(13, report[0].DaysOverdue);
        Assert.Equal(260m, report[0].FineIfReturnedToday);    // 13 * 20
        Assert.Equal("Design Patterns", report[0].Book.Title);
        Assert.Equal(6, report[1].DaysOverdue);
        Assert.Equal(60m, report[1].FineIfReturnedToday);
    }

    [Fact]
    public void OverdueLoans_ExcludesReturnedLoans()
    {
        var (_, loan) = Borrow();
        AdvanceDays(20);
        Library.Return(loan.Id);

        Assert.Empty(Library.OverdueLoans());
    }

    [Fact]
    public void LoanHistory_KeepsReturnedLoans()
    {
        var (member, loan) = Borrow();
        Library.Return(loan.Id);

        Assert.Equal([loan], Library.LoanHistoryFor(member.Id));
        Assert.Empty(Library.ActiveLoansFor(member.Id));
        Assert.Same(loan, Library.FindLoan(loan.Id));
    }
}

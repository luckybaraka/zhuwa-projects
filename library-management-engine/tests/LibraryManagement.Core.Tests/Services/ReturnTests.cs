using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Tests.Services;

public class ReturnTests : LibraryServiceTestBase
{
    private Loan BorrowCleanCode(MembershipType type = MembershipType.Student)
    {
        AddBook();
        var member = Register(type);
        return Library.Borrow(member.Id, TestData.CleanCode).Value;
    }

    [Fact]
    public void Return_OnTime_ChargesNothingAndFreesTheCopy()
    {
        var loan = BorrowCleanCode();
        AdvanceDays(5);

        var receipt = Library.Return(loan.Id).Value;

        Assert.False(receipt.WasLate);
        Assert.Equal(0m, receipt.FineCharged);
        Assert.Equal(Today, loan.ReturnedOn);
        Assert.Equal(1, Library.FindBook(TestData.CleanCode)!.AvailableCopies);
        Assert.Empty(Library.ActiveLoansFor(loan.MemberId));
    }

    [Fact]
    public void Return_OnTheDueDate_IsNotLate()
    {
        var loan = BorrowCleanCode();
        AdvanceDays(14);

        Assert.Equal(0m, Library.Return(loan.Id).Value.FineCharged);
    }

    [Theory]
    [InlineData(MembershipType.Student, 14 + 5, 5, 50)]
    [InlineData(MembershipType.Student, 14 + 60, 60, 300)]    // capped per loan
    [InlineData(MembershipType.Staff, 28 + 3, 3, 0)]          // inside staff grace period
    [InlineData(MembershipType.Staff, 28 + 5, 5, 10)]         // (5 - 3) * 5
    [InlineData(MembershipType.Guest, 7 + 2, 2, 40)]
    public void Return_Late_ChargesFineByPolicyAndAddsToBalance(
        MembershipType type, int daysLater, int expectedDaysOverdue, decimal expectedFine)
    {
        var loan = BorrowCleanCode(type);
        AdvanceDays(daysLater);

        var receipt = Library.Return(loan.Id).Value;

        Assert.Equal(expectedDaysOverdue, receipt.DaysOverdue);
        Assert.Equal(expectedFine, receipt.FineCharged);
        Assert.Equal(expectedFine, loan.FineCharged);
        Assert.Equal(expectedFine, Library.FindMember(loan.MemberId)!.OutstandingFines);
    }

    [Fact]
    public void Return_FinesAccumulateAcrossLoans()
    {
        AddBook(TestData.CleanCode);
        AddBook(TestData.DesignPatterns, title: "Design Patterns");
        var member = Register(MembershipType.Staff);
        var first = Library.Borrow(member.Id, TestData.CleanCode).Value;
        var second = Library.Borrow(member.Id, TestData.DesignPatterns).Value;
        AdvanceDays(28 + 5);

        Library.Return(first.Id);
        Library.Return(second.Id);

        Assert.Equal(20m, member.OutstandingFines);
    }

    [Fact]
    public void Return_Twice_IsRefused()
    {
        var loan = BorrowCleanCode();
        Library.Return(loan.Id);

        var result = Library.Return(loan.Id);

        Assert.True(result.HasError(ErrorCodes.LoanAlreadyReturned));
    }

    [Fact]
    public void Return_UnknownLoan_IsRefused()
    {
        Assert.True(Library.Return(new LoanId(42)).HasError(ErrorCodes.LoanNotFound));
    }

    [Fact]
    public void Return_ReceiptCarriesMemberAndBookForDisplay()
    {
        var loan = BorrowCleanCode();

        var receipt = Library.Return(loan.Id).Value;

        Assert.Equal(loan.MemberId, receipt.Member.Id);
        Assert.Equal("Clean Code", receipt.Book.Title);
    }
}

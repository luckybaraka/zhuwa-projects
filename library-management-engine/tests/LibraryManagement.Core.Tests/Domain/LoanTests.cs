namespace LibraryManagement.Core.Tests.Domain;

public class LoanTests
{
    private static readonly DateOnly Today = TestData.Today;

    [Fact]
    public void NewLoan_IsActiveAndNotOverdueBeforeDueDate()
    {
        var loan = TestData.Loan(borrowedOn: Today, periodDays: 14);

        Assert.True(loan.IsActive);
        Assert.Equal(Today.AddDays(14), loan.DueOn);
        Assert.False(loan.IsOverdueOn(Today.AddDays(14)));   // due date itself is not late
        Assert.True(loan.IsOverdueOn(Today.AddDays(15)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_DueDateNotAfterBorrowDate_Throws(int periodDays)
    {
        Assert.Throws<ArgumentException>(() => TestData.Loan(periodDays: periodDays));
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(14, 0)]
    [InlineData(15, 1)]
    [InlineData(24, 10)]
    public void DaysOverdueOn_CountsWholeDaysPastDue(int daysAfterBorrow, int expected)
    {
        var loan = TestData.Loan(periodDays: 14);

        Assert.Equal(expected, loan.DaysOverdueOn(Today.AddDays(daysAfterBorrow)));
    }

    [Fact]
    public void Renew_MovesDueDateAndCountsRenewal()
    {
        var loan = TestData.Loan(periodDays: 14);

        loan.Renew(Today.AddDays(28));

        Assert.Equal(Today.AddDays(28), loan.DueOn);
        Assert.Equal(1, loan.RenewalCount);
    }

    [Fact]
    public void Renew_ToEarlierOrSameDate_Throws()
    {
        var loan = TestData.Loan(periodDays: 14);

        Assert.Throws<ArgumentException>(() => loan.Renew(loan.DueOn));
    }

    [Fact]
    public void MarkReturned_EndsLoanAndRecordsFine()
    {
        var loan = TestData.Loan(periodDays: 14);

        loan.MarkReturned(Today.AddDays(16), 20m);

        Assert.False(loan.IsActive);
        Assert.Equal(Today.AddDays(16), loan.ReturnedOn);
        Assert.Equal(20m, loan.FineCharged);
        Assert.False(loan.IsOverdueOn(Today.AddDays(30)));  // returned loans are never overdue
    }

    [Fact]
    public void ReturnedLoan_CannotBeReturnedOrRenewedAgain()
    {
        var loan = TestData.Loan();
        loan.MarkReturned(Today.AddDays(1), 0m);

        Assert.Throws<InvalidOperationException>(() => loan.MarkReturned(Today.AddDays(2), 0m));
        Assert.Throws<InvalidOperationException>(() => loan.Renew(Today.AddDays(40)));
    }

    [Fact]
    public void MarkReturned_BeforeBorrowDate_Throws()
    {
        var loan = TestData.Loan();

        Assert.Throws<ArgumentException>(() => loan.MarkReturned(Today.AddDays(-1), 0m));
    }

    [Fact]
    public void MarkReturned_NegativeFine_Throws()
    {
        var loan = TestData.Loan();

        Assert.Throws<ArgumentOutOfRangeException>(() => loan.MarkReturned(Today, -1m));
    }
}

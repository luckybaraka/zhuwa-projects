using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Policies;
using LibraryManagement.Core.Rules;

namespace LibraryManagement.Core.Tests.Rules;

public class BorrowingRuleTests
{
    private static readonly DateOnly Today = TestData.Today;

    private static BorrowingContext Context(
        Member? member = null,
        Book? book = null,
        int available = 1,
        params Loan[] activeLoans) =>
        new(member ?? TestData.Member(), book ?? TestData.Book(), available, activeLoans, Today);

    // --- CategoryAllowedRule -------------------------------------------------------------

    [Fact]
    public void CategoryAllowed_GeneralBook_Passes()
    {
        Assert.Null(new CategoryAllowedRule().Check(Context()));
    }

    [Fact]
    public void CategoryAllowed_ReferenceBook_FailsWithReferenceMessage()
    {
        var book = TestData.Book(TestData.Dictionary, BookCategory.Reference, title: "Oxford Dictionary");

        var error = new CategoryAllowedRule().Check(Context(book: book));

        Assert.NotNull(error);
        Assert.Equal(ErrorCodes.CategoryNotLoanable, error.Code);
        Assert.Contains("reference book", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CategoryAllowed_GuestAndShortLoan_FailsWithMembershipMessage()
    {
        var guest = TestData.Member(MembershipPolicy.Guest);
        var book = TestData.Book(category: BookCategory.ShortLoan);

        var error = new CategoryAllowedRule().Check(Context(guest, book));

        Assert.NotNull(error);
        Assert.Contains("Guest members cannot borrow ShortLoan", error.Message, StringComparison.Ordinal);
    }

    // --- CopyAvailableRule ---------------------------------------------------------------

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(0, false)]
    public void CopyAvailable_PassesOnlyWhenACopyIsOnTheShelf(int available, bool passes)
    {
        var error = new CopyAvailableRule().Check(Context(available: available));

        Assert.Equal(passes, error is null);
        if (!passes)
        {
            Assert.Equal(ErrorCodes.NoCopiesAvailable, error!.Code);
        }
    }

    // --- NotAlreadyBorrowedRule ----------------------------------------------------------

    [Fact]
    public void NotAlreadyBorrowed_DifferentTitleOnLoan_Passes()
    {
        var otherLoan = TestData.Loan(isbn: TestData.DesignPatterns);

        Assert.Null(new NotAlreadyBorrowedRule().Check(Context(activeLoans: otherLoan)));
    }

    [Fact]
    public void NotAlreadyBorrowed_SameTitleOnLoan_Fails()
    {
        var sameTitle = TestData.Loan(isbn: TestData.CleanCode);

        var error = new NotAlreadyBorrowedRule().Check(Context(activeLoans: sameTitle));

        Assert.Equal(ErrorCodes.AlreadyBorrowed, error?.Code);
    }

    // --- LoanLimitRule -------------------------------------------------------------------

    [Fact]
    public void LoanLimit_BelowLimit_Passes()
    {
        var loans = new[] { TestData.Loan(1), TestData.Loan(2) };   // student limit is 3

        Assert.Null(new LoanLimitRule().Check(Context(activeLoans: loans)));
    }

    [Fact]
    public void LoanLimit_AtLimit_Fails()
    {
        var loans = new[] { TestData.Loan(1), TestData.Loan(2), TestData.Loan(3) };

        var error = new LoanLimitRule().Check(Context(activeLoans: loans));

        Assert.Equal(ErrorCodes.LoanLimitReached, error?.Code);
        Assert.Contains("at most 3", error!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LoanLimit_GuestWithOneLoan_Fails()
    {
        var guest = TestData.Member(MembershipPolicy.Guest);

        Assert.NotNull(new LoanLimitRule().Check(Context(guest, activeLoans: TestData.Loan())));
    }

    // --- NoOverdueLoansRule --------------------------------------------------------------

    [Fact]
    public void NoOverdueLoans_LoanDueToday_Passes()
    {
        var dueToday = TestData.Loan(borrowedOn: Today.AddDays(-14), periodDays: 14);

        Assert.Null(new NoOverdueLoansRule().Check(Context(activeLoans: dueToday)));
    }

    [Fact]
    public void NoOverdueLoans_LoanPastDue_Fails()
    {
        var overdue = TestData.Loan(borrowedOn: Today.AddDays(-15), periodDays: 14);

        var error = new NoOverdueLoansRule().Check(Context(activeLoans: overdue));

        Assert.Equal(ErrorCodes.HasOverdueLoans, error?.Code);
    }

    // --- FineLimitRule -------------------------------------------------------------------

    [Fact]
    public void FineLimit_FinesAtThreshold_Passes()
    {
        var member = TestData.Member();
        member.ChargeFine(MembershipPolicy.Student.FineBlockThreshold);

        Assert.Null(new FineLimitRule().Check(Context(member)));
    }

    [Fact]
    public void FineLimit_FinesAboveThreshold_FailsAndShowsAmounts()
    {
        var member = TestData.Member();
        member.ChargeFine(150m);

        var error = new FineLimitRule().Check(Context(member));

        Assert.Equal(ErrorCodes.FinesTooHigh, error?.Code);
        Assert.Contains("KES 150.00", error!.Message, StringComparison.Ordinal);
        Assert.Contains("KES 100.00", error.Message, StringComparison.Ordinal);
    }

    // --- Evaluate ------------------------------------------------------------------------

    [Fact]
    public void Evaluate_AllRulesSatisfied_ReturnsNoErrors()
    {
        Assert.Empty(BorrowingRules.Evaluate(BorrowingRules.Default, Context()));
    }

    [Fact]
    public void Evaluate_ReportsEveryViolationNotJustTheFirst()
    {
        var member = TestData.Member();
        member.ChargeFine(500m);
        var overdue = TestData.Loan(isbn: TestData.DesignPatterns, borrowedOn: Today.AddDays(-30));

        var errors = BorrowingRules.Evaluate(BorrowingRules.Default, Context(member, available: 0, activeLoans: overdue));

        Assert.Equal(
            [ErrorCodes.NoCopiesAvailable, ErrorCodes.HasOverdueLoans, ErrorCodes.FinesTooHigh],
            errors.Select(e => e.Code));
    }

    [Fact]
    public void Default_ContainsEachBuiltInRuleOnce()
    {
        var types = BorrowingRules.Default.Select(r => r.GetType()).ToList();

        Assert.Equal(6, types.Count);
        Assert.Equal(types.Count, types.Distinct().Count());
    }
}

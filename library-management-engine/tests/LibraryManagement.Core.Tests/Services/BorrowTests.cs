using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Persistence.InMemory;
using LibraryManagement.Core.Rules;
using LibraryManagement.Core.Services;

namespace LibraryManagement.Core.Tests.Services;

public class BorrowTests : LibraryServiceTestBase
{
    [Theory]
    [InlineData(MembershipType.Student, BookCategory.General, 14)]
    [InlineData(MembershipType.Staff, BookCategory.General, 28)]
    [InlineData(MembershipType.Guest, BookCategory.General, 7)]
    [InlineData(MembershipType.Student, BookCategory.ShortLoan, 3)]
    [InlineData(MembershipType.Staff, BookCategory.ShortLoan, 3)]
    public void Borrow_Allowed_CreatesLoanWithDueDateFromPolicyAndCategory(
        MembershipType type, BookCategory category, int expectedDays)
    {
        AddBook(category: category);
        var member = Register(type);

        var result = Library.Borrow(member.Id, TestData.CleanCode);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors));
        var loan = result.Value;
        Assert.Equal(member.Id, loan.MemberId);
        Assert.Equal(Today, loan.BorrowedOn);
        Assert.Equal(Today.AddDays(expectedDays), loan.DueOn);
        Assert.True(loan.IsActive);
    }

    [Fact]
    public void Borrow_ReducesAvailabilityAndShowsInMemberLoans()
    {
        AddBook(copies: 2);
        var member = Register();

        var loan = Library.Borrow(member.Id, TestData.CleanCode).Value;

        Assert.Equal(1, Library.FindBook(TestData.CleanCode)!.AvailableCopies);
        Assert.Equal([loan], Library.ActiveLoansFor(member.Id));
    }

    [Fact]
    public void Borrow_UnknownMemberAndBook_ReportsBoth()
    {
        var result = Library.Borrow(new MemberId(9), TestData.CleanCode);

        Assert.True(result.HasError(ErrorCodes.MemberNotFound));
        Assert.True(result.HasError(ErrorCodes.BookNotFound));
    }

    [Fact]
    public void Borrow_ReferenceBook_IsRefused()
    {
        AddBook(TestData.Dictionary, BookCategory.Reference, title: "Oxford Dictionary");
        var staff = Register(MembershipType.Staff);

        Assert.True(Library.Borrow(staff.Id, TestData.Dictionary).HasError(ErrorCodes.CategoryNotLoanable));
    }

    [Fact]
    public void Borrow_GuestShortLoan_IsRefused()
    {
        AddBook(category: BookCategory.ShortLoan);
        var guest = Register(MembershipType.Guest);

        Assert.True(Library.Borrow(guest.Id, TestData.CleanCode).HasError(ErrorCodes.CategoryNotLoanable));
    }

    [Fact]
    public void Borrow_LastCopyAlreadyOut_IsRefused()
    {
        AddBook(copies: 1);
        var first = Register(name: "First");
        var second = Register(name: "Second");
        Library.Borrow(first.Id, TestData.CleanCode);

        var result = Library.Borrow(second.Id, TestData.CleanCode);

        Assert.Equal([ErrorCodes.NoCopiesAvailable], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public void Borrow_SameTitleTwice_IsRefusedEvenWithCopiesLeft()
    {
        AddBook(copies: 5);
        var member = Register();
        Library.Borrow(member.Id, TestData.CleanCode);

        Assert.True(Library.Borrow(member.Id, TestData.CleanCode).HasError(ErrorCodes.AlreadyBorrowed));
    }

    [Fact]
    public void Borrow_StudentBeyondThreeItems_IsRefused()
    {
        var member = Register(MembershipType.Student);
        Isbn[] isbns = [TestData.CleanCode, TestData.DesignPatterns, TestData.Refactoring, Isbn.Parse("9780134685991")];
        foreach (var isbn in isbns)
        {
            AddBook(isbn, title: isbn.Value);
        }

        Assert.All(isbns[..3], isbn => Assert.True(Library.Borrow(member.Id, isbn).IsSuccess));
        Assert.True(Library.Borrow(member.Id, isbns[3]).HasError(ErrorCodes.LoanLimitReached));
    }

    [Fact]
    public void Borrow_GuestSecondItem_IsRefused()
    {
        AddBook(TestData.CleanCode);
        AddBook(TestData.DesignPatterns, title: "Design Patterns");
        var guest = Register(MembershipType.Guest);
        Library.Borrow(guest.Id, TestData.CleanCode);

        Assert.True(Library.Borrow(guest.Id, TestData.DesignPatterns).HasError(ErrorCodes.LoanLimitReached));
    }

    [Fact]
    public void Borrow_WhileHoldingAnOverdueItem_IsRefused()
    {
        AddBook(TestData.CleanCode);
        AddBook(TestData.DesignPatterns, title: "Design Patterns");
        var member = Register();
        Library.Borrow(member.Id, TestData.CleanCode);
        AdvanceDays(15);

        Assert.True(Library.Borrow(member.Id, TestData.DesignPatterns).HasError(ErrorCodes.HasOverdueLoans));
    }

    [Fact]
    public void Borrow_WithFinesAboveThreshold_IsRefusedUntilPaidDown()
    {
        AddBook(TestData.CleanCode);
        AddBook(TestData.DesignPatterns, title: "Design Patterns");
        var member = Register();
        var loan = Library.Borrow(member.Id, TestData.CleanCode).Value;
        AdvanceDays(14 + 11);                        // 11 days late -> KES 110 (> 100 threshold)
        Library.Return(loan.Id);

        Assert.True(Library.Borrow(member.Id, TestData.DesignPatterns).HasError(ErrorCodes.FinesTooHigh));

        Library.PayFine(member.Id, 10m);             // back to the threshold: allowed again
        Assert.True(Library.Borrow(member.Id, TestData.DesignPatterns).IsSuccess);
    }

    [Fact]
    public void Borrow_RefusedRequest_DoesNotCreateALoanOrConsumeAnId()
    {
        AddBook(TestData.Dictionary, BookCategory.Reference, title: "Dictionary");
        AddBook(TestData.CleanCode);
        var member = Register();

        Library.Borrow(member.Id, TestData.Dictionary);
        var loan = Library.Borrow(member.Id, TestData.CleanCode).Value;

        Assert.Equal(new LoanId(1), loan.Id);
        Assert.Single(Library.ActiveLoans());
    }

    [Fact]
    public void Borrow_UsesInjectedRules_SoNewRulesNeedNoServiceChanges()
    {
        var library = new LibraryService(
            new InMemoryBookRepository(), new InMemoryMemberRepository(), new InMemoryLoanRepository(),
            Clock, [new ClosedForStocktakeRule()]);
        library.AddBook(TestData.CleanCode, "Clean Code", "Robert C. Martin", BookCategory.General, 1);
        var member = library.RegisterMember("Amina", EmailAddress.Parse("a@example.org"), MembershipType.Staff).Value;

        var result = library.Borrow(member.Id, TestData.CleanCode);

        Assert.Equal(["test.stocktake"], result.Errors.Select(e => e.Code));
    }

    private sealed class ClosedForStocktakeRule : IBorrowingRule
    {
        public DomainError? Check(BorrowingContext context) =>
            new("test.stocktake", "Lending is paused during stock-take.");
    }
}

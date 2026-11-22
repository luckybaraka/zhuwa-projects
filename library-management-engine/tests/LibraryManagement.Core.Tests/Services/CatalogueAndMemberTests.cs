using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Persistence.InMemory;
using LibraryManagement.Core.Services;

namespace LibraryManagement.Core.Tests.Services;

public class CatalogueAndMemberTests : LibraryServiceTestBase
{
    [Fact]
    public void Constructor_NullDependency_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LibraryService(new InMemoryBookRepository(), new InMemoryMemberRepository(), new InMemoryLoanRepository(), null!));
    }

    [Fact]
    public void Today_ComesFromTheInjectedClock()
    {
        Assert.Equal(TestData.Today, Library.Today);

        AdvanceDays(3);

        Assert.Equal(TestData.Today.AddDays(3), Library.Today);
    }

    // ---------------------------------------------------------------- Books

    [Fact]
    public void AddBook_NewIsbn_IsCataloguedWithAllCopiesAvailable()
    {
        var result = Library.AddBook(TestData.CleanCode, "Clean Code", "Robert C. Martin", BookCategory.General, 3);

        Assert.True(result.IsSuccess);
        var found = Library.FindBook(TestData.CleanCode);
        Assert.NotNull(found);
        Assert.Equal(3, found.AvailableCopies);
        Assert.Equal(0, found.CopiesOnLoan);
    }

    [Fact]
    public void AddBook_DuplicateIsbn_IsRefusedEvenIfEnteredAsIsbn10()
    {
        Library.AddBook(Isbn.Parse("9780262033848"), "Introduction to Algorithms", "Cormen", BookCategory.General, 1);

        var result = Library.AddBook(Isbn.Parse("0262033844"), "Intro to Algorithms", "Cormen", BookCategory.General, 1);

        Assert.True(result.HasError(ErrorCodes.DuplicateBook));
        Assert.Single(Library.ListBooks());
    }

    [Fact]
    public void AddBook_InvalidTitle_ThrowsBecauseCallerSkippedValidation()
    {
        Assert.Throws<ArgumentException>(() =>
            Library.AddBook(TestData.CleanCode, " ", "Author", BookCategory.General, 1));
    }

    [Fact]
    public void AddCopies_ExistingBook_IncreasesTotal()
    {
        AddBook(copies: 2);

        var result = Library.AddCopies(TestData.CleanCode, 3);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, Library.FindBook(TestData.CleanCode)!.AvailableCopies);
    }

    [Fact]
    public void AddCopies_UnknownBook_Fails()
    {
        Assert.True(Library.AddCopies(TestData.CleanCode, 1).HasError(ErrorCodes.BookNotFound));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Book.MaxCopies)]
    public void AddCopies_InvalidCount_Fails(int count)
    {
        AddBook();

        Assert.True(Library.AddCopies(TestData.CleanCode, count).HasError(ErrorCodes.TooManyCopies));
        Assert.Equal(1, Library.FindBook(TestData.CleanCode)!.Book.TotalCopies);
    }

    [Fact]
    public void FindBook_Unknown_ReturnsNull()
    {
        Assert.Null(Library.FindBook(TestData.CleanCode));
    }

    [Fact]
    public void ListBooks_IsSortedByTitle()
    {
        AddBook(TestData.Refactoring, title: "Refactoring");
        AddBook(TestData.CleanCode, title: "clean Code");
        AddBook(TestData.DesignPatterns, title: "Design Patterns");

        Assert.Equal(
            ["clean Code", "Design Patterns", "Refactoring"],
            Library.ListBooks().Select(b => b.Book.Title));
    }

    [Theory]
    [InlineData("clean", 1)]                 // title fragment, case-insensitive
    [InlineData("MARTIN", 1)]                // author fragment
    [InlineData("978-0-13-235088-4", 1)]     // ISBN in any accepted format
    [InlineData("9780201633610", 0)]         // valid ISBN not in catalogue
    [InlineData("tolkien", 0)]
    [InlineData("   ", 0)]
    public void SearchBooks_MatchesIsbnTitleOrAuthor(string query, int expected)
    {
        AddBook();

        Assert.Equal(expected, Library.SearchBooks(query).Count);
    }

    // ---------------------------------------------------------------- Members

    [Fact]
    public void RegisterMember_AssignsSequentialIdsAndPolicy()
    {
        var first = Register(MembershipType.Student);
        var second = Register(MembershipType.Staff);

        Assert.Equal(new MemberId(1), first.Id);
        Assert.Equal(new MemberId(2), second.Id);
        Assert.Equal(MembershipType.Staff, second.MembershipType);
        Assert.Same(second, Library.FindMember(second.Id));
    }

    [Fact]
    public void RegisterMember_DuplicateEmailInAnyCase_IsRefused()
    {
        Register(email: "amina@example.org");

        var result = Library.RegisterMember("Someone Else", EmailAddress.Parse("AMINA@example.org"), MembershipType.Guest);

        Assert.True(result.HasError(ErrorCodes.DuplicateEmail));
        Assert.Single(Library.ListMembers());
    }

    [Fact]
    public void FindMember_Unknown_ReturnsNull()
    {
        Assert.Null(Library.FindMember(new MemberId(99)));
    }

    [Fact]
    public void ListMembers_IsInRegistrationOrder()
    {
        Register(name: "Zed");
        Register(name: "Abe");

        Assert.Equal(["Zed", "Abe"], Library.ListMembers().Select(m => m.Name));
    }
}

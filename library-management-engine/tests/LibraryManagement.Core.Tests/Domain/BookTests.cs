using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Tests.Domain;

public class BookTests
{
    [Fact]
    public void Constructor_TrimsTextAndKeepsValues()
    {
        var book = new Book(TestData.CleanCode, "  Clean Code ", " Robert C. Martin ", BookCategory.ShortLoan, 2);

        Assert.Equal(TestData.CleanCode, book.Isbn);
        Assert.Equal("Clean Code", book.Title);
        Assert.Equal("Robert C. Martin", book.Author);
        Assert.Equal(BookCategory.ShortLoan, book.Category);
        Assert.Equal(2, book.TotalCopies);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankTitle_Throws(string title)
    {
        Assert.Throws<ArgumentException>(() => TestData.Book(title: title));
    }

    [Fact]
    public void Constructor_BlankAuthor_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestData.Book(author: " "));
    }

    [Fact]
    public void Constructor_TitleTooLong_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestData.Book(title: new string('x', Book.MaxTitleLength + 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Book.MaxCopies + 1)]
    public void Constructor_CopiesOutOfRange_Throws(int copies)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestData.Book(copies: copies));
    }

    [Fact]
    public void Constructor_UndefinedCategory_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestData.Book(category: (BookCategory)99));
    }

    [Fact]
    public void AddCopies_IncreasesTotal()
    {
        var book = TestData.Book(copies: 2);

        book.AddCopies(3);

        Assert.Equal(5, book.TotalCopies);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(Book.MaxCopies)]
    public void AddCopies_InvalidCount_ThrowsAndLeavesTotalUnchanged(int count)
    {
        var book = TestData.Book(copies: 1);

        Assert.False(book.CanAddCopies(count));
        Assert.Throws<ArgumentOutOfRangeException>(() => book.AddCopies(count));
        Assert.Equal(1, book.TotalCopies);
    }
}

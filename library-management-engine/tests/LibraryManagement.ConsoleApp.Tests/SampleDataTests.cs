using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Tests;

public class SampleDataTests
{
    [Fact]
    public void Load_AddsBooksOfEveryCategoryAndOneMemberOfEachType()
    {
        var library = LibraryService.CreateInMemory(new FixedClock());

        SampleData.Load(library);

        var books = library.ListBooks();
        Assert.Equal(7, books.Count);
        Assert.Equal(
            Enum.GetValues<BookCategory>().Order(),
            books.Select(b => b.Book.Category).Distinct().Order());
        Assert.Equal(
            Enum.GetValues<MembershipType>(),
            library.ListMembers().Select(m => m.MembershipType));
    }

    [Fact]
    public void Load_Twice_FailsLoudlyOnDuplicates()
    {
        var library = LibraryService.CreateInMemory(new FixedClock());
        SampleData.Load(library);

        Assert.Throws<InvalidOperationException>(() => SampleData.Load(library));
    }
}

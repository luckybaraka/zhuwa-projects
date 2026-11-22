using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Persistence.InMemory;

namespace LibraryManagement.Core.Tests.Persistence;

public class InMemoryRepositoryTests
{
    [Fact]
    public void BookRepository_AddFindAndRejectDuplicate()
    {
        var repo = new InMemoryBookRepository();
        var book = TestData.Book();

        repo.Add(book);

        Assert.Same(book, repo.Find(TestData.CleanCode));
        Assert.Null(repo.Find(TestData.DesignPatterns));
        Assert.Throws<InvalidOperationException>(() => repo.Add(TestData.Book()));
        Assert.Single(repo.All());
    }

    [Fact]
    public void MemberRepository_IssuesIdsAndIndexesByEmail()
    {
        var repo = new InMemoryMemberRepository();
        var id = repo.NextId();
        var member = TestData.Member(id: id.Value, email: "a@example.org");

        repo.Add(member);

        Assert.Equal(new MemberId(1), id);
        Assert.Equal(new MemberId(2), repo.NextId());
        Assert.Same(member, repo.Find(id));
        Assert.Same(member, repo.FindByEmail(EmailAddress.Parse("A@EXAMPLE.ORG")));
        Assert.Throws<InvalidOperationException>(() => repo.Add(TestData.Member(id: 5, email: "a@example.org")));
    }

    [Fact]
    public void LoanRepository_SeparatesActiveFromHistory()
    {
        var repo = new InMemoryLoanRepository();
        var active = TestData.Loan(repo.NextId().Value, memberId: 1, isbn: TestData.CleanCode);
        var returned = TestData.Loan(repo.NextId().Value, memberId: 1, isbn: TestData.DesignPatterns);
        var otherMember = TestData.Loan(repo.NextId().Value, memberId: 2, isbn: TestData.CleanCode);
        returned.MarkReturned(TestData.Today, 0m);

        repo.Add(active);
        repo.Add(returned);
        repo.Add(otherMember);

        Assert.Equal(2, repo.Active().Count);
        Assert.Equal([active], repo.ActiveFor(new MemberId(1)));
        Assert.Equal(2, repo.HistoryFor(new MemberId(1)).Count);
        Assert.Equal(2, repo.CountActiveFor(TestData.CleanCode));
        Assert.Equal(0, repo.CountActiveFor(TestData.DesignPatterns));
        Assert.Same(returned, repo.Find(returned.Id));
        Assert.Throws<InvalidOperationException>(() => repo.Add(active));
    }
}

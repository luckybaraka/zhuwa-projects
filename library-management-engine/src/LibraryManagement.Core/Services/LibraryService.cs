using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Persistence;
using LibraryManagement.Core.Persistence.InMemory;
using LibraryManagement.Core.Policies;
using LibraryManagement.Core.Rules;

namespace LibraryManagement.Core.Services;

/// <summary>
/// The single entry point for every library operation. It coordinates repositories,
/// policies and rules so each use case (borrow, return, renew...) changes state in one
/// consistent step. The UI talks only to this class.
/// </summary>
public sealed class LibraryService
{
    private readonly IBookRepository _books;
    private readonly IMemberRepository _members;
    private readonly ILoanRepository _loans;
    private readonly TimeProvider _clock;
    private readonly IReadOnlyList<IBorrowingRule> _rules;

    public LibraryService(
        IBookRepository books,
        IMemberRepository members,
        ILoanRepository loans,
        TimeProvider clock,
        IEnumerable<IBorrowingRule>? rules = null)
    {
        _books = books ?? throw new ArgumentNullException(nameof(books));
        _members = members ?? throw new ArgumentNullException(nameof(members));
        _loans = loans ?? throw new ArgumentNullException(nameof(loans));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _rules = rules?.ToList() ?? BorrowingRules.Default;
    }

    /// <summary>A service backed by in-memory storage and the standard rules.</summary>
    public static LibraryService CreateInMemory(TimeProvider clock) =>
        new(new InMemoryBookRepository(), new InMemoryMemberRepository(), new InMemoryLoanRepository(), clock);

    /// <summary>The library's current date. All due-date and fine maths uses this.</summary>
    public DateOnly Today => DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);

    // ---------------------------------------------------------------- Catalogue

    public Result<Book> AddBook(Isbn isbn, string title, string author, BookCategory category, int copies)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        if (_books.Find(isbn) is { } existing)
        {
            return Result.Failure<Book>(new DomainError(
                ErrorCodes.DuplicateBook,
                $"ISBN {isbn} is already catalogued as '{existing.Title}'. Add copies to it instead."));
        }

        var book = new Book(isbn, title, author, category, copies);
        _books.Add(book);
        return Result.Success(book);
    }

    public Result<Book> AddCopies(Isbn isbn, int count)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        if (_books.Find(isbn) is not { } book)
        {
            return Result.Failure<Book>(BookNotFound(isbn));
        }

        if (!book.CanAddCopies(count))
        {
            return Result.Failure<Book>(new DomainError(
                ErrorCodes.TooManyCopies,
                $"'{book.Title}' has {book.TotalCopies} copies; adding {count} would exceed the limit of {Book.MaxCopies}."));
        }

        book.AddCopies(count);
        return Result.Success(book);
    }

    public BookAvailability? FindBook(Isbn isbn)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        return _books.Find(isbn) is { } book ? WithAvailability(book) : null;
    }

    /// <summary>The whole catalogue, sorted by title.</summary>
    public IReadOnlyList<BookAvailability> ListBooks() => Sorted(_books.All());

    /// <summary>
    /// Finds books by ISBN (exact, any accepted format) or by a case-insensitive fragment
    /// of the title or author.
    /// </summary>
    public IReadOnlyList<BookAvailability> SearchBooks(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        if (Isbn.TryParse(query, out var isbn))
        {
            return FindBook(isbn) is { } match ? [match] : [];
        }

        var term = query.Trim();
        return Sorted(_books.All().Where(book =>
            book.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            book.Author.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    // ---------------------------------------------------------------- Members

    public Result<Member> RegisterMember(string name, EmailAddress email, MembershipType type)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (_members.FindByEmail(email) is { } existing)
        {
            return Result.Failure<Member>(new DomainError(
                ErrorCodes.DuplicateEmail,
                $"{email} is already registered to {existing.Name} ({existing.Id})."));
        }

        var policy = MembershipPolicy.For(type);
        var member = new Member(_members.NextId(), name, email, policy);
        _members.Add(member);
        return Result.Success(member);
    }

    public Member? FindMember(MemberId id) => _members.Find(id);

    /// <summary>All members in registration order.</summary>
    public IReadOnlyList<Member> ListMembers() => _members.All();

    // ---------------------------------------------------------------- Helpers

    private BookAvailability WithAvailability(Book book) =>
        new(book, book.TotalCopies - _loans.CountActiveFor(book.Isbn));

    private List<BookAvailability> Sorted(IEnumerable<Book> books) =>
        [.. books
            .OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase)
            .Select(WithAvailability)];

    private static DomainError BookNotFound(Isbn isbn) =>
        new(ErrorCodes.BookNotFound, $"No book with ISBN {isbn} is in the catalogue.");
}

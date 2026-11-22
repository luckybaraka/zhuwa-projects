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

    // ---------------------------------------------------------------- Circulation

    /// <summary>
    /// Lends one copy of <paramref name="isbn"/> to a member if every borrowing rule allows it.
    /// The due date comes from the member's policy, capped by the book's category.
    /// </summary>
    public Result<Loan> Borrow(MemberId memberId, Isbn isbn)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        var member = _members.Find(memberId);
        var book = _books.Find(isbn);
        if (member is null || book is null)
        {
            List<DomainError> missing = [];
            if (member is null)
            {
                missing.Add(MemberNotFound(memberId));
            }

            if (book is null)
            {
                missing.Add(BookNotFound(isbn));
            }

            return Result.Failure<Loan>(missing);
        }

        var today = Today;
        var context = new BorrowingContext(
            member, book, WithAvailability(book).AvailableCopies, _loans.ActiveFor(memberId), today);
        var violations = BorrowingRules.Evaluate(_rules, context);
        if (violations.Count > 0)
        {
            return Result.Failure<Loan>(violations);
        }

        var dueOn = today.AddDays(member.Policy.LoanPeriodFor(book.Category));
        var loan = new Loan(_loans.NextId(), memberId, isbn, today, dueOn);
        _loans.Add(loan);
        return Result.Success(loan);
    }

    /// <summary>
    /// Checks an item back in. Any fine is worked out by the member's policy (so grace
    /// periods and caps apply) and added to their balance in the same step.
    /// </summary>
    public Result<ReturnReceipt> Return(LoanId loanId)
    {
        var lookup = FindActiveLoan(loanId);
        if (lookup.IsFailure)
        {
            return Result.Failure<ReturnReceipt>(lookup.Errors);
        }

        var loan = lookup.Value;
        var member = RequireMember(loan.MemberId);
        var book = RequireBook(loan.Isbn);
        var today = Today;

        var daysOverdue = loan.DaysOverdueOn(today);
        var fine = member.Policy.CalculateFine(daysOverdue);
        loan.MarkReturned(today, fine);
        member.ChargeFine(fine);

        return Result.Success(new ReturnReceipt(loan, member, book, daysOverdue, fine));
    }

    public Loan? FindLoan(LoanId id) => _loans.Find(id);

    /// <summary>Every item currently out, oldest first.</summary>
    public IReadOnlyList<Loan> ActiveLoans() => _loans.Active();

    public IReadOnlyList<Loan> ActiveLoansFor(MemberId memberId) => _loans.ActiveFor(memberId);

    /// <summary>A member's loans, current and returned, oldest first.</summary>
    public IReadOnlyList<Loan> LoanHistoryFor(MemberId memberId) => _loans.HistoryFor(memberId);

    // ---------------------------------------------------------------- Helpers

    private Result<Loan> FindActiveLoan(LoanId loanId)
    {
        if (_loans.Find(loanId) is not { } loan)
        {
            return Result.Failure<Loan>(new DomainError(ErrorCodes.LoanNotFound, $"There is no loan {loanId}."));
        }

        return loan.IsActive
            ? Result.Success(loan)
            : Result.Failure<Loan>(new DomainError(
                ErrorCodes.LoanAlreadyReturned, $"Loan {loanId} was already returned on {loan.ReturnedOn:yyyy-MM-dd}."));
    }

    // A loan always points at an existing member and book; if not, storage is corrupt.
    private Member RequireMember(MemberId id) =>
        _members.Find(id) ?? throw new InvalidOperationException($"Member {id} referenced by a loan does not exist.");

    private Book RequireBook(Isbn isbn) =>
        _books.Find(isbn) ?? throw new InvalidOperationException($"Book {isbn} referenced by a loan does not exist.");

    private static DomainError MemberNotFound(MemberId id) =>
        new(ErrorCodes.MemberNotFound, $"There is no member {id}.");

    private BookAvailability WithAvailability(Book book) =>
        new(book, book.TotalCopies - _loans.CountActiveFor(book.Isbn));

    private List<BookAvailability> Sorted(IEnumerable<Book> books) =>
        [.. books
            .OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase)
            .Select(WithAvailability)];

    private static DomainError BookNotFound(Isbn isbn) =>
        new(ErrorCodes.BookNotFound, $"No book with ISBN {isbn} is in the catalogue.");
}

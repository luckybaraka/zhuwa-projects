using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Policies;

namespace LibraryManagement.Core.Tests;

/// <summary>Readable builders so each test only states what matters to it.</summary>
internal static class TestData
{
    public static readonly Isbn CleanCode = Isbn.Parse("9780132350884");
    public static readonly Isbn DesignPatterns = Isbn.Parse("9780201633610");
    public static readonly Isbn Refactoring = Isbn.Parse("9780134757599");
    public static readonly Isbn Dictionary = Isbn.Parse("9780199571123");

    public static readonly DateOnly Today = new(2026, 11, 2);

    public static Book Book(
        Isbn? isbn = null,
        BookCategory category = BookCategory.General,
        int copies = 1,
        string title = "Clean Code",
        string author = "Robert C. Martin") =>
        new(isbn ?? CleanCode, title, author, category, copies);

    public static Member Member(
        MembershipPolicy? policy = null,
        int id = 1,
        string name = "Amina Otieno",
        string email = "amina@example.org") =>
        new(new MemberId(id), name, EmailAddress.Parse(email), policy ?? MembershipPolicy.Student);

    public static Loan Loan(
        int id = 1,
        int memberId = 1,
        Isbn? isbn = null,
        DateOnly? borrowedOn = null,
        int periodDays = 14)
    {
        var start = borrowedOn ?? Today;
        return new Loan(new LoanId(id), new MemberId(memberId), isbn ?? CleanCode, start, start.AddDays(periodDays));
    }
}

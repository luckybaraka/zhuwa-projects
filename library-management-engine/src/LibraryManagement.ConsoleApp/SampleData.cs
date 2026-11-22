using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp;

/// <summary>
/// A small starter catalogue and one member of each type, so the app can be explored
/// straight away. Loaded through the public service API, so it obeys every rule.
/// Start the app with <c>--empty</c> to skip it.
/// </summary>
public static class SampleData
{
    public static void Load(LibraryService library)
    {
        ArgumentNullException.ThrowIfNull(library);

        AddBook(library, "978-0-13-235088-4", "Clean Code", "Robert C. Martin", BookCategory.General, 3);
        AddBook(library, "978-0-201-63361-0", "Design Patterns", "Gamma, Helm, Johnson, Vlissides", BookCategory.General, 2);
        AddBook(library, "978-0-13-475759-9", "Refactoring (2nd ed.)", "Martin Fowler", BookCategory.General, 2);
        AddBook(library, "978-0-262-03384-8", "Introduction to Algorithms", "Cormen, Leiserson, Rivest, Stein", BookCategory.General, 1);
        AddBook(library, "978-0-13-468599-1", "Effective Java (3rd ed.)", "Joshua Bloch", BookCategory.ShortLoan, 1);
        AddBook(library, "978-0-321-12521-7", "Domain-Driven Design", "Eric Evans", BookCategory.ShortLoan, 1);
        AddBook(library, "978-0-19-957112-3", "Oxford Dictionary of English", "Oxford University Press", BookCategory.Reference, 1);

        Register(library, "Amina Otieno", "amina.otieno@example.org", MembershipType.Student);
        Register(library, "Brian Mwangi", "brian.mwangi@example.org", MembershipType.Staff);
        Register(library, "Grace Wanjiru", "grace.wanjiru@example.org", MembershipType.Guest);
    }

    // .Value throws if the seed data ever breaks a rule, which is a bug to catch immediately.
    private static void AddBook(LibraryService library, string isbn, string title, string author, BookCategory category, int copies) =>
        _ = library.AddBook(Isbn.Parse(isbn), title, author, category, copies).Value;

    private static void Register(LibraryService library, string name, string email, MembershipType type) =>
        _ = library.RegisterMember(name, EmailAddress.Parse(email), type).Value;
}

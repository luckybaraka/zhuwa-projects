using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;
using Microsoft.Extensions.Time.Testing;

namespace LibraryManagement.Core.Tests.Services;

/// <summary>
/// A fresh in-memory library per test, with a controllable clock fixed at
/// <see cref="TestData.Today"/> so due dates and fines are deterministic.
/// </summary>
public abstract class LibraryServiceTestBase
{
    protected LibraryServiceTestBase()
    {
        Clock = new FakeTimeProvider(new DateTimeOffset(TestData.Today.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero));
        Library = LibraryService.CreateInMemory(Clock);
    }

    protected FakeTimeProvider Clock { get; }

    protected LibraryService Library { get; }

    protected DateOnly Today => Library.Today;

    protected void AdvanceDays(int days) => Clock.Advance(TimeSpan.FromDays(days));

    protected Book AddBook(
        Isbn? isbn = null,
        BookCategory category = BookCategory.General,
        int copies = 1,
        string title = "Clean Code") =>
        Library.AddBook(isbn ?? TestData.CleanCode, title, "Robert C. Martin", category, copies).Value;

    protected Member Register(
        MembershipType type = MembershipType.Student,
        string name = "Amina Otieno",
        string? email = null) =>
        Library.RegisterMember(name, EmailAddress.Parse(email ?? $"{Guid.NewGuid():N}@example.org"), type).Value;
}

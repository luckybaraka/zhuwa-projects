using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Rules;

/// <summary>
/// Everything a borrowing rule needs to decide, gathered once by the service so that rules
/// are pure functions: no repositories, no clock, trivially unit-testable.
/// </summary>
/// <param name="Member">The member asking to borrow.</param>
/// <param name="Book">The title they want.</param>
/// <param name="AvailableCopies">Copies of the title currently on the shelf.</param>
/// <param name="MemberActiveLoans">Loans the member has not yet returned.</param>
/// <param name="Today">The library's current date.</param>
public sealed record BorrowingContext(
    Member Member,
    Book Book,
    int AvailableCopies,
    IReadOnlyCollection<Loan> MemberActiveLoans,
    DateOnly Today);

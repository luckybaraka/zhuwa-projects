using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Persistence;

/// <summary>Storage for loans, current and returned (returned loans are kept as history).</summary>
public interface ILoanRepository
{
    LoanId NextId();

    Loan? Find(LoanId id);

    /// <summary>Loans not yet returned, oldest first.</summary>
    IReadOnlyList<Loan> Active();

    IReadOnlyList<Loan> ActiveFor(MemberId memberId);

    /// <summary>Copies of a title currently out on loan.</summary>
    int CountActiveFor(Isbn isbn);

    /// <summary>A member's full loan history, oldest first.</summary>
    IReadOnlyList<Loan> HistoryFor(MemberId memberId);

    void Add(Loan loan);
}

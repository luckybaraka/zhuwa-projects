using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Persistence.InMemory;

/// <summary>
/// Process-local loan storage. Queries scan the list, which is fine for a single branch's
/// volumes; a database implementation would index member id and ISBN instead.
/// </summary>
public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly List<Loan> _loans = [];
    private int _lastId;

    public LoanId NextId() => new(++_lastId);

    public Loan? Find(LoanId id) => _loans.Find(loan => loan.Id == id);

    public IReadOnlyList<Loan> Active() => [.. _loans.Where(loan => loan.IsActive)];

    public IReadOnlyList<Loan> ActiveFor(MemberId memberId) =>
        [.. _loans.Where(loan => loan.IsActive && loan.MemberId == memberId)];

    public int CountActiveFor(Isbn isbn) => _loans.Count(loan => loan.IsActive && loan.Isbn == isbn);

    public IReadOnlyList<Loan> HistoryFor(MemberId memberId) =>
        [.. _loans.Where(loan => loan.MemberId == memberId)];

    public void Add(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (Find(loan.Id) is not null)
        {
            throw new InvalidOperationException($"Loan {loan.Id} is already stored.");
        }

        _loans.Add(loan);
    }
}

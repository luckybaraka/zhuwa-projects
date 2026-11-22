using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Services;

/// <summary>What happened when an item came back: how late it was and what was charged.</summary>
public sealed record ReturnReceipt(Loan Loan, Member Member, Book Book, int DaysOverdue, decimal FineCharged)
{
    public bool WasLate => DaysOverdue > 0;
}

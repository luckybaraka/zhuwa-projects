using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Services;

/// <summary>One line of the overdue report.</summary>
public sealed record OverdueLoan(Loan Loan, Member Member, Book Book, int DaysOverdue, decimal FineIfReturnedToday);

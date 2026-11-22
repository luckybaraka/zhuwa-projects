using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Domain;

/// <summary>One copy of a book lent to one member, from borrowing until return.</summary>
/// <remarks>
/// A loan refers to its member and book by id rather than holding object references.
/// That keeps the object graph flat and maps directly onto a table if persistence is added.
/// </remarks>
public sealed class Loan
{
    internal Loan(LoanId id, MemberId memberId, Isbn isbn, DateOnly borrowedOn, DateOnly dueOn)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        if (dueOn <= borrowedOn)
        {
            throw new ArgumentException("A loan must be due after the day it was borrowed.", nameof(dueOn));
        }

        Id = id;
        MemberId = memberId;
        Isbn = isbn;
        BorrowedOn = borrowedOn;
        DueOn = dueOn;
    }

    public LoanId Id { get; }

    public MemberId MemberId { get; }

    public Isbn Isbn { get; }

    public DateOnly BorrowedOn { get; }

    public DateOnly DueOn { get; private set; }

    public DateOnly? ReturnedOn { get; private set; }

    public int RenewalCount { get; private set; }

    /// <summary>The fine charged when the item came back (zero while active or if on time).</summary>
    public decimal FineCharged { get; private set; }

    public bool IsActive => ReturnedOn is null;

    /// <summary>True when the item is still out and <paramref name="today"/> is past its due date.</summary>
    public bool IsOverdueOn(DateOnly today) => IsActive && today > DueOn;

    /// <summary>Whole days past the due date as of <paramref name="date"/> (never negative).</summary>
    public int DaysOverdueOn(DateOnly date) => Math.Max(0, date.DayNumber - DueOn.DayNumber);

    internal void Renew(DateOnly newDueOn)
    {
        EnsureActive();
        if (newDueOn <= DueOn)
        {
            throw new ArgumentException("A renewal must move the due date later.", nameof(newDueOn));
        }

        DueOn = newDueOn;
        RenewalCount++;
    }

    internal void MarkReturned(DateOnly returnedOn, decimal fine)
    {
        EnsureActive();
        if (returnedOn < BorrowedOn)
        {
            throw new ArgumentException("An item cannot be returned before it was borrowed.", nameof(returnedOn));
        }

        ReturnedOn = returnedOn;
        FineCharged = Guard.NotNegative(fine, nameof(fine));
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException($"Loan {Id} has already been returned.");
        }
    }
}

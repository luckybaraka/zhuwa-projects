using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Services;

/// <summary>A book together with how many of its copies are on the shelf right now.</summary>
public sealed record BookAvailability(Book Book, int AvailableCopies)
{
    public int CopiesOnLoan => Book.TotalCopies - AvailableCopies;
}

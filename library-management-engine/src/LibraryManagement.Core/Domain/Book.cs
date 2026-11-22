using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Domain;

/// <summary>
/// A catalogue entry for one title, identified by ISBN, with a number of physical copies.
/// </summary>
/// <remarks>
/// The book deliberately does not store an "available copies" counter. Availability is
/// derived from active loans (total copies minus copies on loan), so there is one source
/// of truth and no counter that can drift out of sync with the loan records.
/// </remarks>
public sealed class Book
{
    public const int MaxTitleLength = 200;
    public const int MaxAuthorLength = 100;
    public const int MaxCopies = 100;

    public Book(Isbn isbn, string title, string author, BookCategory category, int copies)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown book category.");
        }

        Isbn = isbn;
        Title = Guard.NotBlank(title, MaxTitleLength, nameof(title));
        Author = Guard.NotBlank(author, MaxAuthorLength, nameof(author));
        Category = category;
        TotalCopies = Guard.InRange(copies, 1, MaxCopies, nameof(copies));
    }

    public Isbn Isbn { get; }

    public string Title { get; }

    public string Author { get; }

    public BookCategory Category { get; }

    public int TotalCopies { get; private set; }

    /// <summary>True when adding <paramref name="count"/> copies stays within <see cref="MaxCopies"/>.</summary>
    public bool CanAddCopies(int count) => count > 0 && TotalCopies + count <= MaxCopies;

    internal void AddCopies(int count)
    {
        if (!CanAddCopies(count))
        {
            throw new ArgumentOutOfRangeException(
                nameof(count), count, $"Copies must be positive and the total must not exceed {MaxCopies}.");
        }

        TotalCopies += count;
    }

    public override string ToString() => $"{Title} by {Author} ({Isbn})";
}

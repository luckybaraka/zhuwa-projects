using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Persistence;

/// <summary>Storage for catalogue entries, keyed by ISBN.</summary>
public interface IBookRepository
{
    Book? Find(Isbn isbn);

    /// <summary>Every book, in the order they were added.</summary>
    IReadOnlyList<Book> All();

    void Add(Book book);
}

using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Persistence.InMemory;

/// <summary>Process-local book storage. Data is lost when the program exits.</summary>
public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly Dictionary<Isbn, Book> _byIsbn = [];
    private readonly List<Book> _inOrder = [];

    public Book? Find(Isbn isbn) => _byIsbn.GetValueOrDefault(isbn);

    public IReadOnlyList<Book> All() => _inOrder.AsReadOnly();

    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        if (!_byIsbn.TryAdd(book.Isbn, book))
        {
            throw new InvalidOperationException($"A book with ISBN {book.Isbn} is already stored.");
        }

        _inOrder.Add(book);
    }
}

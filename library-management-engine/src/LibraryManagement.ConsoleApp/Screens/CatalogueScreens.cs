using System.Globalization;
using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Screens;

/// <summary>Adding, listing and searching books.</summary>
internal sealed class CatalogueScreens(LibraryService library, Prompter prompter, TextWriter output)
    : Screen(library, prompter, output)
{
    /// <summary>One entry point for new titles and extra copies: the ISBN decides which.</summary>
    public void AddBookOrCopies()
    {
        Heading("Add a book or copies");
        var isbn = Prompter.ReadIsbn("ISBN (10 or 13 digits, hyphens allowed)");

        if (Library.FindBook(isbn) is { } existing)
        {
            AddCopies(existing);
            return;
        }

        var title = Prompter.ReadText("Title", Book.MaxTitleLength);
        var author = Prompter.ReadText("Author", Book.MaxAuthorLength);
        Info("Category:");
        var category = Prompter.ReadChoice<BookCategory>("Category");
        var copies = Prompter.ReadWholeNumber("Number of copies", 1, Book.MaxCopies);

        var result = Library.AddBook(isbn, title, author, category, copies);
        if (result.IsFailure)
        {
            Refused("the book was not added.", result);
            return;
        }

        Done($"added '{result.Value.Title}' ({category}, {copies} cop{(copies == 1 ? "y" : "ies")}).");
    }

    public void ListBooks()
    {
        Heading("Catalogue", interactive: false);
        ShowBooks(Library.ListBooks(), "The catalogue is empty. Use 'Add a book' first.");
    }

    public void SearchBooks()
    {
        Heading("Search books");
        var query = Prompter.ReadText("Title, author or ISBN to search for", Book.MaxTitleLength);
        ShowBooks(Library.SearchBooks(query), $"No books match '{query}'.");
    }

    private void AddCopies(BookAvailability existing)
    {
        var book = existing.Book;
        Info($"Already catalogued: '{book.Title}' by {book.Author} ({book.TotalCopies} copies).");
        if (!book.CanAddCopies(1))
        {
            Info($"It already has the maximum of {Book.MaxCopies} copies.");
            return;
        }

        var count = Prompter.ReadWholeNumber("Copies to add", 1, Book.MaxCopies - book.TotalCopies);
        var result = Library.AddCopies(book.Isbn, count);
        if (result.IsFailure)
        {
            Refused("no copies were added.", result);
            return;
        }

        Done($"'{book.Title}' now has {result.Value.TotalCopies} copies.");
    }

    private void ShowBooks(IReadOnlyList<BookAvailability> books, string emptyMessage)
    {
        if (books.Count == 0)
        {
            Info(emptyMessage);
            return;
        }

        Table.Write(
            Output,
            ["ISBN", "Title", "Author", "Category", "On shelf"],
            books.Select(entry => (IReadOnlyList<string>)
            [
                entry.Book.Isbn.Value,
                entry.Book.Title,
                entry.Book.Author,
                entry.Book.Category.ToString(),
                string.Create(CultureInfo.InvariantCulture, $"{entry.AvailableCopies}/{entry.Book.TotalCopies}"),
            ]));
    }
}

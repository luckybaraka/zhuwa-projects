using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.ConsoleApp.Screens;
using LibraryManagement.ConsoleApp.Time;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp;

/// <summary>
/// The interactive menu loop. It repeats until the user chooses Exit or input is closed,
/// and turns a "cancel" typed inside any operation into a safe return to the menu.
/// </summary>
public sealed class LibraryApp
{
    private const string ExitKey = "0";

    private readonly LibraryService _library;
    private readonly Prompter _prompter;
    private readonly TextWriter _output;
    private readonly IReadOnlyList<MenuItem> _menu;

    public LibraryApp(LibraryService library, AdjustableClock clock, TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(library);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        _library = library;
        _output = output;
        _prompter = new Prompter(input, output);

        var catalogue = new CatalogueScreens(library, _prompter, output);
        var members = new MemberScreens(library, _prompter, output);
        var circulation = new CirculationScreens(library, _prompter, output);
        var admin = new AdminScreens(library, _prompter, output, clock);

        _menu =
        [
            new("1", "Catalogue", "Add a book or copies", catalogue.AddBookOrCopies),
            new("2", "Catalogue", "List all books", catalogue.ListBooks),
            new("3", "Catalogue", "Search books", catalogue.SearchBooks),
            new("4", "Members", "Register a member", members.RegisterMember),
            new("5", "Members", "List members", members.ListMembers),
            new("6", "Members", "Member account (loans and fines)", members.ShowAccount),
            new("7", "Circulation", "Borrow a book", circulation.Borrow),
            new("8", "Circulation", "Return a book", circulation.Return),
            new("9", "Circulation", "Renew a loan", circulation.Renew),
            new("10", "Circulation", "Pay a fine", circulation.PayFine),
            new("11", "Circulation", "Overdue report", circulation.OverdueReport),
            new("12", "Other", "Borrowing rules by membership", admin.ShowRules),
            new("13", "Other", "Advance the date (simulate time passing)", admin.AdvanceDate),
        ];
    }

    /// <returns>The process exit code (always 0: every way out is a normal exit).</returns>
    public int Run()
    {
        _output.WriteLine("Library Management Engine");
        try
        {
            while (true)
            {
                try
                {
                    ShowMenu();
                    var choice = _prompter.Read("Choose an option", ParseMenuChoice);
                    if (choice is null)
                    {
                        _output.WriteLine("Goodbye.");
                        return 0;
                    }

                    choice.Run();
                }
                catch (InputCancelledException)
                {
                    _output.WriteLine("  Cancelled. Nothing was changed.");
                }
            }
        }
        catch (EndOfInputException)
        {
            _output.WriteLine();
            _output.WriteLine("Input closed. Goodbye.");
            return 0;
        }
    }

    private void ShowMenu()
    {
        _output.WriteLine();
        _output.WriteLine($"=== Main menu | today: {Display.Date(_library.Today)} ===");
        foreach (var section in _menu.GroupBy(item => item.Section))
        {
            _output.WriteLine($" {section.Key}");
            foreach (var item in section)
            {
                _output.WriteLine($"  {item.Key,3}) {item.Label}");
            }
        }

        _output.WriteLine($"  {ExitKey,3}) Exit");
    }

    // A null value means "exit".
    private ParseOutcome<MenuItem?> ParseMenuChoice(string input)
    {
        if (input == ExitKey)
        {
            return ParseOutcome.Valid<MenuItem?>(null);
        }

        var match = _menu.FirstOrDefault(item => item.Key == input);
        return match is not null
            ? ParseOutcome.Valid<MenuItem?>(match)
            : ParseOutcome.Invalid<MenuItem?>($"Choose a number from the menu (0-{_menu.Count}).");
    }

    private sealed record MenuItem(string Key, string Section, string Label, Action Run);
}

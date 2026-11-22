using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Screens;

/// <summary>
/// Shared plumbing for every group of screens: access to the library, input and output,
/// plus the "pick an existing member/book/loan" prompts several screens need.
/// Screens only translate between text and <see cref="LibraryService"/> calls; they make
/// no business decisions of their own.
/// </summary>
internal abstract class Screen
{
    protected Screen(LibraryService library, Prompter prompter, TextWriter output)
    {
        Library = library;
        Prompter = prompter;
        Output = output;
    }

    protected LibraryService Library { get; }

    protected Prompter Prompter { get; }

    protected TextWriter Output { get; }

    protected void Heading(string title, bool interactive = true)
    {
        Output.WriteLine();
        Output.WriteLine($"--- {title} ---");
        if (interactive)
        {
            Output.WriteLine($"  (Type '{Prompter.CancelWord}' at any prompt to return to the menu.)");
        }
    }

    protected void Done(string message) => Output.WriteLine($"  Done: {message}");

    protected void Info(string message) => Output.WriteLine($"  {message}");

    protected void Refused(string heading, Result result)
    {
        Output.WriteLine($"  Not possible: {heading}");
        foreach (var error in result.Errors)
        {
            Output.WriteLine($"   - {error.Message}");
        }
    }

    protected Member ReadExistingMember(string prompt = "Member id (e.g. M-0001)") =>
        Prompter.Read(prompt, input => Parsers.MemberId(input).Then(id =>
            Library.FindMember(id) is { } member
                ? ParseOutcome.Valid(member)
                : ParseOutcome.Invalid<Member>($"There is no member {id}. Use 'List members' to see ids.")));

    protected BookAvailability ReadExistingBook(string prompt = "Book ISBN") =>
        Prompter.Read(prompt, input => Parsers.Isbn(input).Then(isbn =>
            Library.FindBook(isbn) is { } book
                ? ParseOutcome.Valid(book)
                : ParseOutcome.Invalid<BookAvailability>($"ISBN {isbn} is not in the catalogue. Use 'Search books' to find it.")));

    protected Loan ReadActiveLoan(string prompt = "Loan id (e.g. L-0001)") =>
        Prompter.Read(prompt, input => Parsers.LoanId(input).Then(id => Library.FindLoan(id) switch
        {
            null => ParseOutcome.Invalid<Loan>($"There is no loan {id}."),
            { IsActive: false } loan => ParseOutcome.Invalid<Loan>(
                $"Loan {id} was already returned on {Display.Date(loan.ReturnedOn!.Value)}."),
            var loan => ParseOutcome.Valid(loan),
        }));

    /// <summary>Loans as a table, resolving ids to names and titles for display.</summary>
    protected void ShowLoans(IEnumerable<Loan> loans, bool includeMember = true)
    {
        var today = Library.Today;
        var rows = loans.Select(loan =>
        {
            List<string> row = [loan.Id.ToString()];
            if (includeMember)
            {
                row.Add(Library.FindMember(loan.MemberId)?.Name ?? loan.MemberId.ToString());
            }

            row.AddRange(
            [
                Library.FindBook(loan.Isbn)?.Book.Title ?? loan.Isbn.Value,
                Display.Date(loan.BorrowedOn),
                Display.Date(loan.DueOn),
                Display.LoanStatus(loan, today),
            ]);
            return (IReadOnlyList<string>)row;
        });

        List<string> headers = includeMember
            ? ["Loan", "Member", "Title", "Borrowed", "Due", "Status"]
            : ["Loan", "Title", "Borrowed", "Due", "Status"];
        Table.Write(Output, headers, rows);
    }
}

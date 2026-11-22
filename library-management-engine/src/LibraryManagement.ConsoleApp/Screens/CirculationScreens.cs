using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.Core.Common;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Screens;

/// <summary>Borrowing, returning, renewing, fines and the overdue report.</summary>
internal sealed class CirculationScreens(LibraryService library, Prompter prompter, TextWriter output)
    : Screen(library, prompter, output)
{
    public void Borrow()
    {
        Heading("Borrow a book");
        if (Library.ListMembers().Count == 0 || Library.ListBooks().Count == 0)
        {
            Info("You need at least one member and one book before anything can be borrowed.");
            return;
        }

        var member = ReadExistingMember();
        var book = ReadExistingBook().Book;

        var result = Library.Borrow(member.Id, book.Isbn);
        if (result.IsFailure)
        {
            Refused($"'{book.Title}' cannot be lent to {member.Name}:", result);
            return;
        }

        var loan = result.Value;
        var days = loan.DueOn.DayNumber - loan.BorrowedOn.DayNumber;
        Done($"lent '{book.Title}' to {member.Name}. Loan {loan.Id} is due back on " +
             $"{Display.Date(loan.DueOn)} ({Display.Days(days)}).");
    }

    public void Return()
    {
        Heading("Return a book");
        if (!ShowActiveLoans())
        {
            return;
        }

        var loan = ReadActiveLoan();
        var result = Library.Return(loan.Id);
        if (result.IsFailure)
        {
            Refused("the return was not recorded.", result);
            return;
        }

        var receipt = result.Value;
        if (!receipt.WasLate)
        {
            Done($"'{receipt.Book.Title}' returned by {receipt.Member.Name}, on time. No fine.");
            return;
        }

        Done($"'{receipt.Book.Title}' returned by {receipt.Member.Name}, {Display.Days(receipt.DaysOverdue)} late.");
        Info($"Fine charged: {Money.Format(receipt.FineCharged)}" +
             (receipt.FineCharged == 0 ? " (within the grace period)." : "."));
        Info($"{receipt.Member.Name} now owes {Money.Format(receipt.Member.OutstandingFines)}.");
    }

    public void Renew()
    {
        Heading("Renew a loan");
        if (!ShowActiveLoans())
        {
            return;
        }

        var loan = ReadActiveLoan();
        var result = Library.Renew(loan.Id);
        if (result.IsFailure)
        {
            Refused($"loan {loan.Id} cannot be renewed:", result);
            return;
        }

        Done($"loan {loan.Id} is now due on {Display.Date(result.Value.DueOn)} " +
             $"(renewal {result.Value.RenewalCount} of {Library.FindMember(loan.MemberId)!.Policy.MaxRenewals}).");
    }

    public void PayFine()
    {
        Heading("Pay a fine");
        var owing = Library.ListMembers().Where(member => member.OutstandingFines > 0).ToList();
        if (owing.Count == 0)
        {
            Info("No member owes any fines.");
            return;
        }

        Table.Write(
            Output,
            ["Id", "Name", "Owes"],
            owing.Select(member => (IReadOnlyList<string>)
                [member.Id.ToString(), member.Name, Money.Format(member.OutstandingFines)]));

        var payer = Prompter.Read("Member id", input => Parsers.MemberId(input).Then(id =>
            owing.Find(member => member.Id == id) is { } match
                ? ParseOutcome.Valid(match)
                : ParseOutcome.Invalid<Core.Domain.Member>($"{id} does not owe anything. Choose from the list above.")));
        var wasBlocked = payer.IsBlockedByFines;
        var amount = Prompter.ReadAmount($"Amount to pay (owes {Money.Format(payer.OutstandingFines)})", payer.OutstandingFines);

        var result = Library.PayFine(payer.Id, amount);
        if (result.IsFailure)
        {
            Refused("the payment was not recorded.", result);
            return;
        }

        Done($"received {Money.Format(amount)} from {payer.Name}. Remaining balance: {Money.Format(result.Value)}.");
        if (wasBlocked && !payer.IsBlockedByFines)
        {
            Info($"{payer.Name} can borrow again.");
        }
    }

    public void OverdueReport()
    {
        Heading("Overdue report", interactive: false);
        var overdue = Library.OverdueLoans();
        if (overdue.Count == 0)
        {
            Info($"Nothing is overdue as of {Display.Date(Library.Today)}.");
            return;
        }

        Table.Write(
            Output,
            ["Loan", "Member", "Title", "Due", "Days late", "Fine if returned today"],
            overdue.Select(line => (IReadOnlyList<string>)
            [
                line.Loan.Id.ToString(),
                $"{line.Member.Name} ({line.Member.MembershipType})",
                line.Book.Title,
                Display.Date(line.Loan.DueOn),
                Display.Days(line.DaysOverdue),
                Money.Format(line.FineIfReturnedToday),
            ]));
    }

    /// <summary>Shows every active loan; returns false (with a message) when there are none.</summary>
    private bool ShowActiveLoans()
    {
        var active = Library.ActiveLoans();
        if (active.Count == 0)
        {
            Info("No items are on loan right now.");
            return false;
        }

        ShowLoans(active);
        return true;
    }
}

using System.Globalization;
using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Screens;

/// <summary>Registering members and viewing their accounts.</summary>
internal sealed class MemberScreens(LibraryService library, Prompter prompter, TextWriter output)
    : Screen(library, prompter, output)
{
    public void RegisterMember()
    {
        Heading("Register a member");
        var name = Prompter.ReadText("Full name", Member.MaxNameLength);
        var email = Prompter.Read("E-mail", input => Parsers.Email(input).Then(address =>
            Library.FindMemberByEmail(address) is { } owner
                ? ParseOutcome.Invalid<EmailAddress>($"{address} is already registered to {owner.Name} ({owner.Id}).")
                : ParseOutcome.Valid(address)));
        Info("Membership type:");
        var type = Prompter.ReadChoice<MembershipType>("Membership");

        var result = Library.RegisterMember(name, email, type);
        if (result.IsFailure)
        {
            Refused("the member was not registered.", result);
            return;
        }

        var member = result.Value;
        Done($"registered {member.Name} as {member.Id} ({type}).");
        Info($"Allowance: {Allowance(member)}.");
    }

    public void ListMembers()
    {
        Heading("Members", interactive: false);
        var members = Library.ListMembers();
        if (members.Count == 0)
        {
            Info("No members yet. Use 'Register a member' first.");
            return;
        }

        Table.Write(
            Output,
            ["Id", "Name", "E-mail", "Type", "On loan", "Fines owed"],
            members.Select(member => (IReadOnlyList<string>)
            [
                member.Id.ToString(),
                member.Name,
                member.Email.Value,
                member.MembershipType.ToString(),
                string.Create(CultureInfo.InvariantCulture,
                    $"{Library.ActiveLoansFor(member.Id).Count}/{member.Policy.MaxActiveLoans}"),
                Money.Format(member.OutstandingFines),
            ]));
    }

    public void ShowAccount()
    {
        Heading("Member account");
        if (Library.ListMembers().Count == 0)
        {
            Info("No members yet.");
            return;
        }

        var member = ReadExistingMember();
        var active = Library.ActiveLoansFor(member.Id);
        var overdue = active.Count(loan => loan.IsOverdueOn(Library.Today));

        Output.WriteLine();
        Info($"{member.Name} ({member.Id}), {member.MembershipType} member");
        Info($"E-mail:      {member.Email}");
        Info($"Allowance:   {Allowance(member)}");
        Info($"On loan:     {active.Count} of {member.Policy.MaxActiveLoans}" +
             (overdue > 0 ? $" ({overdue} overdue)" : string.Empty));
        Info($"Fines owed:  {Money.Format(member.OutstandingFines)}" +
             (member.IsBlockedByFines
                 ? $" (borrowing blocked above {Money.Format(member.Policy.FineBlockThreshold)})"
                 : string.Empty));

        var history = Library.LoanHistoryFor(member.Id);
        if (history.Count == 0)
        {
            Info("No loans yet.");
            return;
        }

        Output.WriteLine();
        Info("Loan history:");
        ShowLoans(history, includeMember: false);
    }

    private static string Allowance(Member member)
    {
        var policy = member.Policy;
        return $"up to {policy.MaxActiveLoans} item(s) for {Display.Days(policy.LoanPeriodDays)}, " +
               $"{policy.MaxRenewals} renewal(s), {Money.Format(policy.DailyFine)} per overdue day";
    }
}

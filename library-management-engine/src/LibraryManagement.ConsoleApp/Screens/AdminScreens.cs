using System.Globalization;
using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.ConsoleApp.Time;
using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Policies;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Screens;

/// <summary>Explaining the rules, and moving the simulated date forward.</summary>
internal sealed class AdminScreens(LibraryService library, Prompter prompter, TextWriter output, AdjustableClock clock)
    : Screen(library, prompter, output)
{
    /// <summary>
    /// Prints the borrowing terms by asking the policies themselves, so this screen can never
    /// disagree with the rules actually enforced.
    /// </summary>
    public void ShowRules()
    {
        Heading("Borrowing rules", interactive: false);
        Table.Write(
            Output,
            ["Membership", "Max items", "Loan period", "Renewals", "Fine per day", "Blocked if owing over", "Notes"],
            Enum.GetValues<MembershipType>().Select(type =>
            {
                var policy = MembershipPolicy.For(type);
                return (IReadOnlyList<string>)
                [
                    type.ToString(),
                    policy.MaxActiveLoans.ToString(CultureInfo.InvariantCulture),
                    Display.Days(policy.LoanPeriodDays),
                    policy.MaxRenewals.ToString(CultureInfo.InvariantCulture),
                    Money.Format(policy.DailyFine),
                    Money.Format(policy.FineBlockThreshold),
                    Notes(policy),
                ];
            }));

        Output.WriteLine();
        Info("For everyone:");
        Info("- Reference books cannot be borrowed.");
        Info($"- Short-loan items are due within {Display.Days(MembershipPolicy.ShortLoanPeriodDays)} and cannot be renewed.");
        Info("- Overdue items must be returned before borrowing more, and cannot be renewed.");
        Info("- Only one copy of a title per member at a time.");
        Info($"- The fine for a single loan is capped at {Money.Format(MembershipPolicy.MaxFinePerLoan)}.");
    }

    public void AdvanceDate()
    {
        Heading("Advance the date (simulation)");
        Info($"Today is {Display.Date(Library.Today)}. Moving time forward lets you see due dates, overdue items and fines.");
        var days = Prompter.ReadWholeNumber("Days to move forward", 1, AdjustableClock.MaxAdvanceDays);

        clock.AdvanceDays(days);

        Done($"today is now {Display.Date(Library.Today)} ({Display.Days(clock.DaysAdvanced)} ahead of the real date).");
        var overdue = Library.OverdueLoans().Count;
        if (overdue > 0)
        {
            Info($"{overdue} loan(s) are now overdue. See 'Overdue report'.");
        }
    }

    // Derived from behaviour, not from the policy's type, so a new policy is described correctly for free.
    private static string Notes(MembershipPolicy policy)
    {
        List<string> notes = [];
        if (!policy.MayBorrow(BookCategory.ShortLoan))
        {
            notes.Add("general collection only");
        }

        var firstChargedDay = Enumerable.Range(1, 60).FirstOrDefault(day => policy.CalculateFine(day) > 0);
        if (firstChargedDay > 1)
        {
            notes.Add($"first {Display.Days(firstChargedDay - 1)} overdue free");
        }

        return notes.Count == 0 ? "-" : string.Join("; ", notes);
    }
}

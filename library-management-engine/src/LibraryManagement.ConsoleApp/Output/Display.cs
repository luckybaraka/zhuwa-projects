using System.Globalization;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.ConsoleApp.Output;

/// <summary>Consistent text for values that appear on many screens.</summary>
public static class Display
{
    public static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Days(int days) => days == 1 ? "1 day" : $"{days} days";

    /// <summary>Human status of an active loan relative to <paramref name="today"/>.</summary>
    public static string LoanStatus(Loan loan, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (!loan.IsActive)
        {
            return $"returned {Date(loan.ReturnedOn!.Value)}";
        }

        var daysLeft = loan.DueOn.DayNumber - today.DayNumber;
        return daysLeft switch
        {
            < 0 => $"OVERDUE {Days(-daysLeft)}",
            0 => "due today",
            _ => $"due in {Days(daysLeft)}",
        };
    }
}

using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>Overdue items must come back before anything new goes out.</summary>
public sealed class NoOverdueLoansRule : IBorrowingRule
{
    public DomainError? Check(BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var overdue = context.MemberActiveLoans.Count(loan => loan.IsOverdueOn(context.Today));

        return overdue == 0
            ? null
            : new DomainError(
                ErrorCodes.HasOverdueLoans,
                $"{context.Member.Name} has {overdue} overdue item(s) that must be returned first.");
    }
}

using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>Members may not hold more items than their membership allows.</summary>
public sealed class LoanLimitRule : IBorrowingRule
{
    public DomainError? Check(BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var policy = context.Member.Policy;

        return context.MemberActiveLoans.Count < policy.MaxActiveLoans
            ? null
            : new DomainError(
                ErrorCodes.LoanLimitReached,
                $"{context.Member.Name} already has {context.MemberActiveLoans.Count} items on loan; " +
                $"{policy.Type} members may hold at most {policy.MaxActiveLoans}.");
    }
}

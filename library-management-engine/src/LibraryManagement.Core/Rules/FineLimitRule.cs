using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>Unpaid fines above the membership's threshold block new loans.</summary>
public sealed class FineLimitRule : IBorrowingRule
{
    public DomainError? Check(BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var member = context.Member;

        return member.IsBlockedByFines
            ? new DomainError(
                ErrorCodes.FinesTooHigh,
                $"{member.Name} owes {Money.Format(member.OutstandingFines)}; {member.MembershipType} " +
                $"members cannot borrow while owing more than {Money.Format(member.Policy.FineBlockThreshold)}.")
            : null;
    }
}

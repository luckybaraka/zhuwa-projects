using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Policies;

/// <summary>
/// Staff: more items for longer, more renewals, and a grace period before fines start.
/// </summary>
public sealed class StaffPolicy : MembershipPolicy
{
    /// <summary>The first overdue days are not charged.</summary>
    public const int GracePeriodDays = 3;

    internal StaffPolicy()
    {
    }

    public override MembershipType Type => MembershipType.Staff;

    public override int MaxActiveLoans => 10;

    public override int LoanPeriodDays => 28;

    public override int MaxRenewals => 3;

    public override decimal DailyFine => 5m;

    public override decimal FineBlockThreshold => 500m;

    /// <summary>Only days beyond the grace period are charged.</summary>
    public override decimal CalculateFine(int daysOverdue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(daysOverdue);
        return base.CalculateFine(Math.Max(0, daysOverdue - GracePeriodDays));
    }
}

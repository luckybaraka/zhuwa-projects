using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Policies;

/// <summary>
/// Guests (walk-in or visiting readers): one general-collection item for a week, no
/// renewals, and no borrowing at all while any fine is unpaid.
/// </summary>
public sealed class GuestPolicy : MembershipPolicy
{
    internal GuestPolicy()
    {
    }

    public override MembershipType Type => MembershipType.Guest;

    public override int MaxActiveLoans => 1;

    public override int LoanPeriodDays => 7;

    public override int MaxRenewals => 0;

    public override decimal DailyFine => 20m;

    public override decimal FineBlockThreshold => 0m;

    /// <summary>High-demand short-loan items are kept for registered students and staff.</summary>
    public override bool MayBorrow(BookCategory category) => category == BookCategory.General;
}

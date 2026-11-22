using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Policies;

/// <summary>Students: a few items for two weeks, one renewal, and only small fines tolerated.</summary>
public sealed class StudentPolicy : MembershipPolicy
{
    internal StudentPolicy()
    {
    }

    public override MembershipType Type => MembershipType.Student;

    public override int MaxActiveLoans => 3;

    public override int LoanPeriodDays => 14;

    public override int MaxRenewals => 1;

    public override decimal DailyFine => 10m;

    public override decimal FineBlockThreshold => 100m;
}

using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Policies;

/// <summary>
/// The borrowing terms that come with a type of membership: how many items, for how long,
/// how many renewals, and how lateness is fined.
/// </summary>
/// <remarks>
/// Policies are objects that a <see cref="Member"/> holds (composition), rather than
/// subclasses of Member (inheritance). A member's identity (id, name, e-mail, fines)
/// does not change when the rules that apply to them change, and adding a new membership
/// type means adding one policy class, without editing Member or LibraryService.
/// Numeric limits are abstract properties; behaviour that really differs between types
/// (fine calculation, which categories may be borrowed) is a virtual method.
/// </remarks>
public abstract class MembershipPolicy
{
    /// <summary>Short-loan items are due within this many days, whatever the membership.</summary>
    public const int ShortLoanPeriodDays = 3;

    /// <summary>The fine for a single loan never exceeds this amount (roughly a replacement cost).</summary>
    public const decimal MaxFinePerLoan = 300m;

    public static MembershipPolicy Student { get; } = new StudentPolicy();

    public static MembershipPolicy Staff { get; } = new StaffPolicy();

    public static MembershipPolicy Guest { get; } = new GuestPolicy();

    public abstract MembershipType Type { get; }

    /// <summary>Most items the member may hold at once.</summary>
    public abstract int MaxActiveLoans { get; }

    /// <summary>Standard loan length for general-collection items.</summary>
    public abstract int LoanPeriodDays { get; }

    /// <summary>How many times one loan may be renewed.</summary>
    public abstract int MaxRenewals { get; }

    /// <summary>Fine charged per overdue day.</summary>
    public abstract decimal DailyFine { get; }

    /// <summary>Borrowing is blocked while unpaid fines are above this amount.</summary>
    public abstract decimal FineBlockThreshold { get; }

    public static MembershipPolicy For(MembershipType type) => type switch
    {
        MembershipType.Student => Student,
        MembershipType.Staff => Staff,
        MembershipType.Guest => Guest,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown membership type."),
    };

    /// <summary>Whether this membership may borrow items of the given category at all.</summary>
    public virtual bool MayBorrow(BookCategory category) => category != BookCategory.Reference;

    /// <summary>Loan length for an item, taking the category's own limit into account.</summary>
    public int LoanPeriodFor(BookCategory category) =>
        category == BookCategory.ShortLoan
            ? Math.Min(LoanPeriodDays, ShortLoanPeriodDays)
            : LoanPeriodDays;

    /// <summary>Fine for returning an item <paramref name="daysOverdue"/> days late, capped per loan.</summary>
    public virtual decimal CalculateFine(int daysOverdue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(daysOverdue);
        return Math.Min(daysOverdue * DailyFine, MaxFinePerLoan);
    }

    public override string ToString() => Type.ToString();
}

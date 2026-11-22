using LibraryManagement.Core.Common;
using LibraryManagement.Core.Policies;

namespace LibraryManagement.Core.Domain;

/// <summary>A registered library member and the fines they owe.</summary>
/// <remarks>
/// State changes (charging and paying fines) are <c>internal</c>: only the library
/// service can make them, as part of a return or a payment. Other code can read a
/// member's balance but cannot change it.
/// </remarks>
public sealed class Member
{
    public const int MaxNameLength = 100;

    public Member(MemberId id, string name, EmailAddress email, MembershipPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(policy);

        Id = id;
        Name = Guard.NotBlank(name, MaxNameLength, nameof(name));
        Email = email;
        Policy = policy;
    }

    public MemberId Id { get; }

    public string Name { get; }

    public EmailAddress Email { get; }

    public MembershipPolicy Policy { get; }

    public MembershipType MembershipType => Policy.Type;

    public decimal OutstandingFines { get; private set; }

    /// <summary>True when unpaid fines are high enough to block new loans.</summary>
    public bool IsBlockedByFines => OutstandingFines > Policy.FineBlockThreshold;

    internal void ChargeFine(decimal amount) =>
        OutstandingFines += Guard.NotNegative(amount, nameof(amount));

    internal void PayFine(decimal amount)
    {
        if (amount <= 0 || amount > OutstandingFines)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount), amount, "Payment must be positive and no more than the outstanding balance.");
        }

        OutstandingFines -= amount;
    }

    public override string ToString() => $"{Name} ({Id}, {MembershipType})";
}

namespace LibraryManagement.Core.Domain;

/// <summary>The kind of membership. Each type maps to one <see cref="Policies.MembershipPolicy"/>.</summary>
public enum MembershipType
{
    Student,
    Staff,
    Guest,
}

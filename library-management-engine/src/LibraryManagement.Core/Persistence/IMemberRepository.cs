using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Persistence;

/// <summary>Storage for members. Issues ids the way a database identity column would.</summary>
public interface IMemberRepository
{
    MemberId NextId();

    Member? Find(MemberId id);

    Member? FindByEmail(EmailAddress email);

    /// <summary>Every member, in registration order.</summary>
    IReadOnlyList<Member> All();

    void Add(Member member);
}

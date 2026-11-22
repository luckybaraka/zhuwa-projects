using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Persistence.InMemory;

/// <summary>Process-local member storage. Data is lost when the program exits.</summary>
public sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly Dictionary<MemberId, Member> _byId = [];
    private readonly Dictionary<EmailAddress, Member> _byEmail = [];
    private readonly List<Member> _inOrder = [];
    private int _lastId;

    public MemberId NextId() => new(++_lastId);

    public Member? Find(MemberId id) => _byId.GetValueOrDefault(id);

    public Member? FindByEmail(EmailAddress email) => _byEmail.GetValueOrDefault(email);

    public IReadOnlyList<Member> All() => _inOrder.AsReadOnly();

    public void Add(Member member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (_byId.ContainsKey(member.Id) || _byEmail.ContainsKey(member.Email))
        {
            throw new InvalidOperationException($"Member {member.Id} or e-mail {member.Email} is already stored.");
        }

        _byId.Add(member.Id, member);
        _byEmail.Add(member.Email, member);
        _inOrder.Add(member);
    }
}

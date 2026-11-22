namespace LibraryManagement.Core.Domain;

/// <summary>
/// Strongly-typed member identifier. A dedicated type (instead of a bare int) means the
/// compiler rejects passing a loan id where a member id is expected.
/// </summary>
public readonly record struct MemberId
{
    public const char Prefix = 'M';

    public MemberId(int value) =>
        Value = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Ids start at 1.");

    public int Value { get; }

    public static bool TryParse(string? input, out MemberId id)
    {
        var ok = EntityId.TryParse(input, Prefix, out var value);
        id = ok ? new MemberId(value) : default;
        return ok;
    }

    public override string ToString() => EntityId.Format(Prefix, Value);
}

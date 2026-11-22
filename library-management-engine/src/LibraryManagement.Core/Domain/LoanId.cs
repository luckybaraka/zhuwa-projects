namespace LibraryManagement.Core.Domain;

/// <summary>Strongly-typed loan identifier, formatted as "L-0001".</summary>
public readonly record struct LoanId
{
    public const char Prefix = 'L';

    public LoanId(int value) =>
        Value = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Ids start at 1.");

    public int Value { get; }

    public static bool TryParse(string? input, out LoanId id)
    {
        var ok = EntityId.TryParse(input, Prefix, out var value);
        id = ok ? new LoanId(value) : default;
        return ok;
    }

    public override string ToString() => EntityId.Format(Prefix, Value);
}

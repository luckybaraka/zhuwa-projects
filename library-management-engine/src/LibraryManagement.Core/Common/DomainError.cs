namespace LibraryManagement.Core.Common;

/// <summary>
/// A business-rule failure: a stable machine-readable <see cref="Code"/> (what tests and
/// callers branch on) plus a human-readable <see cref="Message"/> (what users see).
/// </summary>
public sealed record DomainError(string Code, string Message)
{
    public override string ToString() => $"{Code}: {Message}";
}

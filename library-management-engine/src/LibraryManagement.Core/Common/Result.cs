namespace LibraryManagement.Core.Common;

/// <summary>
/// Outcome of an operation that business rules can legitimately refuse
/// (e.g. "loan limit reached"). Refusals are expected, so they are values, not exceptions.
/// Exceptions are reserved for programming errors such as broken invariants.
/// </summary>
public class Result
{
    private protected Result(IReadOnlyList<DomainError> errors) => Errors = errors;

    /// <summary>All reasons the operation was refused; empty on success.</summary>
    public IReadOnlyList<DomainError> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new([]);

    public static Result Failure(DomainError error) => new([error]);

    public static Result<T> Success<T>(T value) => new(value, []);

    public static Result<T> Failure<T>(DomainError error) => new(default, [error]);

    public static Result<T> Failure<T>(IReadOnlyList<DomainError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("A failure needs at least one error.", nameof(errors));
        }

        return new(default, errors);
    }

    /// <summary>True when any error carries the given code.</summary>
    public bool HasError(string code) => Errors.Any(e => e.Code == code);
}

/// <summary>A <see cref="Result"/> that carries a value when successful.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, IReadOnlyList<DomainError> errors)
        : base(errors) => _value = value;

    /// <summary>The value. Reading it from a failed result is a programming error.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot read the value of a failed result: {string.Join("; ", Errors)}");
}

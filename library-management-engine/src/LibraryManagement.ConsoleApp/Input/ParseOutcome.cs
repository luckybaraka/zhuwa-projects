namespace LibraryManagement.ConsoleApp.Input;

/// <summary>The result of interpreting one line of user input: a value, or why it was rejected.</summary>
public readonly record struct ParseOutcome<T>
{
    internal ParseOutcome(bool isValid, T? value, string? error)
    {
        IsValid = isValid;
        Value = value;
        Error = error;
    }

    public bool IsValid { get; }

    public T? Value { get; }

    public string? Error { get; }

    /// <summary>Applies a further check to a valid value; invalid outcomes pass through unchanged.</summary>
    public ParseOutcome<TNext> Then<TNext>(Func<T, ParseOutcome<TNext>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return IsValid ? next(Value!) : ParseOutcome.Invalid<TNext>(Error!);
    }
}

/// <summary>Factory methods for <see cref="ParseOutcome{T}"/>.</summary>
public static class ParseOutcome
{
    public static ParseOutcome<T> Valid<T>(T value) => new(true, value, null);

    public static ParseOutcome<T> Invalid<T>(string error) => new(false, default, error);
}

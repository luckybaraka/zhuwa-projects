namespace LibraryManagement.ConsoleApp.Input;

/// <summary>
/// Standard input was closed (Ctrl+Z / Ctrl+D or the end of a piped script). The app
/// treats this as a request to exit cleanly instead of looping on empty input forever.
/// </summary>
public sealed class EndOfInputException : Exception
{
    public EndOfInputException()
        : base("Input was closed.")
    {
    }

    public EndOfInputException(string message)
        : base(message)
    {
    }

    public EndOfInputException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The user typed the cancel word to abandon the current operation.</summary>
public sealed class InputCancelledException : Exception
{
    public InputCancelledException()
        : base("The operation was cancelled by the user.")
    {
    }

    public InputCancelledException(string message)
        : base(message)
    {
    }

    public InputCancelledException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

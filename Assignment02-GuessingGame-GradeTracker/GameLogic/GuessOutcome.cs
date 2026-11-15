namespace GameLogic;

/// <summary>
/// The result of comparing a player's guess against the secret target.
/// </summary>
public enum GuessOutcome
{
    /// <summary>The guess was smaller than the target.</summary>
    TooLow,

    /// <summary>The guess was larger than the target.</summary>
    TooHigh,

    /// <summary>The guess matched the target exactly.</summary>
    Correct
}

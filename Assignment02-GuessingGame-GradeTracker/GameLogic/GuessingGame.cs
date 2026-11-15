namespace GameLogic;

/// <summary>
/// Core logic for a single round of the number guessing game.
///
/// This type holds no console dependencies so it can be unit tested
/// directly. The target can be supplied explicitly (for deterministic
/// tests) or generated randomly via <see cref="CreateRandom"/>.
/// </summary>
public class GuessingGame
{
    /// <summary>The secret number the player is trying to find.</summary>
    public int Target { get; }

    /// <summary>Lowest number the target can be (inclusive).</summary>
    public int Low { get; }

    /// <summary>Highest number the target can be (inclusive).</summary>
    public int High { get; }

    /// <summary>Total number of guesses the player is allowed.</summary>
    public int MaxAttempts { get; }

    /// <summary>How many guesses have been made so far.</summary>
    public int AttemptsUsed { get; private set; }

    /// <summary>True once the player has guessed the target.</summary>
    public bool IsWon { get; private set; }

    /// <summary>Guesses still available to the player.</summary>
    public int AttemptsRemaining => MaxAttempts - AttemptsUsed;

    /// <summary>True when the game has been won or all attempts are used up.</summary>
    public bool IsOver => IsWon || AttemptsRemaining <= 0;

    /// <summary>
    /// Creates a game with an explicit target. Useful for tests.
    /// </summary>
    public GuessingGame(int target, int maxAttempts, int low = 1, int high = 100)
    {
        if (low > high)
            throw new ArgumentException("Low bound cannot exceed high bound.");
        if (maxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts),
                "There must be at least one attempt.");
        if (target < low || target > high)
            throw new ArgumentOutOfRangeException(nameof(target),
                $"Target {target} is outside the range {low}-{high}.");

        Target = target;
        MaxAttempts = maxAttempts;
        Low = low;
        High = high;
    }

    /// <summary>
    /// Creates a game whose target is chosen randomly within the range.
    /// </summary>
    public static GuessingGame CreateRandom(int maxAttempts, Random rng, int low = 1, int high = 100)
    {
        ArgumentNullException.ThrowIfNull(rng);
        if (low > high)
            throw new ArgumentException("Low bound cannot exceed high bound.");

        int target = rng.Next(low, high + 1);
        return new GuessingGame(target, maxAttempts, low, high);
    }

    /// <summary>
    /// Records a single guess and reports how it compares to the target.
    /// </summary>
    /// <exception cref="InvalidOperationException">The game is already over.</exception>
    public GuessOutcome Guess(int value)
    {
        if (IsOver)
            throw new InvalidOperationException("The game is already over.");

        AttemptsUsed++;

        if (value == Target)
        {
            IsWon = true;
            return GuessOutcome.Correct;
        }

        return value < Target ? GuessOutcome.TooLow : GuessOutcome.TooHigh;
    }
}

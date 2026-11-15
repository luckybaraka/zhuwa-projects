namespace GradeLogic;

/// <summary>
/// Pure statistics helpers over an array of scores. No console dependency,
/// so every method can be unit tested directly.
/// </summary>
public static class GradeCalculator
{
    /// <summary>Lowest valid score.</summary>
    public const int MinScore = 0;

    /// <summary>Highest valid score.</summary>
    public const int MaxScore = 100;

    /// <summary>Returns the mean of the scores.</summary>
    /// <exception cref="ArgumentException">The array is empty.</exception>
    public static double Average(int[] scores)
    {
        EnsureNotEmpty(scores);

        long total = 0;
        foreach (int score in scores)
            total += score;

        return (double)total / scores.Length;
    }

    /// <summary>Returns the highest score in the array.</summary>
    /// <exception cref="ArgumentException">The array is empty.</exception>
    public static int Highest(int[] scores)
    {
        EnsureNotEmpty(scores);

        int highest = scores[0];
        foreach (int score in scores)
            if (score > highest)
                highest = score;

        return highest;
    }

    /// <summary>Returns the lowest score in the array.</summary>
    /// <exception cref="ArgumentException">The array is empty.</exception>
    public static int Lowest(int[] scores)
    {
        EnsureNotEmpty(scores);

        int lowest = scores[0];
        foreach (int score in scores)
            if (score < lowest)
                lowest = score;

        return lowest;
    }

    /// <summary>True when the value is a valid score (0-100 inclusive).</summary>
    public static bool IsValidScore(int score) => score is >= MinScore and <= MaxScore;

    private static void EnsureNotEmpty(int[] scores)
    {
        ArgumentNullException.ThrowIfNull(scores);
        if (scores.Length == 0)
            throw new ArgumentException("Cannot compute statistics for an empty set of scores.");
    }
}

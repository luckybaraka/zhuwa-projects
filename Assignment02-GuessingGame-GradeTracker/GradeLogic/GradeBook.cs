namespace GradeLogic;

/// <summary>
/// Holds the students that have been entered and exposes class-wide
/// statistics (average, highest, lowest) over their scores.
/// </summary>
public class GradeBook
{
    private readonly List<Student> _students = new();

    /// <summary>The students entered so far, in insertion order.</summary>
    public IReadOnlyList<Student> Students => _students;

    /// <summary>How many students have been recorded.</summary>
    public int Count => _students.Count;

    /// <summary>True when no students have been entered yet.</summary>
    public bool IsEmpty => _students.Count == 0;

    /// <summary>
    /// Adds a student after validating the name and score.
    /// </summary>
    /// <exception cref="ArgumentException">The name is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The score is outside 0-100.</exception>
    public void Add(string name, int score)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name cannot be blank.", nameof(name));
        if (!GradeCalculator.IsValidScore(score))
            throw new ArgumentOutOfRangeException(nameof(score),
                $"Score must be between {GradeCalculator.MinScore} and {GradeCalculator.MaxScore}.");

        _students.Add(new Student(name.Trim(), score));
    }

    /// <summary>Returns the scores as an array, in insertion order.</summary>
    public int[] Scores()
    {
        var scores = new int[_students.Count];
        for (int i = 0; i < _students.Count; i++)
            scores[i] = _students[i].Score;
        return scores;
    }

    /// <summary>Average score across all students.</summary>
    public double Average() => GradeCalculator.Average(Scores());

    /// <summary>Highest score across all students.</summary>
    public int Highest() => GradeCalculator.Highest(Scores());

    /// <summary>Lowest score across all students.</summary>
    public int Lowest() => GradeCalculator.Lowest(Scores());
}

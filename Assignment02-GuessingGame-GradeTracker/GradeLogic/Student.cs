namespace GradeLogic;

/// <summary>
/// A single student with a name and one numeric score (0-100).
/// </summary>
public record Student(string Name, int Score)
{
    /// <summary>The letter grade (A-F) derived from this student's score.</summary>
    public char Grade => GradeCalculator.LetterGrade(Score);
}

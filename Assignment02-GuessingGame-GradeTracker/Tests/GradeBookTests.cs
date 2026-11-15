using GradeLogic;

namespace Tests;

public class LetterGradeTests
{
    [Theory]
    [InlineData(100, 'A')]
    [InlineData(90, 'A')]
    [InlineData(89, 'B')]
    [InlineData(80, 'B')]
    [InlineData(79, 'C')]
    [InlineData(70, 'C')]
    [InlineData(69, 'D')]
    [InlineData(60, 'D')]
    [InlineData(59, 'F')]
    [InlineData(0, 'F')]
    public void LetterGrade_MapsScoreToExpectedLetter(int score, char expected)
    {
        Assert.Equal(expected, GradeCalculator.LetterGrade(score));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void LetterGrade_OutOfRange_Throws(int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GradeCalculator.LetterGrade(score));
    }

    [Fact]
    public void Student_Grade_ReflectsScore()
    {
        Assert.Equal('A', new Student("Ada", 95).Grade);
        Assert.Equal('F', new Student("Bo", 40).Grade);
    }
}

public class GradeBookTests
{
    [Fact]
    public void NewGradeBook_IsEmpty()
    {
        var book = new GradeBook();
        Assert.True(book.IsEmpty);
        Assert.Equal(0, book.Count);
    }

    [Fact]
    public void Add_StoresStudentsInOrder()
    {
        var book = new GradeBook();
        book.Add("Alice", 90);
        book.Add("Bob", 70);

        Assert.Equal(2, book.Count);
        Assert.Equal(new[] { 90, 70 }, book.Scores());
        Assert.Equal("Alice", book.Students[0].Name);
        Assert.Equal("Bob", book.Students[1].Name);
    }

    [Fact]
    public void Add_TrimsName()
    {
        var book = new GradeBook();
        book.Add("  Carol  ", 88);
        Assert.Equal("Carol", book.Students[0].Name);
    }

    [Fact]
    public void Add_WithBlankName_Throws()
    {
        var book = new GradeBook();
        Assert.Throws<ArgumentException>(() => book.Add("   ", 50));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(150)]
    public void Add_WithInvalidScore_Throws(int score)
    {
        var book = new GradeBook();
        Assert.Throws<ArgumentOutOfRangeException>(() => book.Add("Dave", score));
    }

    [Fact]
    public void Statistics_ReflectAddedStudents()
    {
        var book = new GradeBook();
        book.Add("Alice", 95);
        book.Add("Bob", 72);
        book.Add("Carol", 58);

        Assert.Equal(75.0, book.Average());
        Assert.Equal(95, book.Highest());
        Assert.Equal(58, book.Lowest());
    }
}

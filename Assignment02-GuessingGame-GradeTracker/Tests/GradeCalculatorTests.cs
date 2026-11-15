using GradeLogic;

namespace Tests;

public class GradeCalculatorTests
{
    [Fact]
    public void Average_ReturnsMeanOfScores()
    {
        int[] scores = { 90, 80, 70 };
        Assert.Equal(80.0, GradeCalculator.Average(scores));
    }

    [Fact]
    public void Average_IsNotTruncatedToInteger()
    {
        int[] scores = { 90, 72, 58 }; // sum 220 / 3 = 73.33...
        Assert.Equal(73.333, GradeCalculator.Average(scores), precision: 3);
    }

    [Fact]
    public void Highest_ReturnsLargestScore()
    {
        int[] scores = { 55, 91, 73 };
        Assert.Equal(91, GradeCalculator.Highest(scores));
    }

    [Fact]
    public void Lowest_ReturnsSmallestScore()
    {
        int[] scores = { 55, 91, 73 };
        Assert.Equal(55, GradeCalculator.Lowest(scores));
    }

    [Fact]
    public void Statistics_WorkForSingleScore()
    {
        int[] scores = { 42 };
        Assert.Equal(42.0, GradeCalculator.Average(scores));
        Assert.Equal(42, GradeCalculator.Highest(scores));
        Assert.Equal(42, GradeCalculator.Lowest(scores));
    }

    [Fact]
    public void Average_OnEmptyArray_Throws()
    {
        Assert.Throws<ArgumentException>(() => GradeCalculator.Average(Array.Empty<int>()));
    }

    [Fact]
    public void Highest_OnEmptyArray_Throws()
    {
        Assert.Throws<ArgumentException>(() => GradeCalculator.Highest(Array.Empty<int>()));
    }

    [Fact]
    public void Lowest_OnEmptyArray_Throws()
    {
        Assert.Throws<ArgumentException>(() => GradeCalculator.Lowest(Array.Empty<int>()));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(-1, false)]
    [InlineData(101, false)]
    public void IsValidScore_ChecksRange(int score, bool expected)
    {
        Assert.Equal(expected, GradeCalculator.IsValidScore(score));
    }
}

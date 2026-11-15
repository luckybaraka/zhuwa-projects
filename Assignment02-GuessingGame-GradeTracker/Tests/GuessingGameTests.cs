using GameLogic;

namespace Tests;

public class GuessingGameTests
{
    [Fact]
    public void Guess_WhenEqualToTarget_ReturnsCorrectAndWins()
    {
        var game = new GuessingGame(target: 42, maxAttempts: 5);

        GuessOutcome outcome = game.Guess(42);

        Assert.Equal(GuessOutcome.Correct, outcome);
        Assert.True(game.IsWon);
        Assert.True(game.IsOver);
    }

    [Fact]
    public void Guess_WhenBelowTarget_ReturnsTooLow()
    {
        var game = new GuessingGame(target: 42, maxAttempts: 5);

        Assert.Equal(GuessOutcome.TooLow, game.Guess(10));
        Assert.False(game.IsWon);
    }

    [Fact]
    public void Guess_WhenAboveTarget_ReturnsTooHigh()
    {
        var game = new GuessingGame(target: 42, maxAttempts: 5);

        Assert.Equal(GuessOutcome.TooHigh, game.Guess(90));
        Assert.False(game.IsWon);
    }

    [Fact]
    public void Guess_TracksAttemptsUsedAndRemaining()
    {
        var game = new GuessingGame(target: 42, maxAttempts: 3);

        game.Guess(1);
        game.Guess(2);

        Assert.Equal(2, game.AttemptsUsed);
        Assert.Equal(1, game.AttemptsRemaining);
    }

    [Fact]
    public void Game_IsOver_WhenAllAttemptsUsedWithoutWinning()
    {
        var game = new GuessingGame(target: 42, maxAttempts: 2);

        game.Guess(1);
        game.Guess(2);

        Assert.True(game.IsOver);
        Assert.False(game.IsWon);
    }

    [Fact]
    public void Guess_AfterGameIsOver_Throws()
    {
        var game = new GuessingGame(target: 42, maxAttempts: 1);
        game.Guess(1); // uses the only attempt

        Assert.Throws<InvalidOperationException>(() => game.Guess(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Constructor_WithTargetOutOfRange_Throws(int target)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GuessingGame(target, maxAttempts: 5, low: 1, high: 100));
    }

    [Fact]
    public void Constructor_WithNonPositiveAttempts_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GuessingGame(target: 50, maxAttempts: 0));
    }

    [Fact]
    public void CreateRandom_ProducesTargetWithinRange()
    {
        var rng = new Random(12345); // fixed seed = deterministic test

        for (int i = 0; i < 1000; i++)
        {
            var game = GuessingGame.CreateRandom(maxAttempts: 5, rng, low: 1, high: 10);
            Assert.InRange(game.Target, 1, 10);
        }
    }

    [Fact]
    public void CreateRandom_WithNullRandom_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => GuessingGame.CreateRandom(maxAttempts: 5, rng: null!));
    }
}

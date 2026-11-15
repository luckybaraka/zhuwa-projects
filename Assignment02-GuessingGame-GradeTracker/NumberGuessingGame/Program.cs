using GameLogic;

namespace NumberGuessingGame;

/// <summary>
/// Console front-end for the number guessing game.
///
/// All game rules live in the <see cref="GameLogic"/> library; this class
/// only handles reading input and printing output. Work is split into small
/// methods instead of one long Main.
/// </summary>
public static class Program
{
    private const int Low = 1;
    private const int High = 100;

    private static readonly Random Rng = new();

    public static void Main()
    {
        Console.WriteLine("=== Number Guessing Game ===");
        RunMenu();
        Console.WriteLine("Thanks for playing. Goodbye!");
    }

    /// <summary>Shows the menu and keeps looping until the user quits.</summary>
    private static void RunMenu()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("1) Play (easy   - 10 tries)");
            Console.WriteLine("2) Play (normal -  7 tries)");
            Console.WriteLine("3) Play (hard   -  5 tries)");
            Console.WriteLine("4) Quit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();
            if (choice is null) // stdin closed (EOF / Ctrl+Z) — exit cleanly
                return;

            switch (choice.Trim())
            {
                case "1":
                    PlayRound(maxAttempts: 10);
                    break;
                case "2":
                    PlayRound(maxAttempts: 7);
                    break;
                case "3":
                    PlayRound(maxAttempts: 5);
                    break;
                case "4":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Please choose 1, 2, 3 or 4.");
                    break;
            }
        }
    }

    /// <summary>Plays one full round of the game.</summary>
    private static void PlayRound(int maxAttempts)
    {
        var game = GuessingGame.CreateRandom(maxAttempts, Rng, Low, High);
        Console.WriteLine();
        Console.WriteLine($"I'm thinking of a number between {Low} and {High}.");
        Console.WriteLine($"You have {game.MaxAttempts} tries. Good luck!");

        while (!game.IsOver)
        {
            int? guess = ReadGuess(game.AttemptsRemaining);
            if (guess is null) // stdin closed mid-round
                return;

            GuessOutcome outcome = game.Guess(guess.Value);

            switch (outcome)
            {
                case GuessOutcome.Correct:
                    Console.WriteLine($"Correct! You got it in {game.AttemptsUsed} tries. 🎉");
                    break;
                case GuessOutcome.TooLow:
                    Console.WriteLine("Too low. Aim higher.");
                    break;
                case GuessOutcome.TooHigh:
                    Console.WriteLine("Too high. Aim lower.");
                    break;
            }
        }

        if (!game.IsWon)
            Console.WriteLine($"Out of tries! The number was {game.Target}.");
    }

    /// <summary>
    /// Reads a valid integer guess, re-prompting on bad input.
    /// Returns null if stdin is closed (EOF).
    /// </summary>
    private static int? ReadGuess(int attemptsRemaining)
    {
        while (true)
        {
            Console.Write($"[{attemptsRemaining} left] Your guess ({Low}-{High}): ");
            string? input = Console.ReadLine();
            if (input is null)
                return null;

            if (int.TryParse(input, out int guess) && guess >= Low && guess <= High)
                return guess;

            Console.WriteLine($"Please enter a whole number between {Low} and {High}.");
        }
    }
}

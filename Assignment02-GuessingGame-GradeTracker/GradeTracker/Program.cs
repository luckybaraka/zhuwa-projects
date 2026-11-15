using GradeLogic;

namespace GradeTracker;

/// <summary>
/// Console front-end for the grade tracker.
///
/// All rules and statistics live in the <see cref="GradeLogic"/> library;
/// this class only reads input and prints output, split into small methods
/// instead of one long Main.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Grade Tracker ===");
        var book = new GradeBook();
        RunMenu(book);
        Console.WriteLine("Goodbye!");
    }

    /// <summary>Shows the menu and loops until the user quits.</summary>
    private static void RunMenu(GradeBook book)
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("1) Add a student score");
            Console.WriteLine("2) List students and grades");
            Console.WriteLine("3) Show summary (average / highest / lowest)");
            Console.WriteLine("4) Quit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    AddStudent(book);
                    break;
                case "2":
                    ListStudents(book);
                    break;
                case "3":
                    ShowSummary(book);
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

    /// <summary>Prompts for a name and score, then records the student.</summary>
    private static void AddStudent(GradeBook book)
    {
        Console.Write("Student name: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannot be blank. Student not added.");
            return;
        }

        int score = ReadScore();
        book.Add(name, score);
        Console.WriteLine($"Added {name} with score {score} (grade {GradeCalculator.LetterGrade(score)}).");
    }

    /// <summary>Lists every student with their score and letter grade.</summary>
    private static void ListStudents(GradeBook book)
    {
        if (book.IsEmpty)
        {
            Console.WriteLine("No students yet. Add one first.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Name",-20}{"Score",8}{"Grade",8}");
        Console.WriteLine(new string('-', 36));
        foreach (Student s in book.Students)
            Console.WriteLine($"{s.Name,-20}{s.Score,8}{s.Grade,8}");
    }

    /// <summary>Prints the class average, highest and lowest scores.</summary>
    private static void ShowSummary(GradeBook book)
    {
        if (book.IsEmpty)
        {
            Console.WriteLine("No students yet. Add one first.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Students : {book.Count}");
        Console.WriteLine($"Average  : {book.Average():0.00}");
        Console.WriteLine($"Highest  : {book.Highest()}");
        Console.WriteLine($"Lowest   : {book.Lowest()}");
    }

    /// <summary>Reads a valid score (0-100), re-prompting on bad input.</summary>
    private static int ReadScore()
    {
        while (true)
        {
            Console.Write($"Score ({GradeCalculator.MinScore}-{GradeCalculator.MaxScore}): ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int score) && GradeCalculator.IsValidScore(score))
                return score;

            Console.WriteLine($"Please enter a whole number between {GradeCalculator.MinScore} and {GradeCalculator.MaxScore}.");
        }
    }
}

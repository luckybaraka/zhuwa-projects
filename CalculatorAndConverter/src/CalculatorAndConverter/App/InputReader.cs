namespace CalculatorAndConverter.App;

public static class InputReader
{
    public static int ReadMenuChoice(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string line = ReadLineOrCancel();

            if (int.TryParse(line, out int value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Please enter a whole number between {min} and {max}.");
        }
    }

    public static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string line = ReadLineOrCancel();

            if (double.TryParse(line, out double value))
                return value;

            Console.WriteLine("That is not a valid number. Please try again.");
        }
    }

    public static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        return ReadLineOrCancel();
    }

    public static IReadOnlyList<double> ReadNumbers(string prompt, int minCount)
    {
        Console.WriteLine(prompt);
        var values = new List<double>();

        while (true)
        {
            Console.Write($"  Number {values.Count + 1} (blank to finish): ");
            string line = ReadLineOrCancel();

            if (line.Length == 0)
            {
                if (values.Count >= minCount)
                    return values;

                Console.WriteLine($"Please enter at least {minCount} numbers.");
                continue;
            }

            if (double.TryParse(line, out double value))
                values.Add(value);
            else
                Console.WriteLine("That is not a valid number. Please try again.");
        }
    }

    public static decimal ReadPositiveDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string line = ReadLineOrCancel();

            if (decimal.TryParse(line, out decimal value) && value > 0)
                return value;

            Console.WriteLine("Please enter a number greater than zero.");
        }
    }

    private static string ReadLineOrCancel()
    {
        string? line = Console.ReadLine();
        if (line is null)
            throw new InputCancelledException();

        return line;
    }
}

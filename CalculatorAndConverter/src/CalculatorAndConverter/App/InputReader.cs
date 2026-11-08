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

    public static decimal ReadPositiveDecimalOrDefault(string prompt, decimal defaultValue)
    {
        while (true)
        {
            Console.Write(prompt);
            string line = ReadLineOrCancel();

            if (string.IsNullOrWhiteSpace(line))
                return defaultValue;

            if (decimal.TryParse(line, out decimal value) && value > 0)
                return value;

            Console.WriteLine("Please enter a number greater than zero, or press Enter for the default.");
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

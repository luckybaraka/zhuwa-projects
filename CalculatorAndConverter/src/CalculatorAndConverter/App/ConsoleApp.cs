using CalculatorAndConverter.Core;

namespace CalculatorAndConverter.App;

public sealed class ConsoleApp
{
    public void Run()
    {
        Console.WriteLine("=== Calculator & Unit Converter ===");

        try
        {
            MainLoop();
        }
        catch (InputCancelledException)
        {
            Console.WriteLine();
        }

        Console.WriteLine("Goodbye!");
    }

    private static void MainLoop()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Main menu");
            Console.WriteLine("1. Calculator");
            Console.WriteLine("2. Unit converter");
            Console.WriteLine("3. Currency converter (GHS to USD)");
            Console.WriteLine("4. Exit");

            switch (InputReader.ReadMenuChoice("Choose an option: ", 1, 4))
            {
                case 1: RunCalculator(); break;
                case 2: RunUnitConverter(); break;
                case 3: RunCurrencyConverter(); break;
                case 4: return;
            }
        }
    }

    private static void RunCalculator()
    {
        Console.WriteLine();
        Console.WriteLine("Calculator");
        Console.WriteLine("1. Add");
        Console.WriteLine("2. Subtract");
        Console.WriteLine("3. Multiply");
        Console.WriteLine("4. Divide");
        Console.WriteLine("5. Evaluate an expression (e.g. 2 * 4 - 3 / 5)");
        Console.WriteLine("6. Back");

        int choice = InputReader.ReadMenuChoice("Choose an operation: ", 1, 6);
        if (choice == 6)
            return;

        if (choice == 5)
        {
            RunExpression();
            return;
        }

        IReadOnlyList<double> numbers = InputReader.ReadNumbers("Enter the numbers (at least two):", 2);

        try
        {
            double result = choice switch
            {
                1 => Calculator.Add(numbers),
                2 => Calculator.Subtract(numbers),
                3 => Calculator.Multiply(numbers),
                _ => Calculator.Divide(numbers),
            };

            Console.WriteLine($"Result: {result}");
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void RunExpression()
    {
        Console.WriteLine();
        Console.WriteLine("Expression evaluator");
        Console.WriteLine("Type a maths expression using + - * / and ( ), for example:");
        Console.WriteLine("  2 * 4 - 3 / 5");
        Console.WriteLine("Use '.' as the decimal point. Leave it blank to go back.");

        while (true)
        {
            string input = InputReader.ReadLine("Expression: ");
            if (input.Trim().Length == 0)
                return;

            try
            {
                double result = ExpressionEvaluator.Evaluate(input);
                Console.WriteLine($"= {result}");
                return;
            }
            catch (Exception ex) when (ex is FormatException or DivideByZeroException)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Please try again, or leave it blank to go back.");
            }
        }
    }

    private static void RunUnitConverter()
    {
        Console.WriteLine();
        Console.WriteLine("Unit converter");
        Console.WriteLine("1. Kilometres to miles");
        Console.WriteLine("2. Miles to kilometres");
        Console.WriteLine("3. Celsius to Fahrenheit");
        Console.WriteLine("4. Fahrenheit to Celsius");
        Console.WriteLine("5. Back");

        int choice = InputReader.ReadMenuChoice("Choose a conversion: ", 1, 5);
        if (choice == 5)
            return;

        switch (choice)
        {
            case 1:
                double km = InputReader.ReadDouble("Enter kilometres: ");
                Console.WriteLine($"{km} km = {DistanceConverter.KilometresToMiles(km):0.####} miles");
                break;

            case 2:
                double miles = InputReader.ReadDouble("Enter miles: ");
                Console.WriteLine($"{miles} miles = {DistanceConverter.MilesToKilometres(miles):0.####} km");
                break;

            case 3:
                double celsius = InputReader.ReadDouble("Enter degrees Celsius: ");
                Console.WriteLine($"{celsius} C = {TemperatureConverter.CelsiusToFahrenheit(celsius):0.####} F");
                break;

            case 4:
                double fahrenheit = InputReader.ReadDouble("Enter degrees Fahrenheit: ");
                Console.WriteLine($"{fahrenheit} F = {TemperatureConverter.FahrenheitToCelsius(fahrenheit):0.####} C");
                break;
        }
    }

    private static void RunCurrencyConverter()
    {
        Console.WriteLine();
        Console.WriteLine("Currency converter (GHS to USD)");

        decimal amount = InputReader.ReadPositiveDecimal("Enter amount in GHS: ");
        decimal usdPerGhs = ChooseExchangeRate();

        decimal usd = CurrencyConverter.GhsToUsd(amount, usdPerGhs);
        Console.WriteLine($"{amount:0.00} GHS = {usd:0.00} USD  (rate: 1 GHS = {usdPerGhs} USD)");
    }

    private static decimal ChooseExchangeRate()
    {
        Console.WriteLine("Exchange rate source:");
        Console.WriteLine("1. Fetch the live rate online");
        Console.WriteLine($"2. Use the built-in rate ({CurrencyConverter.ReferenceUsdPerGhs} USD, as of {CurrencyConverter.ReferenceRateAsOf})");
        Console.WriteLine("3. Enter my own rate");

        return InputReader.ReadMenuChoice("Choose a rate source: ", 1, 3) switch
        {
            1 => TryFetchLiveRate() ?? UseBuiltInRate(),
            2 => CurrencyConverter.ReferenceUsdPerGhs,
            _ => InputReader.ReadPositiveDecimal("Enter exchange rate (USD for 1 GHS): "),
        };
    }

    private static decimal? TryFetchLiveRate()
    {
        Console.WriteLine("Fetching the live rate...");
        try
        {
            decimal rate = new LiveFxRateService().GetUsdPerGhs();
            Console.WriteLine($"Live rate: 1 GHS = {rate} USD");
            return rate;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not fetch the live rate ({ex.Message}).");
            return null;
        }
    }

    private static decimal UseBuiltInRate()
    {
        Console.WriteLine($"Using the built-in rate instead ({CurrencyConverter.ReferenceUsdPerGhs} USD).");
        return CurrencyConverter.ReferenceUsdPerGhs;
    }
}

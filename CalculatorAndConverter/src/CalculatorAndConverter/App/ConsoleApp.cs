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
        Console.WriteLine("5. Back");

        int choice = InputReader.ReadMenuChoice("Choose an operation: ", 1, 5);
        if (choice == 5)
            return;

        double a = InputReader.ReadDouble("Enter the first number: ");
        double b = InputReader.ReadDouble("Enter the second number: ");

        try
        {
            double result = choice switch
            {
                1 => Calculator.Add(a, b),
                2 => Calculator.Subtract(a, b),
                3 => Calculator.Multiply(a, b),
                _ => Calculator.Divide(a, b),
            };

            Console.WriteLine($"Result: {result}");
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine(ex.Message);
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
        decimal usdPerGhs = InputReader.ReadPositiveDecimal("Enter exchange rate (USD for 1 GHS): ");

        decimal usd = CurrencyConverter.GhsToUsd(amount, usdPerGhs);
        Console.WriteLine($"{amount:0.00} GHS = {usd:0.00} USD");
    }
}

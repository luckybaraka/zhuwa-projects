namespace CalculatorAndConverter.Core;

public static class Calculator
{
    public static double Add(double a, double b) => a + b;

    public static double Subtract(double a, double b) => a - b;

    public static double Multiply(double a, double b) => a * b;

    public static double Divide(double a, double b)
    {
        if (b == 0)
            throw new DivideByZeroException("Cannot divide by zero.");

        return a / b;
    }

    // Chained operations applied left-to-right across any number of operands,
    // e.g. Add([2, 3, 4]) == 9 and Subtract([100, 10, 5]) == 85.
    public static double Add(IReadOnlyList<double> values) => Aggregate(values, Add);

    public static double Subtract(IReadOnlyList<double> values) => Aggregate(values, Subtract);

    public static double Multiply(IReadOnlyList<double> values) => Aggregate(values, Multiply);

    public static double Divide(IReadOnlyList<double> values) => Aggregate(values, Divide);

    private static double Aggregate(IReadOnlyList<double> values, Func<double, double, double> operation)
    {
        if (values is null || values.Count == 0)
            throw new ArgumentException("At least one number is required.", nameof(values));

        double result = values[0];
        for (int i = 1; i < values.Count; i++)
            result = operation(result, values[i]);

        return result;
    }
}

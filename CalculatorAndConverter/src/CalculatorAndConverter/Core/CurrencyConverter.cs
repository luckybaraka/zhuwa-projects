namespace CalculatorAndConverter.Core;

public static class CurrencyConverter
{
    public const decimal ReferenceUsdPerGhs = 0.0859m;
    public const string ReferenceRateAsOf = "6 November 2026";

    public static decimal GhsToUsd(decimal amountGhs, decimal usdPerGhs)
    {
        if (amountGhs < 0)
            throw new ArgumentOutOfRangeException(nameof(amountGhs), "Amount cannot be negative.");

        if (usdPerGhs <= 0)
            throw new ArgumentOutOfRangeException(nameof(usdPerGhs), "Exchange rate must be greater than zero.");

        return amountGhs * usdPerGhs;
    }
}

namespace CalculatorAndConverter.Core;

public static class CurrencyConverter
{
    public static decimal GhsToUsd(decimal amountGhs, decimal usdPerGhs)
    {
        if (amountGhs < 0)
            throw new ArgumentOutOfRangeException(nameof(amountGhs), "Amount cannot be negative.");

        if (usdPerGhs <= 0)
            throw new ArgumentOutOfRangeException(nameof(usdPerGhs), "Exchange rate must be greater than zero.");

        return amountGhs * usdPerGhs;
    }
}

using System.Text.Json;

namespace CalculatorAndConverter.Core;

public static class ExchangeRateParser
{
    public static decimal ParseRate(string json, string currencyCode)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("rates", out JsonElement rates) ||
            !rates.TryGetProperty(currencyCode, out JsonElement rate))
        {
            throw new FormatException($"Rate for {currencyCode} was not found in the response.");
        }

        return rate.GetDecimal();
    }
}

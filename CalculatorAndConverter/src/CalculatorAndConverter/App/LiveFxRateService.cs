using CalculatorAndConverter.Core;

namespace CalculatorAndConverter.App;

public sealed class LiveFxRateService
{
    private const string Endpoint = "https://open.er-api.com/v6/latest/GHS";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public decimal GetUsdPerGhs()
    {
        string json = Http.GetStringAsync(Endpoint).GetAwaiter().GetResult();
        return ExchangeRateParser.ParseRate(json, "USD");
    }
}

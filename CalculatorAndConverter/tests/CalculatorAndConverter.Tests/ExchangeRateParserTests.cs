using CalculatorAndConverter.Core;
using Xunit;

namespace CalculatorAndConverter.Tests;

public class ExchangeRateParserTests
{
    [Fact]
    public void ParseRate_ReadsRequestedCurrency()
    {
        string json = """{ "result": "success", "base_code": "GHS", "rates": { "USD": 0.0859, "EUR": 0.08 } }""";
        Assert.Equal(0.0859m, ExchangeRateParser.ParseRate(json, "USD"));
    }

    [Fact]
    public void ParseRate_MissingCurrency_Throws()
    {
        string json = """{ "rates": { "EUR": 0.08 } }""";
        Assert.Throws<FormatException>(() => ExchangeRateParser.ParseRate(json, "USD"));
    }

    [Fact]
    public void ParseRate_MissingRatesObject_Throws()
    {
        string json = """{ "result": "error" }""";
        Assert.Throws<FormatException>(() => ExchangeRateParser.ParseRate(json, "USD"));
    }

    [Fact]
    public void ParseRate_InvalidJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => ExchangeRateParser.ParseRate("not json", "USD"));
    }
}

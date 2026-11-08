using CalculatorAndConverter.Core;
using Xunit;

namespace CalculatorAndConverter.Tests;

public class CurrencyConverterTests
{
    [Theory]
    [InlineData(100, 0.065, 6.5)]
    [InlineData(0, 0.065, 0)]
    [InlineData(50, 0.1, 5)]
    public void GhsToUsd_ConvertsCorrectly(decimal amount, decimal rate, decimal expected)
    {
        Assert.Equal(expected, CurrencyConverter.GhsToUsd(amount, rate));
    }

    [Fact]
    public void GhsToUsd_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CurrencyConverter.GhsToUsd(-1, 0.065m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.5)]
    public void GhsToUsd_NonPositiveRate_Throws(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CurrencyConverter.GhsToUsd(100, rate));
    }
}

using CalculatorAndConverter.Core;
using Xunit;

namespace CalculatorAndConverter.Tests;

public class TemperatureConverterTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    [InlineData(37, 98.6)]
    public void CelsiusToFahrenheit_ConvertsCorrectly(double celsius, double expected)
    {
        Assert.Equal(expected, TemperatureConverter.CelsiusToFahrenheit(celsius), 6);
    }

    [Theory]
    [InlineData(32, 0)]
    [InlineData(212, 100)]
    [InlineData(-40, -40)]
    public void FahrenheitToCelsius_ConvertsCorrectly(double fahrenheit, double expected)
    {
        Assert.Equal(expected, TemperatureConverter.FahrenheitToCelsius(fahrenheit), 6);
    }
}

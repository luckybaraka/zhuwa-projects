using CalculatorAndConverter.Core;
using Xunit;

namespace CalculatorAndConverter.Tests;

public class DistanceConverterTests
{
    [Fact]
    public void KilometresToMiles_ConvertsCorrectly()
    {
        Assert.Equal(6.21371, DistanceConverter.KilometresToMiles(10), 5);
    }

    [Fact]
    public void MilesToKilometres_ConvertsCorrectly()
    {
        Assert.Equal(16.0934, DistanceConverter.MilesToKilometres(10), 3);
    }

    [Fact]
    public void RoundTrip_ReturnsOriginalValue()
    {
        double kilometres = 42.195;
        double result = DistanceConverter.MilesToKilometres(DistanceConverter.KilometresToMiles(kilometres));
        Assert.Equal(kilometres, result, 6);
    }

    [Fact]
    public void Zero_ConvertsToZero()
    {
        Assert.Equal(0, DistanceConverter.KilometresToMiles(0));
    }
}

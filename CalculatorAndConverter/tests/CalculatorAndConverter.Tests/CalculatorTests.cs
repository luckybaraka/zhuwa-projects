using CalculatorAndConverter.Core;
using Xunit;

namespace CalculatorAndConverter.Tests;

public class CalculatorTests
{
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(-4, 4, 0)]
    [InlineData(-5, -6, -11)]
    public void Add_ReturnsSum(double a, double b, double expected)
    {
        Assert.Equal(expected, Calculator.Add(a, b));
    }

    [Theory]
    [InlineData(10, 4, 6)]
    [InlineData(0, 7, -7)]
    public void Subtract_ReturnsDifference(double a, double b, double expected)
    {
        Assert.Equal(expected, Calculator.Subtract(a, b));
    }

    [Theory]
    [InlineData(3, 4, 12)]
    [InlineData(-2, 5, -10)]
    [InlineData(6, 0, 0)]
    public void Multiply_ReturnsProduct(double a, double b, double expected)
    {
        Assert.Equal(expected, Calculator.Multiply(a, b));
    }

    [Theory]
    [InlineData(10, 2, 5)]
    [InlineData(-9, 3, -3)]
    public void Divide_ReturnsQuotient(double a, double b, double expected)
    {
        Assert.Equal(expected, Calculator.Divide(a, b));
    }

    [Fact]
    public void Divide_ByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Calculator.Divide(1, 0));
    }

    [Fact]
    public void Add_WithManyNumbers_ReturnsRunningSum()
    {
        Assert.Equal(14, Calculator.Add(new double[] { 2, 3, 4, 5 }));
    }

    [Fact]
    public void Subtract_WithManyNumbers_SubtractsLeftToRight()
    {
        Assert.Equal(85, Calculator.Subtract(new double[] { 100, 10, 5 }));
    }

    [Fact]
    public void Multiply_WithManyNumbers_ReturnsRunningProduct()
    {
        Assert.Equal(24, Calculator.Multiply(new double[] { 2, 3, 4 }));
    }

    [Fact]
    public void Divide_WithManyNumbers_DividesLeftToRight()
    {
        Assert.Equal(5, Calculator.Divide(new double[] { 100, 4, 5 }));
    }

    [Fact]
    public void Add_WithSingleNumber_ReturnsThatNumber()
    {
        Assert.Equal(7, Calculator.Add(new double[] { 7 }));
    }

    [Fact]
    public void Divide_ByZeroAmongManyNumbers_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Calculator.Divide(new double[] { 10, 0, 2 }));
    }

    [Fact]
    public void Add_WithNoNumbers_Throws()
    {
        Assert.Throws<ArgumentException>(() => Calculator.Add(Array.Empty<double>()));
    }
}

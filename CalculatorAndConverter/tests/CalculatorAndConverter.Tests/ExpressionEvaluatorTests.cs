using CalculatorAndConverter.Core;
using Xunit;

namespace CalculatorAndConverter.Tests;

public class ExpressionEvaluatorTests
{
    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("2 * 4 - 3 / 5", 7.4)]       // (2*4) - (3/5) = 8 - 0.6
    [InlineData("2 + 3 * 4", 14)]            // precedence: * before +
    [InlineData("10 / 2 / 5", 1)]            // left-to-right
    [InlineData("1.5 + 2.5", 4)]             // decimals
    [InlineData("  2  +  2  ", 4)]           // whitespace is ignored
    [InlineData("-5 + 2", -3)]               // leading unary minus
    [InlineData("3 - -2", 5)]                // unary minus after an operator
    [InlineData("2 * -3", -6)]
    public void Evaluate_RespectsPrecedenceAndSyntax(string expression, double expected)
    {
        Assert.Equal(expected, ExpressionEvaluator.Evaluate(expression), precision: 10);
    }

    [Theory]
    [InlineData("(2 + 3) * 4", 20)]
    [InlineData("2 * (3 + 4)", 14)]
    [InlineData("((1 + 2) * (3 + 4))", 21)]
    [InlineData("-(2 + 3)", -5)]
    public void Evaluate_HandlesParentheses(string expression, double expected)
    {
        Assert.Equal(expected, ExpressionEvaluator.Evaluate(expression), precision: 10);
    }

    [Fact]
    public void Evaluate_DivideByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => ExpressionEvaluator.Evaluate("1 / 0"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2 +")]              // trailing operator
    [InlineData("2 + + 3 4")]       // dangling number
    [InlineData("(2 + 3")]          // unbalanced parenthesis
    [InlineData("2 $ 3")]           // unknown character
    [InlineData("abc")]
    public void Evaluate_InvalidExpression_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => ExpressionEvaluator.Evaluate(expression));
    }
}

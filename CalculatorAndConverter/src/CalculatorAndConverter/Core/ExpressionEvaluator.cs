using System.Globalization;

namespace CalculatorAndConverter.Core;

// Evaluates a full arithmetic expression such as "2 * 4 - 3 / 5" with the usual
// operator precedence (* and / before + and -), parentheses, unary +/-, and
// decimal numbers. Numbers use '.' as the decimal point (invariant culture) so
// the syntax is unambiguous regardless of machine settings.
public static class ExpressionEvaluator
{
    public static double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new FormatException("The expression is empty.");

        var parser = new Parser(expression);
        double result = parser.ParseExpression();
        parser.ExpectEnd();
        return result;
    }

    // Recursive-descent parser:
    //   expression := term (('+' | '-') term)*
    //   term       := factor (('*' | '/') factor)*
    //   factor     := number | '(' expression ')' | ('+' | '-') factor
    private sealed class Parser
    {
        private readonly string _text;
        private int _pos;

        public Parser(string text) => _text = text;

        public double ParseExpression()
        {
            double value = ParseTerm();
            while (true)
            {
                SkipWhitespace();
                char op = Peek();
                if (op == '+') { _pos++; value += ParseTerm(); }
                else if (op == '-') { _pos++; value -= ParseTerm(); }
                else break;
            }

            return value;
        }

        private double ParseTerm()
        {
            double value = ParseFactor();
            while (true)
            {
                SkipWhitespace();
                char op = Peek();
                if (op == '*')
                {
                    _pos++;
                    value *= ParseFactor();
                }
                else if (op == '/')
                {
                    _pos++;
                    double divisor = ParseFactor();
                    if (divisor == 0)
                        throw new DivideByZeroException("Cannot divide by zero.");

                    value /= divisor;
                }
                else
                {
                    break;
                }
            }

            return value;
        }

        private double ParseFactor()
        {
            SkipWhitespace();
            char c = Peek();

            if (c == '+') { _pos++; return ParseFactor(); }
            if (c == '-') { _pos++; return -ParseFactor(); }

            if (c == '(')
            {
                _pos++;
                double value = ParseExpression();
                SkipWhitespace();
                if (Peek() != ')')
                    throw new FormatException("Missing a closing parenthesis ')'.");

                _pos++;
                return value;
            }

            return ParseNumber();
        }

        private double ParseNumber()
        {
            SkipWhitespace();
            int start = _pos;
            while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
                _pos++;

            if (_pos == start)
            {
                if (_pos < _text.Length)
                    throw new FormatException($"Unexpected character '{_text[_pos]}' in the expression.");

                throw new FormatException("Expected a number but the expression ended early.");
            }

            string token = _text[start.._pos];
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                throw new FormatException($"'{token}' is not a valid number.");

            return value;
        }

        public void ExpectEnd()
        {
            SkipWhitespace();
            if (_pos < _text.Length)
                throw new FormatException($"Unexpected character '{_text[_pos]}' in the expression.");
        }

        private void SkipWhitespace()
        {
            while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
                _pos++;
        }

        private char Peek() => _pos < _text.Length ? _text[_pos] : '\0';
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;

namespace CalculatorInAir
{
    public static class MathParser
    {
        public enum TokenType
        {
            Number,
            Plus,
            Minus,
            Multiply,
            Divide,
            Modulo,
            Power,
            LParen,
            RParen
        }

        public class Token
        {
            public TokenType Type { get; }
            public string Value { get; }

            public Token(TokenType type, string value = "")
            {
                Type = type;
                Value = value;
            }
        }

        public static double LastResult { get; set; } = 0.0;

        public static double Evaluate(string expression)
        {
            string trimmed = expression.Trim();
            if (string.IsNullOrEmpty(trimmed))
                throw new ArgumentException("Empty expression");

            var tokens = Tokenize(trimmed);
            if (tokens.Count == 0)
                throw new ArgumentException("Empty expression");

            int index = 0;
            double result = ParseExpression(tokens, ref index);

            if (index < tokens.Count)
            {
                throw new ArgumentException($"Unexpected token at the end: '{tokens[index].Value}'");
            }

            LastResult = result;
            return result;
        }

        public static string FormatResult(double val, int decimalPlaces)
        {
            if (double.IsNaN(val)) return "NaN";
            if (double.IsInfinity(val)) return double.IsPositiveInfinity(val) ? "∞" : "-∞";

            if (decimalPlaces < 0)
            {
                // Round to 12 decimal places to clean up double precision issues,
                // then convert using invariant culture up to 12 decimals, stripping trailing zeros.
                double rounded = Math.Round(val, 12);
                return rounded.ToString("0.############", CultureInfo.InvariantCulture);
            }
            else
            {
                double rounded = Math.Round(val, decimalPlaces);
                string format = decimalPlaces == 0 ? "0" : "0." + new string('0', decimalPlaces);
                return rounded.ToString(format, CultureInfo.InvariantCulture);
            }
        }

        private static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                bool ShouldImplicitMultiply(TokenType nextType)
                {
                    if (tokens.Count == 0) return false;
                    var lastType = tokens[tokens.Count - 1].Type;

                    // Two adjacent numbers (e.g. "2 3" or "1.2.3") is invalid syntax, not implicit multiplication
                    if (lastType == TokenType.Number && nextType == TokenType.Number)
                    {
                        return false;
                    }

                    bool lastIsPrimary = lastType == TokenType.Number || lastType == TokenType.RParen;
                    bool nextIsPrimary = nextType == TokenType.Number || nextType == TokenType.LParen;
                    return lastIsPrimary && nextIsPrimary;
                }

                if (char.IsDigit(c) || c == '.')
                {
                    int start = i;
                    bool hasDecimal = c == '.';
                    i++;
                    while (i < input.Length && (char.IsDigit(input[i]) || (!hasDecimal && input[i] == '.')))
                    {
                        if (input[i] == '.') hasDecimal = true;
                        i++;
                    }

                    if (i < input.Length && (input[i] == 'e' || input[i] == 'E'))
                    {
                        int lookahead = i + 1;
                        if (lookahead < input.Length && (input[lookahead] == '+' || input[lookahead] == '-'))
                        {
                            lookahead++;
                        }
                        if (lookahead < input.Length && char.IsDigit(input[lookahead]))
                        {
                            i = lookahead;
                            while (i < input.Length && char.IsDigit(input[i]))
                            {
                                i++;
                            }
                        }
                    }

                    string numStr = input.Substring(start, i - start);
                    if (ShouldImplicitMultiply(TokenType.Number))
                    {
                        tokens.Add(new Token(TokenType.Multiply, "*"));
                    }
                    tokens.Add(new Token(TokenType.Number, numStr));
                }
                else
                {
                    TokenType? type = null;
                    string val = c.ToString();

                    switch (c)
                    {
                        case '+': type = TokenType.Plus; break;
                        case '-': type = TokenType.Minus; break;
                        case '*': type = TokenType.Multiply; break;
                        case '/': type = TokenType.Divide; break;
                        case '%': type = TokenType.Modulo; break;
                        case '^': type = TokenType.Power; break;
                        case '(': type = TokenType.LParen; break;
                        case ')': type = TokenType.RParen; break;
                    }

                    if (type.HasValue)
                    {
                        if (type.Value == TokenType.LParen && ShouldImplicitMultiply(TokenType.LParen))
                        {
                            tokens.Add(new Token(TokenType.Multiply, "*"));
                        }
                        tokens.Add(new Token(type.Value, val));
                        i++;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid character: '{c}'");
                    }
                }
            }

            return tokens;
        }

        private static double ParseExpression(List<Token> tokens, ref int index)
        {
            double result = ParseTerm(tokens, ref index);
            while (index < tokens.Count && (tokens[index].Type == TokenType.Plus || tokens[index].Type == TokenType.Minus))
            {
                var op = tokens[index].Type;
                index++;
                double right = ParseTerm(tokens, ref index);
                if (op == TokenType.Plus) result += right;
                else result -= right;
            }
            return result;
        }

        private static double ParseTerm(List<Token> tokens, ref int index)
        {
            double result = ParseUnary(tokens, ref index);
            while (index < tokens.Count && (tokens[index].Type == TokenType.Multiply || tokens[index].Type == TokenType.Divide || tokens[index].Type == TokenType.Modulo))
            {
                var op = tokens[index].Type;
                index++;
                double right = ParseUnary(tokens, ref index);
                if (op == TokenType.Multiply)
                {
                    result *= right;
                }
                else if (op == TokenType.Divide)
                {
                    if (right == 0) throw new DivideByZeroException("Division by zero");
                    result /= right;
                }
                else // Modulo
                {
                    if (right == 0) throw new DivideByZeroException("Division by zero");
                    result %= right;
                }
            }
            return result;
        }

        private static double ParseFactor(List<Token> tokens, ref int index)
        {
            double result = ParsePrimary(tokens, ref index);
            if (index < tokens.Count && tokens[index].Type == TokenType.Power)
            {
                index++;
                double right = ParseUnary(tokens, ref index);
                result = Math.Pow(result, right);
            }
            return result;
        }

        private static double ParseUnary(List<Token> tokens, ref int index)
        {
            if (index < tokens.Count && (tokens[index].Type == TokenType.Plus || tokens[index].Type == TokenType.Minus))
            {
                var op = tokens[index].Type;
                index++;
                double val = ParseUnary(tokens, ref index);
                return op == TokenType.Plus ? val : -val;
            }
            return ParseFactor(tokens, ref index);
        }

        private static double ParsePrimary(List<Token> tokens, ref int index)
        {
            if (index >= tokens.Count)
                throw new ArgumentException("Unexpected end of expression");

            var token = tokens[index];
            if (token.Type == TokenType.Number)
            {
                index++;
                if (double.TryParse(token.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                {
                    return val;
                }
                throw new ArgumentException($"Invalid number literal: '{token.Value}'");
            }
            else if (token.Type == TokenType.LParen)
            {
                index++; // consume '('
                double val = ParseExpression(tokens, ref index);
                if (index >= tokens.Count || tokens[index].Type != TokenType.RParen)
                {
                    throw new ArgumentException("Mismatched parenthesis");
                }
                index++; // consume ')'
                return val;
            }
            else
            {
                throw new ArgumentException($"Unexpected token: '{token.Value}'");
            }
        }
    }
}

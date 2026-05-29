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
            Identifier,
            Plus,
            Minus,
            Multiply,
            Divide,
            Modulo,
            Power,
            LParen,
            RParen,
            Comma
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

        public static double Evaluate(string expression)
        {
            var tokens = Tokenize(expression);
            if (tokens.Count == 0)
                throw new ArgumentException("Empty expression");

            int index = 0;
            double result = ParseExpression(tokens, ref index);

            if (index < tokens.Count)
            {
                throw new ArgumentException($"Unexpected token at the end: '{tokens[index].Value}'");
            }

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

                // Inline helper to see if implicit multiplication is applicable
                bool ShouldImplicitMultiply(TokenType nextType)
                {
                    if (tokens.Count == 0) return false;
                    var lastType = tokens[tokens.Count - 1].Type;
                    bool lastIsPrimary = lastType == TokenType.Number || lastType == TokenType.Identifier || lastType == TokenType.RParen;
                    bool nextIsPrimary = nextType == TokenType.Number || nextType == TokenType.Identifier || nextType == TokenType.LParen;
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

                    // Check for scientific notation (e.g., 1e-5, 2.5e+3)
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
                else if (char.IsLetter(c) || c == 'π')
                {
                    int start = i;
                    i++;
                    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == 'π'))
                    {
                        i++;
                    }

                    string ident = input.Substring(start, i - start);
                    if (ShouldImplicitMultiply(TokenType.Identifier))
                    {
                        tokens.Add(new Token(TokenType.Multiply, "*"));
                    }
                    tokens.Add(new Token(TokenType.Identifier, ident));
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
                        case ',': type = TokenType.Comma; break;
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
            double result = ParseFactor(tokens, ref index);
            while (index < tokens.Count && (tokens[index].Type == TokenType.Multiply || tokens[index].Type == TokenType.Divide || tokens[index].Type == TokenType.Modulo))
            {
                var op = tokens[index].Type;
                index++;
                double right = ParseFactor(tokens, ref index);
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
            double result = ParseUnary(tokens, ref index);
            if (index < tokens.Count && tokens[index].Type == TokenType.Power)
            {
                index++;
                // Power is right-associative (e.g. 2^3^2 = 2^(3^2))
                double right = ParseFactor(tokens, ref index);
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
            return ParsePrimary(tokens, ref index);
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
            else if (token.Type == TokenType.Identifier)
            {
                string name = token.Value.ToLowerInvariant();
                index++;

                // Check if it is a function call (followed by '(')
                if (index < tokens.Count && tokens[index].Type == TokenType.LParen)
                {
                    index++; // consume '('
                    var args = new List<double>();
                    if (tokens[index].Type != TokenType.RParen)
                    {
                        args.Add(ParseExpression(tokens, ref index));
                        while (index < tokens.Count && tokens[index].Type == TokenType.Comma)
                        {
                            index++; // consume ','
                            args.Add(ParseExpression(tokens, ref index));
                        }
                    }

                    if (index >= tokens.Count || tokens[index].Type != TokenType.RParen)
                    {
                        throw new ArgumentException($"Mismatched parenthesis in function '{name}'");
                    }
                    index++; // consume ')'

                    return EvaluateFunction(name, args);
                }
                else
                {
                    // It's a constant
                    return EvaluateConstant(name);
                }
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

        private static double EvaluateConstant(string name)
        {
            switch (name)
            {
                case "pi":
                case "π":
                    return Math.PI;
                case "e":
                    return Math.E;
                case "tau":
                    return Math.PI * 2.0;
                default:
                    throw new ArgumentException($"Unknown constant: '{name}'");
            }
        }

        private static double EvaluateFunction(string name, List<double> args)
        {
            int count = args.Count;
            switch (name)
            {
                case "sin":
                    if (count != 1) throw new ArgumentException("sin expects 1 argument");
                    return Math.Sin(args[0]);
                case "cos":
                    if (count != 1) throw new ArgumentException("cos expects 1 argument");
                    return Math.Cos(args[0]);
                case "tan":
                    if (count != 1) throw new ArgumentException("tan expects 1 argument");
                    return Math.Tan(args[0]);
                case "asin":
                case "arcsin":
                    if (count != 1) throw new ArgumentException("asin expects 1 argument");
                    return Math.Asin(args[0]);
                case "acos":
                case "arccos":
                    if (count != 1) throw new ArgumentException("acos expects 1 argument");
                    return Math.Acos(args[0]);
                case "atan":
                case "arctan":
                    if (count != 1) throw new ArgumentException("atan expects 1 argument");
                    return Math.Atan(args[0]);
                case "sqrt":
                    if (count != 1) throw new ArgumentException("sqrt expects 1 argument");
                    if (args[0] < 0) throw new ArgumentException("sqrt of negative number is undefined");
                    return Math.Sqrt(args[0]);
                case "cbrt":
                    if (count != 1) throw new ArgumentException("cbrt expects 1 argument");
                    return Math.Cbrt(args[0]);
                case "log":
                    if (count == 1) return Math.Log10(args[0]);
                    if (count == 2) return Math.Log(args[0], args[1]);
                    throw new ArgumentException("log expects 1 or 2 arguments");
                case "ln":
                    if (count != 1) throw new ArgumentException("ln expects 1 argument");
                    return Math.Log(args[0]);
                case "abs":
                    if (count != 1) throw new ArgumentException("abs expects 1 argument");
                    return Math.Abs(args[0]);
                case "exp":
                    if (count != 1) throw new ArgumentException("exp expects 1 argument");
                    return Math.Exp(args[0]);
                case "round":
                    if (count == 1) return Math.Round(args[0]);
                    if (count == 2) return Math.Round(args[0], (int)args[1]);
                    throw new ArgumentException("round expects 1 or 2 arguments");
                case "floor":
                    if (count != 1) throw new ArgumentException("floor expects 1 argument");
                    return Math.Floor(args[0]);
                case "ceil":
                    if (count != 1) throw new ArgumentException("ceil expects 1 argument");
                    return Math.Ceiling(args[0]);
                default:
                    throw new ArgumentException($"Unknown function: '{name}'");
            }
        }
    }
}

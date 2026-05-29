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

        public static double LastResult { get; set; } = 0.0;

        public static double Evaluate(string expression)
        {
            string trimmed = expression.Trim();
            if (string.IsNullOrEmpty(trimmed))
                throw new ArgumentException("Empty expression");

            // Check for unit conversion split
            string[]? parts = SplitByConversion(trimmed);
            if (parts != null && parts.Length == 2)
            {
                string leftSide = parts[0].Trim();
                string targetUnit = parts[1].Trim();

                // Find the unit at the end of leftSide
                int i = leftSide.Length - 1;
                while (i >= 0 && char.IsWhiteSpace(leftSide[i])) i--;
                int unitEnd = i;
                while (i >= 0 && (char.IsLetter(leftSide[i]) || leftSide[i] == '°' || leftSide[i] == 'º' || leftSide[i] == '米' || leftSide[i] == '克' || leftSide[i] == '磅' || leftSide[i] == '码' || leftSide[i] == '里' || leftSide[i] == '度' || leftSide[i] == '文')) i--;
                int unitStart = i + 1;

                if (unitStart <= unitEnd)
                {
                    string sourceUnit = leftSide.Substring(unitStart, unitEnd - unitStart + 1);
                    string mathExpr = leftSide.Substring(0, unitStart).Trim();

                    // If mathExpr is empty, default to "1" (e.g. "m to cm" -> "1 m to cm")
                    if (string.IsNullOrEmpty(mathExpr))
                    {
                        mathExpr = "1";
                    }

                    double val = Evaluate(mathExpr);
                    if (Convert(val, sourceUnit, targetUnit, out double convertedVal))
                    {
                        LastResult = convertedVal;
                        return convertedVal;
                    }
                }
            }

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

        private static string[]? SplitByConversion(string input)
        {
            int index = input.IndexOf(" to ", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                return new string[] { input.Substring(0, index), input.Substring(index + 4) };
            }

            index = input.IndexOf(" in ", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                return new string[] { input.Substring(0, index), input.Substring(index + 4) };
            }

            return null;
        }

        private enum UnitCategory
        {
            Length,
            Weight,
            Temperature
        }

        private class UnitInfo
        {
            public UnitCategory Category { get; }
            public double Multiplier { get; }

            public UnitInfo(UnitCategory category, double multiplier)
            {
                Category = category;
                Multiplier = multiplier;
            }
        }

        private static readonly Dictionary<string, UnitInfo> UnitDict = new Dictionary<string, UnitInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // Length
            { "m", new UnitInfo(UnitCategory.Length, 1.0) },
            { "meter", new UnitInfo(UnitCategory.Length, 1.0) },
            { "meters", new UnitInfo(UnitCategory.Length, 1.0) },
            { "米", new UnitInfo(UnitCategory.Length, 1.0) },
            { "cm", new UnitInfo(UnitCategory.Length, 0.01) },
            { "centimeter", new UnitInfo(UnitCategory.Length, 0.01) },
            { "centimeters", new UnitInfo(UnitCategory.Length, 0.01) },
            { "厘米", new UnitInfo(UnitCategory.Length, 0.01) },
            { "mm", new UnitInfo(UnitCategory.Length, 0.001) },
            { "millimeter", new UnitInfo(UnitCategory.Length, 0.001) },
            { "millimeters", new UnitInfo(UnitCategory.Length, 0.001) },
            { "毫米", new UnitInfo(UnitCategory.Length, 0.001) },
            { "km", new UnitInfo(UnitCategory.Length, 1000.0) },
            { "kilometer", new UnitInfo(UnitCategory.Length, 1000.0) },
            { "kilometers", new UnitInfo(UnitCategory.Length, 1000.0) },
            { "千米", new UnitInfo(UnitCategory.Length, 1000.0) },
            { "公里", new UnitInfo(UnitCategory.Length, 1000.0) },
            { "in", new UnitInfo(UnitCategory.Length, 0.0254) },
            { "inch", new UnitInfo(UnitCategory.Length, 0.0254) },
            { "inches", new UnitInfo(UnitCategory.Length, 0.0254) },
            { "英寸", new UnitInfo(UnitCategory.Length, 0.0254) },
            { "ft", new UnitInfo(UnitCategory.Length, 0.3048) },
            { "foot", new UnitInfo(UnitCategory.Length, 0.3048) },
            { "feet", new UnitInfo(UnitCategory.Length, 0.3048) },
            { "英尺", new UnitInfo(UnitCategory.Length, 0.3048) },
            { "yd", new UnitInfo(UnitCategory.Length, 0.9144) },
            { "yard", new UnitInfo(UnitCategory.Length, 0.9144) },
            { "yards", new UnitInfo(UnitCategory.Length, 0.9144) },
            { "码", new UnitInfo(UnitCategory.Length, 0.9144) },
            { "mi", new UnitInfo(UnitCategory.Length, 1609.344) },
            { "mile", new UnitInfo(UnitCategory.Length, 1609.344) },
            { "miles", new UnitInfo(UnitCategory.Length, 1609.344) },
            { "英里", new UnitInfo(UnitCategory.Length, 1609.344) },

            // Weight/Mass
            { "kg", new UnitInfo(UnitCategory.Weight, 1.0) },
            { "kilogram", new UnitInfo(UnitCategory.Weight, 1.0) },
            { "kilograms", new UnitInfo(UnitCategory.Weight, 1.0) },
            { "千克", new UnitInfo(UnitCategory.Weight, 1.0) },
            { "公斤", new UnitInfo(UnitCategory.Weight, 1.0) },
            { "g", new UnitInfo(UnitCategory.Weight, 0.001) },
            { "gram", new UnitInfo(UnitCategory.Weight, 0.001) },
            { "grams", new UnitInfo(UnitCategory.Weight, 0.001) },
            { "克", new UnitInfo(UnitCategory.Weight, 0.001) },
            { "mg", new UnitInfo(UnitCategory.Weight, 0.000001) },
            { "milligram", new UnitInfo(UnitCategory.Weight, 0.000001) },
            { "milligrams", new UnitInfo(UnitCategory.Weight, 0.000001) },
            { "毫克", new UnitInfo(UnitCategory.Weight, 0.000001) },
            { "lb", new UnitInfo(UnitCategory.Weight, 0.45359237) },
            { "lbs", new UnitInfo(UnitCategory.Weight, 0.45359237) },
            { "pound", new UnitInfo(UnitCategory.Weight, 0.45359237) },
            { "pounds", new UnitInfo(UnitCategory.Weight, 0.45359237) },
            { "磅", new UnitInfo(UnitCategory.Weight, 0.45359237) },
            { "oz", new UnitInfo(UnitCategory.Weight, 0.028349523125) },
            { "ounce", new UnitInfo(UnitCategory.Weight, 0.028349523125) },
            { "ounces", new UnitInfo(UnitCategory.Weight, 0.028349523125) },
            { "盎司", new UnitInfo(UnitCategory.Weight, 0.028349523125) },

            // Temperature
            { "c", new UnitInfo(UnitCategory.Temperature, 0) },
            { "celsius", new UnitInfo(UnitCategory.Temperature, 0) },
            { "摄氏度", new UnitInfo(UnitCategory.Temperature, 0) },
            { "f", new UnitInfo(UnitCategory.Temperature, 0) },
            { "fahrenheit", new UnitInfo(UnitCategory.Temperature, 0) },
            { "华氏度", new UnitInfo(UnitCategory.Temperature, 0) },
            { "k", new UnitInfo(UnitCategory.Temperature, 0) },
            { "kelvin", new UnitInfo(UnitCategory.Temperature, 0) },
            { "开尔文", new UnitInfo(UnitCategory.Temperature, 0) }
        };

        private static bool Convert(double val, string from, string to, out double result)
        {
            result = 0;
            if (!UnitDict.TryGetValue(from, out var fromInfo) || !UnitDict.TryGetValue(to, out var toInfo))
            {
                return false;
            }
            if (fromInfo.Category != toInfo.Category)
            {
                return false;
            }

            if (fromInfo.Category == UnitCategory.Temperature)
            {
                double celsius = 0;
                string fLower = from.ToLowerInvariant();
                if (fLower == "c" || fLower == "celsius" || fLower == "摄氏度")
                {
                    celsius = val;
                }
                else if (fLower == "f" || fLower == "fahrenheit" || fLower == "华氏度")
                {
                    celsius = (val - 32) / 1.8;
                }
                else if (fLower == "k" || fLower == "kelvin" || fLower == "开尔文")
                {
                    celsius = val - 273.15;
                }

                string tLower = to.ToLowerInvariant();
                if (tLower == "c" || tLower == "celsius" || tLower == "摄氏度")
                {
                    result = celsius;
                }
                else if (tLower == "f" || tLower == "fahrenheit" || tLower == "华氏度")
                {
                    result = celsius * 1.8 + 32;
                }
                else if (tLower == "k" || tLower == "kelvin" || tLower == "开尔文")
                {
                    result = celsius + 273.15;
                }
                return true;
            }
            else
            {
                double baseValue = val * fromInfo.Multiplier;
                result = baseValue / toInfo.Multiplier;
                return true;
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

                if (index < tokens.Count && tokens[index].Type == TokenType.LParen)
                {
                    index++; // consume '('
                    var args = new List<double>();
                    if (index >= tokens.Count)
                    {
                        throw new ArgumentException($"Mismatched parenthesis in function '{name}'");
                    }
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
                case "ans":
                    return LastResult;
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

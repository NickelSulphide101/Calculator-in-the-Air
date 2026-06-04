using System;
using Xunit;
using CalculatorInAir;

namespace CalculatorInAir.Tests
{
    public class MathParserTests
    {
        [Theory]
        [InlineData("2 + 3 * 4", 14.0)]
        [InlineData("(2 + 3) * 4", 20.0)]
        [InlineData("10 - 5 - 2", 3.0)]
        [InlineData("2^3^2", 512.0)] // right-associative power: 2^(3^2) = 512
        [InlineData("10 % 3", 1.0)]
        [InlineData("-5 + +3", -2.0)]
        public void Evaluate_BasicArithmetic_ShouldCalculateCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData("2pi", 2 * Math.PI)]
        [InlineData("2(3+4)", 14.0)]
        [InlineData("(2+3)(4+1)", 25.0)]
        public void Evaluate_ImplicitMultiplication_ShouldCalculateCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData("pi", Math.PI)]
        [InlineData("π", Math.PI)]
        [InlineData("e", Math.E)]
        [InlineData("tau", Math.PI * 2.0)]
        public void Evaluate_Constants_ShouldResolveCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 5);
        }

        [Fact]
        public void Evaluate_AnsVariable_ShouldResolveLastCalculatedResult()
        {
            // Evaluate an expression to set LastResult
            double val1 = MathParser.Evaluate("10 + 5.5");
            Assert.Equal(15.5, val1);
            Assert.Equal(15.5, MathParser.LastResult);

            // Now evaluate using ans
            double val2 = MathParser.Evaluate("ans * 2");
            Assert.Equal(31.0, val2);
            Assert.Equal(31.0, MathParser.LastResult);
        }

        [Theory]
        [InlineData("sin(pi/2)", 1.0)]
        [InlineData("cos(0)", 1.0)]
        [InlineData("sqrt(3^2 + 4^2)", 5.0)]
        [InlineData("log(100)", 2.0)]
        [InlineData("ln(e)", 1.0)]
        [InlineData("abs(-10.5)", 10.5)]
        [InlineData("round(2.7)", 3.0)]
        [InlineData("round(2.718, 2)", 2.72)]
        [InlineData("floor(3.8)", 3.0)]
        [InlineData("ceil(3.2)", 4.0)]
        public void Evaluate_MathematicalFunctions_ShouldCalculateCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData("10 m to cm", 1000.0)]
        [InlineData("1 km to m", 1000.0)]
        [InlineData("1 inch to mm", 25.4)]
        [InlineData("6 feet to inch", 72.0)]
        [InlineData("1 mile in km", 1.609344)]
        [InlineData("1000 g to kg", 1.0)]
        [InlineData("1 lb to oz", 16.0)]
        [InlineData("0 C to F", 32.0)]
        [InlineData("100 C to F", 212.0)]
        [InlineData("0 C to K", 273.15)]
        [InlineData("32 F to C", 0.0)]
        [InlineData("212 F to C", 100.0)]
        [InlineData("0 °C to °F", 32.0)]
        [InlineData("100 °C to °F", 212.0)]
        [InlineData("0 ºC to ºF", 32.0)]
        [InlineData("32 °F to °C", 0.0)]
        public void Evaluate_UnitConversions_ShouldConvertCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 4);
        }

        [Theory]
        [InlineData("m to cm", 100.0)] // defaults to 1
        [InlineData("kg to g", 1000.0)]
        [InlineData("0 C to C", 0.0)]
        public void Evaluate_UnitConversionsDefaultOne_ShouldConvertCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 4);
        }

        [Fact]
        public void Evaluate_InvalidExpressions_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate(""));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("2 + "));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("2 + (3"));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("unknown_func(2)"));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("10 m to kg")); // mismatched categories
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("sin(")); // incomplete function call
        }

        [Theory]
        [InlineData("-2^2", -4.0)]
        [InlineData("2^-2", 0.25)]
        [InlineData("-2^-2", -0.25)]
        [InlineData("2^3^2", 512.0)]
        [InlineData("2^-3^2", 0.001953125)]
        public void Evaluate_PowerPrecedenceAndUnary_ShouldCalculateCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 9);
        }

        [Fact]
        public void Evaluate_MismatchedUnitConversions_ShouldThrowFriendlyException()
        {
            var ex = Assert.Throws<ArgumentException>(() => MathParser.Evaluate("10 m to kg"));
            Assert.Contains("Unsupported or mismatched unit conversion from 'm' to 'kg'", ex.Message);
        }

        [Theory]
        [InlineData("round(1.23, -1)")]
        [InlineData("round(1.23, 100)")]
        public void Evaluate_RoundDigitsValidation_ShouldThrowException(string expr)
        {
            var ex = Assert.Throws<ArgumentException>(() => MathParser.Evaluate(expr));
            Assert.Contains("Rounding decimals must be between 0 and 15", ex.Message);
        }
    }
}

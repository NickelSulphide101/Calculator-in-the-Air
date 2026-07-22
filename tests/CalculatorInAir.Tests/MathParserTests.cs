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
        [InlineData("2(3+4)", 14.0)]
        [InlineData("(2+3)(4+1)", 25.0)]
        [InlineData("(2+3)4", 20.0)]
        public void Evaluate_ImplicitMultiplication_ShouldCalculateCorrectly(string expr, double expected)
        {
            double result = MathParser.Evaluate(expr);
            Assert.Equal(expected, result, 5);
        }

        [Fact]
        public void Evaluate_LastResultProperty_ShouldStoreLastCalculatedResult()
        {
            double val1 = MathParser.Evaluate("10 + 5.5");
            Assert.Equal(15.5, val1);
            Assert.Equal(15.5, MathParser.LastResult);
        }

        [Theory]
        [InlineData("sin(30)")]
        [InlineData("cos(0)")]
        [InlineData("sqrt(16)")]
        [InlineData("pi")]
        [InlineData("10 m to cm")]
        [InlineData("abc")]
        public void Evaluate_LetterInputs_ShouldThrowArgumentException(string expr)
        {
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate(expr));
        }

        [Fact]
        public void Evaluate_InvalidExpressions_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate(""));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("2 + "));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("2 + (3"));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("2 3"));
            Assert.Throws<ArgumentException>(() => MathParser.Evaluate("1.2.3"));
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
    }
}

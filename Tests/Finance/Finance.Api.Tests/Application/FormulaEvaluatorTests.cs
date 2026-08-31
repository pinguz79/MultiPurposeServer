using Finance.Api.Application;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Application
{
    public class FormulaEvaluatorTests
    {
        [Theory]
        [InlineData("38,90", "38.90", 38.90)]
        [InlineData("3.480,50", "3480.50", 3480.50)]
        [InlineData("+38.9", "38.90", 38.90)]
        public async Task ConstantFormulaIsNormalizedAndEvaluated(string formula, string expectedFormula, decimal expectedValue)
        {
            // Arrange
            var evaluator = new FormulaEvaluator(Mock.Of<IFormulaResolver>());

            // Act
            FormulaValidationResult validation = await evaluator.Validate(formula);
            FormulaEvaluationResult evaluation = await evaluator.Evaluate(formula, new DateOnly(2026, 8, 28));

            // Assert
            validation.Formula.Should().Be(expectedFormula);
            evaluation.Value.Should().Be(expectedValue);
        }

        [Fact]
        public async Task RecurringEntryIsCanonicalizedAndRoundedOnlyAfterEvaluation()
        {
            // Arrange
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(item => item.ResolveCanonicalName("aFFitto")).ReturnsAsync("Affitto");
            resolver.Setup(item => item.ResolveCanonicalName("Affitto")).ReturnsAsync("Affitto");
            resolver.Setup(item => item.Resolve(It.Is<IReadOnlyList<string>>(dependencies => dependencies.SequenceEqual(new[] { "Affitto" })), It.IsAny<DateOnly>()))
                .ReturnsAsync([new ResolvedFormulaParameter("Affitto", 10.005m, false)]);
            var evaluator = new FormulaEvaluator(resolver.Object);

            // Act
            FormulaValidationResult validation = await evaluator.Validate("[aFFitto]");
            FormulaEvaluationResult evaluation = await evaluator.Evaluate(validation.Formula, new DateOnly(2026, 8, 28));

            // Assert
            validation.Formula.Should().Be("[Affitto]");
            evaluation.Value.Should().Be(10.01m);
        }

        [Fact]
        public async Task UncoveredIntervalReturnsZeroAndWarning()
        {
            // Arrange
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(item => item.ResolveCanonicalName("Affitto")).ReturnsAsync("Affitto");
            resolver.Setup(item => item.Resolve(It.Is<IReadOnlyList<string>>(dependencies => dependencies.SequenceEqual(new[] { "Affitto" })), It.IsAny<DateOnly>()))
                .ReturnsAsync([new ResolvedFormulaParameter("Affitto", 0m, true)]);
            var evaluator = new FormulaEvaluator(resolver.Object);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[Affitto]", new DateOnly(2026, 8, 28));

            // Assert
            result.Value.Should().Be(0m);
            result.HasUncoveredInterval.Should().BeTrue();
        }

        [Theory]
        [InlineData("Min(10.00, 4.00)", 4.00)]
        [InlineData("Max(10.00, 4.00)", 10.00)]
        public async Task SupportedFunctionIsEvaluated(string formula, decimal expected)
        {
            // Arrange
            var evaluator = new FormulaEvaluator(Mock.Of<IFormulaResolver>());

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate(formula, new DateOnly(2026, 8, 28));

            // Assert
            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("[conto.saldo]")]
        [InlineData("Sin(1)")]
        public async Task UnsupportedConstructIsRejected(string formula)
        {
            // Arrange
            var evaluator = new FormulaEvaluator(Mock.Of<IFormulaResolver>());

            // Act
            FormulaValidationResult result = await evaluator.Validate(formula);

            // Assert
            result.IsValid.Should().BeFalse();
        }
    }
}

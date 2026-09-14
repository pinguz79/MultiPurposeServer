using System.Globalization;

using Finance.Api.Application;
using Finance.DataModel.Models;

using FluentAssertions;

namespace Finance.Api.Tests.Infrastructure
{
    public class RevolvingFormulaIntegrationTests
    {
        [Fact]
        public async Task RunReadOnlyBatch_WhenParameterWasReplaced_ResolvesNewDefinitionAndDiscardsResults()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            var date = new DateOnly(2026, 9, 6);
            await scenario.Evaluator.Evaluate("[AmEx.QuotaRata]", date);
            await scenario.Parameters.Delete(scenario.Card.Id, "QuotaRata");
            await scenario.SetParameter("QuotaRata", 0.2m, TipoParametroConto.Percentuale);
            FormulaEvaluationResult? result = null;

            // Act
            await scenario.Cache.RunReadOnlyBatch(async () => result = await scenario.Evaluator.Evaluate("[AmEx.QuotaRata]", date));

            // Assert
            result!.Error.Should().BeNull();
            result.Value.Should().Be(0.2m);
            scenario.Cache.CanReuseAcrossEvaluations.Should().BeFalse();
            scenario.Cache.TryGetResult("[AmEx.QuotaRata]", date, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData("[AmEx.InteressiCiclo]", 8, 16.19)]
        [InlineData("[AmEx.InteressiCiclo]", 9, 15.96)]
        [InlineData("[AmEx.RataUltimoCicloChiuso]", 8, 220.16)]
        [InlineData("[AmEx.RataUltimoCicloChiuso]", 9, 160.66)]
        public async Task Evaluate_WhenReplayingRealAmexHistory_MatchesStatement(string formula, int month, decimal expected)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(1591.59m);
            await scenario.Add(new DateOnly(2025, 7, 6), "15.18", NaturaMovimento.Interessi);
            await scenario.Add(new DateOnly(2025, 7, 6), "2.00", NaturaMovimento.Bollo);
            await scenario.Add(new DateOnly(2025, 7, 21), "-169.65", NaturaMovimento.Rimborso);
            var purchases = new Dictionary<DateOnly, decimal>
            {
                [new(2025, 7, 14)] = 3.83m,
                [new(2025, 7, 17)] = 3.98m,
                [new(2025, 7, 18)] = 0.60m,
                [new(2025, 7, 21)] = 10.27m,
                [new(2025, 7, 23)] = 62.06m,
                [new(2025, 7, 24)] = 13.14m,
                [new(2025, 7, 26)] = 64.85m,
                [new(2025, 7, 30)] = 10.26m,
                [new(2025, 8, 1)] = 25.69m,
                [new(2025, 8, 2)] = 2.70m,
                [new(2025, 8, 21)] = 60.01m,
                [new(2025, 8, 22)] = 5m,
                [new(2025, 8, 24)] = 3.96m,
                [new(2025, 8, 27)] = 18m,
                [new(2025, 8, 28)] = 11.40m,
                [new(2025, 8, 30)] = 3.60m,
                [new(2025, 8, 31)] = 7.99m,
                [new(2025, 9, 3)] = 3.39m,
                [new(2025, 9, 4)] = 24.49m,
                [new(2025, 9, 5)] = 10.27m,
            };
            foreach ((DateOnly date, decimal amount) in purchases)
            {
                await scenario.Add(date, amount.ToString("0.00", CultureInfo.InvariantCulture));
            }
            await scenario.AddCycle(new DateOnly(2025, 8, 6));
            await scenario.AddCycle(new DateOnly(2025, 9, 6));

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate(formula, new DateOnly(2025, month, 19));

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("[AmEx.InteressiCiclo]", 10.19)]
        [InlineData("[AmEx.BolloCiclo]", 2.00)]
        [InlineData("[AmEx.SaldoUltimoCicloChiuso]", 1012.19)]
        [InlineData("-[AmEx.RataUltimoCicloChiuso]", -101.22)]
        public async Task Evaluate_WhenClosingChargesReferenceAccount_ComputesWithoutCircularity(string formula, decimal expected)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            await scenario.AddCycle(new DateOnly(2025, 9, 6));

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate(formula, new DateOnly(2025, 9, 19));

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Evaluate_WhenPreviousCycleRepaid_ExcludesPaidChargesFromCapital(bool consolidated)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            await scenario.AddCycle(new DateOnly(2025, 9, 6));
            await scenario.AddCycle(new DateOnly(2025, 10, 6));
            if (consolidated)
            {
                foreach (Movimento movement in await scenario.Movements.GetBefore(new DateOnly(2025, 10, 1)))
                {
                    FormulaEvaluationResult value = await scenario.Evaluator.Evaluate(movement.Formula, movement.Date);
                    await scenario.Movements.Consolidate(movement.Id, value.Value!.Value.ToString("0.00", CultureInfo.InvariantCulture));
                }
            }

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate("[AmEx.RataUltimoCicloChiuso]", new DateOnly(2025, 10, 19));

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(92.23m);
        }

        [Fact]
        public async Task Evaluate_WhenInterestIsManuallyCorrected_UsesCorrectionForPayment()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            DateOnly closing = new(2025, 9, 6);
            Movimento interest = await scenario.Add(closing, "[AmEx.InteressiCiclo]", NaturaMovimento.Interessi);
            await scenario.Add(closing, "[AmEx.BolloCiclo]", NaturaMovimento.Bollo);
            await scenario.Evaluator.Evaluate("[AmEx.RataUltimoCicloChiuso]", closing);
            await scenario.Movements.Update(interest.Id, null, null, "15.00", null, false);

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate("[AmEx.RataUltimoCicloChiuso]", closing);

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(101.70m);
        }

        [Fact]
        public async Task Evaluate_WhenOrdinaryMovementDependsOnOwnInterest_ReportsCircularReference()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            Movimento invalid = await scenario.Add(new DateOnly(2025, 9, 6), "[AmEx.InteressiCiclo]");

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate("[AmEx.InteressiCiclo]", invalid.Date);

            // Assert
            result.Value.Should().BeNull();
            result.Error.Should().Contain(invalid.Id.ToString());
        }

        [Fact]
        public async Task Evaluate_WhenYearChanges_UsesDailyCalendarYearAndTemporalRate()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            await scenario.Parameters.Replace(scenario.Card.Id, "Tan", "Tan", TipoParametroConto.Percentuale, [
                new ParametroConto { Index = 0, DisplayName = "Dal nuovo anno", Value = 0.24m, ValidFrom = new DateOnly(2024, 1, 1) },
                new ParametroConto { Index = 1, DisplayName = "Ordinario", Value = 0.12m },
            ]);

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate("[AmEx.InteressiCiclo]", new DateOnly(2024, 1, 6));

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(12.15m);
        }

        [Fact]
        public async Task Evaluate_WhenTenYearsOfCyclesExist_ComputesFinalPaymentWithoutCircularity()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            var firstClosing = new DateOnly(2025, 9, 6);
            for (int month = 0; month < 120; month++)
            {
                await scenario.AddCycle(firstClosing.AddMonths(month));
            }

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate("[AmEx.RataUltimoCicloChiuso]", firstClosing.AddMonths(119).AddDays(13));

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(0m);
        }

        [Fact]
        public async Task Validate_WhenCalculatedPropertyHasMixedCase_ReturnsCanonicalName()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();

            // Act
            FormulaValidationResult result = await scenario.Evaluator.Validate("[aMeX.iNteressiCiclo]");

            // Assert
            result.IsValid.Should().BeTrue();
            result.Formula.Should().Be("[AmEx.InteressiCiclo]");
        }

        [Fact]
        public async Task Evaluate_WhenRequiredParameterIsMissing_ReturnsErrorInsteadOfZero()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create();
            await scenario.Parameters.Delete(scenario.Card.Id, "Tan");

            // Act
            FormulaEvaluationResult result = await scenario.Evaluator.Evaluate("[AmEx.InteressiCiclo]", new DateOnly(2025, 9, 6));

            // Assert
            result.Value.Should().BeNull();
            result.Error.Should().Contain("Tan");
        }
    }
}

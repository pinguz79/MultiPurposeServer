using Finance.Desktop.Models;
using Finance.Desktop.Presentation;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class CycleSummaryTests
    {
        [Theory]
        [InlineData(9, 21, true, false, 1114.60)]
        [InlineData(10, 7, false, false, 1129.60)]
        [InlineData(9, 1, false, true, 1129.60)]
        public void CycleSummary_WhenPeriodChanges_SeparatesCurrentAndClosingBalances(int month, int day, bool open, bool future, decimal expected)
        {
            // Arrange
            var cycle = new Ciclo(new DateOnly(2026, 9, 7), new DateOnly(2026, 10, 6), -80.40m,
                [new(Guid.NewGuid(), new DateOnly(2026, 9, 17), "Spesa", 22m, 0m, 0m),
                new(Guid.NewGuid(), new DateOnly(2026, 9, 19), "Rimborso", -117.40m, 0m, 0m),
                new(Guid.NewGuid(), new DateOnly(2026, 10, 6), "Interessi", 15m, 0m, 0m)]);

            // Act
            var summary = new CycleSummary(cycle, 1210m, new DateOnly(2026, month, day), null);

            // Assert
            summary.OpeningBalance.Should().Be(1210m);
            summary.ClosingBalance.Should().Be(1129.60m);
            summary.ReferenceBalance.Should().Be(expected);
            summary.IsOpen.Should().Be(open);
            summary.IsFuture.Should().Be(future);
            summary.RemainingPlafond.Should().BeNull();
        }

        [Theory]
        [InlineData(1078.63, 521.37, 521)]
        [InlineData(1601.73, -1.73, 0)]
        public void CycleSummary_WhenEmptyHistoricalCycle_UsesHistoricalPlafondAndPreservesExactBalance(decimal balance, decimal remaining, decimal statement)
        {
            // Arrange
            var cycle = new Ciclo(new DateOnly(2024, 9, 7), new DateOnly(2024, 10, 6), 0m, []);
            var plafond = new ParametroConto("Plafond", "Plafond", TipoParametroConto.Importo, 2000m, null, null,
                [new(null, "Nuovo", 2000m, new DateOnly(2025, 1, 1), null, 0), new(null, "Base", 1600m, null, null, 1)]);

            // Act
            var summary = new CycleSummary(cycle, balance, new DateOnly(2026, 9, 21), plafond);

            // Assert
            summary.ClosingBalance.Should().Be(balance);
            summary.RemainingPlafond.Should().Be(remaining);
            summary.StatementRemainingPlafond.Should().Be(statement);
        }
    }
}

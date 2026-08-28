using Finance.Desktop.Models;
using Finance.Desktop.Presentation;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class MovimentiTimelineCalculatorTests
    {
        [Fact]
        public void CalculateMonth_CurrentMonth_ReturnsDeltaAndBalanceAtToday()
        {
            // Arrange
            DateOnly today = new(2026, 8, 28);
            ContoMovimenti timeline = CreateTimeline(
                new Movimento(Guid.NewGuid(), new DateOnly(2026, 8, 20), "Passato", -25m, 975m),
                new Movimento(Guid.NewGuid(), today, "Oggi", 100m, 1075m),
                new Movimento(Guid.NewGuid(), new DateOnly(2026, 8, 30), "Futuro", -10m, 1065m));

            // Act
            MonthSummary result = MovimentiTimelineCalculator.CalculateMonth(timeline, new DateOnly(2026, 8, 1), today);

            // Assert
            result.Delta.Should().Be(65m);
            result.CurrentBalance.Should().Be(1075m);
            result.Movements.Should().HaveCount(3);
        }

        [Fact]
        public void CalculateMonth_MonthWithoutMovements_ReturnsZeroDelta()
        {
            // Arrange
            DateOnly today = new(2026, 8, 28);
            ContoMovimenti timeline = CreateTimeline();

            // Act
            MonthSummary result = MovimentiTimelineCalculator.CalculateMonth(timeline, new DateOnly(2026, 7, 1), today);

            // Assert
            result.Delta.Should().Be(0m);
            result.CurrentBalance.Should().BeNull();
            result.Movements.Should().BeEmpty();
        }

        private static ContoMovimenti CreateTimeline(params Movimento[] movements) => new(
            new Conto(Guid.NewGuid(), "Conto", "Conto", 1000m),
            8,
            2026,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 9, 30),
            1000m,
            movements.LastOrDefault()?.BalanceAfter ?? 1000m,
            movements);
    }
}

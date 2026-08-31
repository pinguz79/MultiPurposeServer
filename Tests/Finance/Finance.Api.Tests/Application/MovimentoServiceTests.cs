using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Application
{
    public class MovimentoServiceTests
    {
        [Fact]
        public async Task GetTimelineCalculatesOpeningAndProgressiveBalancesInDateAndIdOrder()
        {
            // Arrange
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 100m };
            Movimento beforeRange = new() { Id = Guid.NewGuid(), ContoId = conto.Id, Date = new DateOnly(2026, 6, 30), Description = "Prima", Formula = "10,00" };
            Movimento first = new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), ContoId = conto.Id, Date = new DateOnly(2026, 8, 10), Description = "Primo", Formula = "20,00" };
            Movimento second = new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), ContoId = conto.Id, Date = new DateOnly(2026, 8, 10), Description = "Secondo", Formula = "-5,00" };
            var contoRepository = new Mock<IContoRepository>();
            contoRepository.Setup(repository => repository.GetByName(conto.Name)).ReturnsAsync(conto);
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(repository => repository.GetPreviousDates(conto.Id, new DateOnly(2026, 8, 1), 15)).ReturnsAsync([beforeRange.Date]);
            movimentoRepository.Setup(repository => repository.GetNextDates(conto.Id, new DateOnly(2026, 8, 31), 15)).ReturnsAsync([]);
            movimentoRepository.Setup(repository => repository.GetByContoThrough(conto.Id, new DateOnly(2026, 9, 30))).ReturnsAsync([beforeRange, first, second]);
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            await using var context = new FinanceContext(options);
            var evaluator = CreateEvaluator(beforeRange, first, second);
            var service = new MovimentoService(contoRepository.Object, movimentoRepository.Object, evaluator.Object, new EntityFrameworkPersistenceCoordinator<FinanceContext>(context));

            // Act
            var result = await service.GetTimeline(conto.Name, 8, 2026);

            // Assert
            result.From.Should().Be(new DateOnly(2026, 7, 1));
            result.To.Should().Be(new DateOnly(2026, 9, 30));
            result.OpeningBalance.Should().Be(110m);
            result.Items.Select(item => item.Id).Should().ContainInOrder(first.Id, second.Id);
            result.Items.Select(item => item.BalanceAfter).Should().ContainInOrder(130m, 125m);
            result.ClosingBalance.Should().Be(125m);
        }

        [Fact]
        public async Task GetTimelineExtendsRangeToIncludeMinimumMovementsAndDateTies()
        {
            // Arrange
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express" };
            Movimento[] movements =
            [
                .. Enumerable.Range(0, 16).Select(index => new Movimento
                {
                    Id = Guid.NewGuid(),
                    ContoId = conto.Id,
                    Date = index < 2 ? new DateOnly(2026, 6, 10) : new DateOnly(2026, 6, 11 + index),
                    Description = index.ToString(),
                    Formula = "0,00",
                }),
            ];
            var contoRepository = new Mock<IContoRepository>();
            contoRepository.Setup(repository => repository.GetByName(conto.Name)).ReturnsAsync(conto);
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(repository => repository.GetPreviousDates(conto.Id, new DateOnly(2026, 8, 1), 15))
                .ReturnsAsync([.. movements.OrderByDescending(movimento => movimento.Date).ThenByDescending(movimento => movimento.Id).Take(15).Select(movimento => movimento.Date)]);
            movimentoRepository.Setup(repository => repository.GetNextDates(conto.Id, new DateOnly(2026, 8, 31), 15)).ReturnsAsync([]);
            movimentoRepository.Setup(repository => repository.GetByContoThrough(conto.Id, new DateOnly(2026, 9, 30)))
                .ReturnsAsync([.. movements.OrderBy(movimento => movimento.Date).ThenBy(movimento => movimento.Id)]);
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            await using var context = new FinanceContext(options);
            var evaluator = new Mock<IFormulaEvaluator>();
            evaluator.Setup(item => item.Evaluate(It.IsAny<string>(), It.IsAny<DateOnly>())).ReturnsAsync(new FormulaEvaluationResult(0m, false, null));
            var service = new MovimentoService(contoRepository.Object, movimentoRepository.Object, evaluator.Object, new EntityFrameworkPersistenceCoordinator<FinanceContext>(context));

            // Act
            var result = await service.GetTimeline(conto.Name, 8, 2026);

            // Assert
            result.From.Should().BeBefore(new DateOnly(2026, 7, 1));
            result.Items.Count(item => item.Date == result.From).Should().BeGreaterThanOrEqualTo(1);
        }

        private static Mock<IFormulaEvaluator> CreateEvaluator(params Movimento[] movements)
        {
            var evaluator = new Mock<IFormulaEvaluator>();

            foreach (Movimento movimento in movements)
            {
                decimal value = decimal.Parse(movimento.Formula.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                evaluator.Setup(item => item.Evaluate(movimento.Formula, movimento.Date)).ReturnsAsync(new FormulaEvaluationResult(value, false, null));
            }

            return evaluator;
        }
    }
}

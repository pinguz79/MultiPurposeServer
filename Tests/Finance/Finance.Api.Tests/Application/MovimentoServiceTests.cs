using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Responses;
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

        [Fact]
        public async Task GetCycleTimelineHidesTechnicalMovementsButIncludesThemInOverallBalance()
        {
            // Arrange
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloCard", DisplayName = "Hello Card" };
            var tecnico = new Categoria { Id = Guid.NewGuid(), Name = "Tecnico", DisplayName = "Tecnico" };
            Movimento beforeRange = new() { Id = Guid.NewGuid(), ContoId = conto.Id, Date = new DateOnly(2026, 6, 1), Description = "Precedente", Formula = "10,00" };
            Movimento purchase = new() { Id = Guid.NewGuid(), ContoId = conto.Id, Date = new DateOnly(2026, 7, 23), Description = "Acquisto", Formula = "100,00" };
            Movimento reset = new() { Id = Guid.NewGuid(), ContoId = conto.Id, Date = new DateOnly(2026, 8, 6), Description = "Ripristino", Formula = "-80,00", Categoria = tecnico };
            Movimento secondPurchase = new() { Id = Guid.NewGuid(), ContoId = conto.Id, Date = new DateOnly(2026, 8, 10), Description = "Secondo acquisto", Formula = "20,00" };
            var contoRepository = new Mock<IContoRepository>();
            contoRepository.Setup(repository => repository.GetByName(conto.Name)).ReturnsAsync(conto);
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(repository => repository.GetPreviousDates(conto.Id, new DateOnly(2026, 7, 22), 15)).ReturnsAsync([beforeRange.Date]);
            movimentoRepository.Setup(repository => repository.GetNextDates(conto.Id, new DateOnly(2026, 8, 21), 15)).ReturnsAsync([]);
            movimentoRepository.Setup(repository => repository.GetByContoThrough(conto.Id, new DateOnly(2026, 9, 21)))
                .ReturnsAsync([beforeRange, purchase, reset, secondPurchase]);
            var parameterService = new Mock<IParametroContoService>();
            parameterService.Setup(service => service.Resolve(conto.Id, "ChiusuraCiclo", new DateOnly(2026, 8, 31)))
                .ReturnsAsync(new ParametroConto { Name = "ChiusuraCiclo", Type = TipoParametroConto.Intero, Value = 21m });
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            await using var context = new FinanceContext(options);
            var service = new MovimentoService(
                contoRepository.Object,
                movimentoRepository.Object,
                CreateEvaluator(beforeRange, purchase, reset, secondPurchase).Object,
                new EntityFrameworkPersistenceCoordinator<FinanceContext>(context),
                parametroContoService: parameterService.Object);

            // Act
            var result = await service.GetCycleTimeline(conto.Name, 8, 2026);

            // Assert
            result.From.Should().Be(new DateOnly(2026, 6, 22));
            result.To.Should().Be(new DateOnly(2026, 9, 21));
            result.OpeningBalance.Should().Be(10m);
            result.ClosingBalance.Should().Be(50m);
            CicloDto selectedCycle = result.Cycles.Single(cycle => cycle.To == new DateOnly(2026, 8, 21));
            selectedCycle.Items.Select(item => item.Description).Should().Equal("Acquisto", "Secondo acquisto");
            selectedCycle.Items.Select(item => item.CycleBalanceAfter).Should().Equal(100m, 120m);
            selectedCycle.Items.Select(item => item.BalanceAfter).Should().Equal(110m, 50m);
            selectedCycle.Total.Should().Be(120m);
        }

        [Fact]
        public async Task UpdateAssignsCategoryResolvedByLogicalName()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Casa", DisplayName = "Casa" };
            var movimento = new Movimento { Id = id, CategoriaId = categoria.Id };
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(repository => repository.Update(id, null, null, null, categoria.Id, false)).ReturnsAsync(movimento);
            var categoriaService = new Mock<ICategoriaService>();
            categoriaService.Setup(service => service.Resolve("casa")).ReturnsAsync(categoria);
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            await using var context = new FinanceContext(options);
            var service = new MovimentoService(
                Mock.Of<IContoRepository>(),
                movimentoRepository.Object,
                Mock.Of<IFormulaEvaluator>(),
                new EntityFrameworkPersistenceCoordinator<FinanceContext>(context),
                categoriaService.Object);

            // Act
            Movimento result = await service.Update(id, null, null, null, "casa");

            // Assert
            result.CategoriaId.Should().Be(categoria.Id);
            movimentoRepository.VerifyAll();
            categoriaService.VerifyAll();
        }

        [Fact]
        public async Task UpdateRejectsCategoryAndClearCategoryTogether()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            await using var context = new FinanceContext(options);
            var service = new MovimentoService(
                Mock.Of<IContoRepository>(),
                Mock.Of<IMovimentoRepository>(),
                Mock.Of<IFormulaEvaluator>(),
                new EntityFrameworkPersistenceCoordinator<FinanceContext>(context));

            // Act
            var action = () => service.Update(Guid.NewGuid(), null, null, null, "Casa", true);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
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

using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Application
{
    public class CycleIndicatorsServiceTests
    {
        [Fact]
        public async Task GetReturnsNullWhenAccountDoesNotHaveCompleteCardProfile()
        {
            // Arrange
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank" };
            var parametroService = new Mock<IParametroContoService>();
            parametroService.Setup(item => item.Resolve(conto.Id, "Plafond", It.IsAny<DateOnly>())).ReturnsAsync((ParametroConto?)null);
            var service = new CycleIndicatorsService(parametroService.Object, Mock.Of<IMovimentoRepository>(), Mock.Of<IFormulaEvaluator>());

            // Act
            CycleIndicators? result = await service.Get(conto, 1_000m, new DateOnly(2026, 9, 25));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSeparatesCurrentCycleSpentFromClosedCyclePendingDebit()
        {
            // Arrange
            var date = new DateOnly(2026, 9, 25);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloCard" };
            Mock<IParametroContoService> parametroService = CreateCardProfile(conto.Id, date);
            var movimento = new Movimento { Id = Guid.NewGuid(), Date = date, Formula = "1000.00" };
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(item => item.GetByContoThrough(conto.Id, date)).ReturnsAsync([movimento]);
            movimentoRepository.Setup(item => item.GetByContoAfter(conto.Id, date)).ReturnsAsync([]);
            var formulaEvaluator = new Mock<IFormulaEvaluator>();
            formulaEvaluator.Setup(item => item.Evaluate("1000.00", date)).ReturnsAsync(new FormulaEvaluationResult(1_000m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date))
                .ReturnsAsync(new FormulaEvaluationResult(5_000m, false, null));
            var service = new CycleIndicatorsService(parametroService.Object, movimentoRepository.Object, formulaEvaluator.Object);

            // Act
            CycleIndicators result = (await service.Get(conto, 6_000m, date))!;

            // Assert
            result.From.Should().Be(new DateOnly(2026, 9, 22));
            result.To.Should().Be(new DateOnly(2026, 10, 21));
            result.CurrentCycleSpent.Should().Be(1_000m);
            result.PendingDebit.Should().Be(5_000m);
            result.RemainingPlafond.Should().Be(-1_000m);
            result.RemainingIncludingOverdraft.Should().Be(-500m);
        }

        [Fact]
        public async Task GetFindsFirstFutureCrossingOfBothCardLimits()
        {
            // Arrange
            var date = new DateOnly(2026, 9, 4);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloCard" };
            Mock<IParametroContoService> parametroService = CreateCardProfile(conto.Id, date);
            var firstDate = date.AddDays(1);
            var secondDate = date.AddDays(2);
            var first = new Movimento { Id = Guid.NewGuid(), Date = firstDate, Formula = "250.00" };
            var second = new Movimento { Id = Guid.NewGuid(), Date = secondDate, Formula = "400.00" };
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(item => item.GetByContoThrough(conto.Id, date)).ReturnsAsync([]);
            movimentoRepository.Setup(item => item.GetByContoAfter(conto.Id, date)).ReturnsAsync([first, second]);
            var formulaEvaluator = new Mock<IFormulaEvaluator>();
            formulaEvaluator.Setup(item => item.Evaluate("250.00", firstDate)).ReturnsAsync(new FormulaEvaluationResult(250m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("400.00", secondDate)).ReturnsAsync(new FormulaEvaluationResult(400m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date))
                .ReturnsAsync(new FormulaEvaluationResult(0m, false, null));
            var service = new CycleIndicatorsService(parametroService.Object, movimentoRepository.Object, formulaEvaluator.Object);

            // Act
            CycleIndicators result = (await service.Get(conto, 4_900m, date))!;

            // Assert
            result.FirstPlafondExceeded.Should().Be(new CycleThreshold(firstDate, 5_150m, 150m));
            result.FirstTotalLimitExceeded.Should().Be(new CycleThreshold(secondDate, 5_550m, 50m));
        }

        [Fact]
        public async Task GetFindsNextCrossingAfterCurrentlyExceededBalanceReturnsBelowLimits()
        {
            // Arrange
            var date = new DateOnly(2026, 9, 4);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloCard" };
            Mock<IParametroContoService> parametroService = CreateCardProfile(conto.Id, date);
            var resetDate = date.AddDays(2);
            var nextCrossingDate = date.AddDays(3);
            var reset = new Movimento { Id = Guid.NewGuid(), Date = resetDate, Formula = "-1500.00" };
            var purchase = new Movimento { Id = Guid.NewGuid(), Date = nextCrossingDate, Formula = "2000.00" };
            var movimentoRepository = new Mock<IMovimentoRepository>();
            movimentoRepository.Setup(item => item.GetByContoThrough(conto.Id, date)).ReturnsAsync([]);
            movimentoRepository.Setup(item => item.GetByContoAfter(conto.Id, date)).ReturnsAsync([reset, purchase]);
            var formulaEvaluator = new Mock<IFormulaEvaluator>();
            formulaEvaluator.Setup(item => item.Evaluate("-1500.00", resetDate)).ReturnsAsync(new FormulaEvaluationResult(-1_500m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("2000.00", nextCrossingDate)).ReturnsAsync(new FormulaEvaluationResult(2_000m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date))
                .ReturnsAsync(new FormulaEvaluationResult(0m, false, null));
            var service = new CycleIndicatorsService(parametroService.Object, movimentoRepository.Object, formulaEvaluator.Object);

            // Act
            CycleIndicators result = (await service.Get(conto, 6_000m, date))!;

            // Assert
            result.FirstPlafondExceeded.Should().Be(new CycleThreshold(nextCrossingDate, 6_500m, 1_500m));
            result.FirstTotalLimitExceeded.Should().Be(new CycleThreshold(nextCrossingDate, 6_500m, 1_000m));
        }

        private static Mock<IParametroContoService> CreateCardProfile(Guid contoId, DateOnly date)
        {
            var service = new Mock<IParametroContoService>();
            Setup(service, contoId, date, "Plafond", TipoParametroConto.Importo, 5_000m);
            Setup(service, contoId, date, "PercentualeScoperto", TipoParametroConto.Percentuale, 0.10m);
            Setup(service, contoId, date, "ChiusuraCiclo", TipoParametroConto.Intero, 21m);
            Setup(service, contoId, date, "Addebito", TipoParametroConto.Intero, 5m);
            Setup(service, contoId, date, "RipristinoPlafond", TipoParametroConto.Intero, 6m);

            return service;
        }

        private static void Setup(
            Mock<IParametroContoService> service,
            Guid contoId,
            DateOnly date,
            string name,
            TipoParametroConto type,
            decimal value)
            => service.Setup(item => item.Resolve(contoId, name, date)).ReturnsAsync(new ParametroConto
            {
                Name = name,
                DisplayName = name,
                Type = type,
                Value = value,
            });
    }
}

using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Application
{
    public class ContoServiceTests
    {
        [Fact]
        public async Task CreateNormalizesNameAndPersistsConto()
        {
            // Arrange
            var repository = new Mock<IContoRepository>();
            repository.Setup(item => item.CreateConto("AmericanExpress", "American Express", 123.45m))
                .ReturnsAsync(new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 123.45m });
            var service = CreateService(repository.Object);

            // Act
            var result = await service.CreateConto("american express", " American Express ", 123.45m);

            // Assert
            result.Name.Should().Be("AmericanExpress");
            repository.Verify(item => item.CreateConto("AmericanExpress", "American Express", 123.45m), Times.Once);
        }

        [Fact]
        public void NormalizeNamePreservesPascalCaseWordBoundaries()
        {
            // Arrange
            const string Name = "AmericanExpress";

            // Act
            var result = ContoService.NormalizeName(Name);

            // Assert
            result.Should().Be(Name);
        }

        [Fact]
        public async Task CreateRejectsDuplicateNormalizedName()
        {
            // Arrange
            var repository = new Mock<IContoRepository>();
            repository.Setup(item => item.NameExists("AmericanExpress")).ReturnsAsync(true);
            var service = CreateService(repository.Object);

            // Act
            var action = () => service.CreateConto("American Express", "American Express", 0m);

            // Assert
            await action.Should().ThrowAsync<DuplicateNameException>();
            repository.Verify(item => item.CreateConto(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task CreateRejectsAmountWithMoreThanTwoDecimals()
        {
            // Arrange
            var service = CreateService(Mock.Of<IContoRepository>());

            // Act
            var action = () => service.CreateConto("Conto", "Conto", 1.001m);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetContiOrdersByDisplayNameThenNameIgnoringCase()
        {
            // Arrange
            var repository = new Mock<IContoRepository>();
            repository.Setup(item => item.GetConti()).ReturnsAsync(
            [
                new Conto { Name = "Zulu", DisplayName = "conto" },
                new Conto { Name = "Alfa", DisplayName = "Conto" },
                new Conto { Name = "Beta", DisplayName = "Altro" },
            ]);
            var service = CreateService(repository.Object);

            // Act
            var result = await service.GetConti();

            // Assert
            result.Select(conto => conto.Name).Should().ContainInOrder("Beta", "Alfa", "Zulu");
        }

        [Fact]
        public async Task GetStatusReturnsFirstFutureDateWhoseClosingBalanceIsNegative()
        {
            // Arrange
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var conto = new Conto { Id = Guid.NewGuid(), InitialBalance = 100m };
            var firstDate = today.AddDays(1);
            var secondDate = today.AddDays(2);
            var movementRepository = new Mock<IMovimentoRepository>();
            movementRepository.Setup(item => item.GetByContoThrough(conto.Id, today)).ReturnsAsync([]);
            movementRepository.Setup(item => item.GetByContoAfter(conto.Id, today)).ReturnsAsync(
            [
                new Movimento { Date = firstDate, Formula = "-120" },
                new Movimento { Date = firstDate, Formula = "50" },
                new Movimento { Date = secondDate, Formula = "-40" },
            ]);
            var formulaEvaluator = new Mock<IFormulaEvaluator>();
            formulaEvaluator.Setup(item => item.Evaluate("-120", firstDate)).ReturnsAsync(new FormulaEvaluationResult(-120m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("50", firstDate)).ReturnsAsync(new FormulaEvaluationResult(50m, false, null));
            formulaEvaluator.Setup(item => item.Evaluate("-40", secondDate)).ReturnsAsync(new FormulaEvaluationResult(-40m, false, null));
            var cycleIndicatorsService = new Mock<ICycleIndicatorsService>();
            cycleIndicatorsService.Setup(item => item.Get(conto, 100m, today)).ReturnsAsync((CycleIndicators?)null);
            var service = new ContoService(
                Mock.Of<IContoRepository>(),
                movementRepository.Object,
                formulaEvaluator.Object,
                cycleIndicatorsService.Object,
                Mock.Of<IRevolvingIndicatorsService>());

            // Act
            ContoStatus result = await service.GetStatus(conto);

            // Assert
            result.Balance.Should().Be(100m);
            result.FirstNegativeBalanceDate.Should().Be(secondDate);
            result.FirstNegativeBalance.Should().Be(-10m);
        }

        [Fact]
        public async Task UpdateNormalizesNameAndPersistsConto()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var repository = new Mock<IContoRepository>();
            repository.Setup(item => item.NameExists("AmericanExpress", contoId)).ReturnsAsync(false);
            repository.Setup(item => item.UpdateConto(contoId, "AmericanExpress", null, null))
                .ReturnsAsync(new Conto { Id = contoId, Name = "AmericanExpress", DisplayName = "American Express" });
            var service = CreateService(repository.Object);

            // Act
            var result = await service.UpdateConto(contoId, "american express", null, null);

            // Assert
            result.Name.Should().Be("AmericanExpress");
            repository.Verify(item => item.UpdateConto(contoId, "AmericanExpress", null, null), Times.Once);
        }

        [Fact]
        public async Task UpdateRejectsNameOwnedByAnotherConto()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var repository = new Mock<IContoRepository>();
            repository.Setup(item => item.NameExists("AmericanExpress", contoId)).ReturnsAsync(true);
            var service = CreateService(repository.Object);

            // Act
            var action = () => service.UpdateConto(contoId, "American Express", null, null);

            // Assert
            await action.Should().ThrowAsync<DuplicateNameException>();
            repository.Verify(item => item.UpdateConto(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>()), Times.Never);
        }

        [Fact]
        public async Task GetStatus_WhenRevolving_DoesNotIncludeFutureChargesInCurrentDebt()
        {
            // Arrange
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var conto = new Conto { InitialBalance = 1500m };
            var movements = new Mock<IMovimentoRepository>(MockBehavior.Strict);
            movements.Setup(item => item.GetByContoThrough(conto.Id, today)).ReturnsAsync([]);
            var indicators = new RevolvingIndicators(1600m, 160m, 100m, 260m);
            var revolving = new Mock<IRevolvingIndicatorsService>();
            revolving.Setup(item => item.Get(conto, 1500m, today)).ReturnsAsync(indicators);
            var service = new ContoService(Mock.Of<IContoRepository>(), movements.Object, Mock.Of<IFormulaEvaluator>(), Mock.Of<ICycleIndicatorsService>(), revolving.Object);

            // Act
            ContoStatus result = await service.GetStatus(conto);

            // Assert
            result.Balance.Should().Be(1500m);
            result.RevolvingIndicators.Should().Be(indicators);
            result.FirstNegativeBalanceDate.Should().BeNull();
            movements.Verify(item => item.GetByContoAfter(It.IsAny<Guid>(), It.IsAny<DateOnly>()), Times.Never);
        }

        private static ContoService CreateService(IContoRepository repository)
            => new(repository, Mock.Of<IMovimentoRepository>(), Mock.Of<IFormulaEvaluator>(), Mock.Of<ICycleIndicatorsService>(), Mock.Of<IRevolvingIndicatorsService>());

        [Theory]
        [InlineData("2026-10-21", 21, "2026-09-21")]
        [InlineData("2026-10-22", 21, "2026-10-21")]
        [InlineData("2026-03-01", 31, "2026-02-28")]
        [InlineData("2024-03-01", 31, "2024-02-29")]
        [InlineData("2026-01-01", 31, "2025-12-31")]
        public void GetPreviousPeriodBalance_WhenClosingBoundaryChanges_UsesEndOfPreviousCycle(string date, int day, string previousEnd)
        {
            // Arrange
            DateOnly today = DateOnly.ParseExact(date, "yyyy-MM-dd");
            DateOnly end = DateOnly.ParseExact(previousEnd, "yyyy-MM-dd");
            var conto = new Conto { InitialBalance = 100m, Parametri = [new ParametroConto { Name = "ChiusuraCiclo", Type = TipoParametroConto.Intero, Value = day }] };
            IReadOnlyList<(Movimento Movement, decimal Amount)> movements = [(new Movimento { Date = end }, -20m), (new Movimento { Date = end.AddDays(1) }, 5m)];
            var method = typeof(ContoService).GetMethod("GetPreviousPeriodBalance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;

            // Act
            var result = (decimal)method.Invoke(null, [conto, today, null, null, movements])!;

            // Assert
            result.Should().Be(80m);
        }

        [Theory]
        [InlineData(false, false, 90)]
        [InlineData(true, false, 20)]
        [InlineData(true, true, 90)]
        public async Task GetStatus_WhenPreviousPeriodExists_ReturnsComparableBalanceWithoutExtraQueries(bool cyclic, bool revolving, decimal expected)
        {
            // Arrange
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var month = new DateOnly(today.Year, today.Month, 1);
            DateOnly end = month.AddDays(-1);
            var conto = new Conto { Id = Guid.NewGuid(), InitialBalance = 100m };
            if (cyclic)
            {
                conto.Parametri.Add(new ParametroConto { Name = "ChiusuraCiclo", Type = TipoParametroConto.Intero, Value = 31m });
            }
            var repository = new Mock<IMovimentoRepository>(MockBehavior.Strict);
            repository.Setup(item => item.GetByContoThrough(conto.Id, today)).ReturnsAsync(
            [
                new Movimento { Date = end, Formula = "20" },
                new Movimento { Date = end, Formula = "-30", Categoria = new Categoria { Name = "Tecnico" } },
                new Movimento { Date = today, Formula = "5" },
            ]);
            repository.Setup(item => item.GetByContoAfter(conto.Id, today)).ReturnsAsync([]);
            var evaluator = new Mock<IFormulaEvaluator>();
            evaluator.Setup(item => item.Evaluate("20", end)).ReturnsAsync(new FormulaEvaluationResult(20m, false, null));
            evaluator.Setup(item => item.Evaluate("-30", end)).ReturnsAsync(new FormulaEvaluationResult(-30m, false, null));
            evaluator.Setup(item => item.Evaluate("5", today)).ReturnsAsync(new FormulaEvaluationResult(5m, false, null));
            var cycles = new Mock<ICycleIndicatorsService>();
            if (cyclic)
            {
                cycles.Setup(item => item.Get(conto, 95m, today)).ReturnsAsync(new CycleIndicators(month, month.AddMonths(1).AddDays(-1), 5m, 0m, 0m, 0m, 0m, 0m, null, null, null));
            }
            var revolvings = new Mock<IRevolvingIndicatorsService>();
            if (revolving)
            {
                revolvings.Setup(item => item.Get(conto, 95m, today)).ReturnsAsync(new RevolvingIndicators(0m, 0m, 0m, 0m));
            }
            var service = new ContoService(Mock.Of<IContoRepository>(), repository.Object, evaluator.Object, cycles.Object, revolvings.Object);

            // Act
            ContoStatus result = await service.GetStatus(conto);

            // Assert
            result.PreviousPeriodBalance.Should().Be(expected);
            result.Balance.Should().Be(95m);
            repository.Verify(item => item.GetByContoThrough(conto.Id, today), Times.Once);
            evaluator.Verify(item => item.Evaluate(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Exactly(3));
        }
    }
}

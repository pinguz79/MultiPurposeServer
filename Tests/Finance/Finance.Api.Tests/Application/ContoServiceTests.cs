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
                cycleIndicatorsService.Object);

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

        private static ContoService CreateService(IContoRepository repository)
            => new(repository, Mock.Of<IMovimentoRepository>(), Mock.Of<IFormulaEvaluator>(), Mock.Of<ICycleIndicatorsService>());
    }
}

using Finance.Api.Application;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Application
{
    public class RevolvingIndicatorsServiceTests
    {
        [Fact]
        public async Task Get_WhenFixedPaymentExceedsLimit_ReturnsNegativeAvailabilityWithoutOverdraft()
        {
            // Arrange
            var date = new DateOnly(2026, 10, 1);
            var conto = new Conto();
            var parameters = new Mock<IParametroContoService>();
            parameters.Setup(item => item.Resolve(conto.Id, "Rata", date)).ReturnsAsync(new ParametroConto { Type = TipoParametroConto.Importo, Value = 500m });
            parameters.Setup(item => item.Resolve(conto.Id, "Plafond", date)).ReturnsAsync(new ParametroConto { Type = TipoParametroConto.Importo, Value = 5600m });
            parameters.Setup(item => item.Resolve(conto.Id, "PercentualeScoperto", date)).ReturnsAsync(new ParametroConto { Type = TipoParametroConto.Percentuale, Value = 0m });
            var service = new RevolvingIndicatorsService(parameters.Object);

            // Act
            RevolvingIndicators? result = await service.Get(conto, 5650m, date);

            // Assert
            result.Should().Be(new RevolvingIndicators(5600m, 0m, -50m, -50m));
        }

        [Theory]
        [InlineData(1500, 100, 260)]
        [InlineData(1650, -50, 110)]
        [InlineData(1800, -200, -40)]
        [InlineData(-100, 1700, 1860)]
        public async Task Get_WhenRevolving_ReturnsAvailabilityFromCurrentDebt(decimal balance, decimal remaining, decimal includingOverdraft)
        {
            // Arrange
            var date = new DateOnly(2026, 9, 15);
            var conto = new Conto();
            var parameters = new Mock<IParametroContoService>();
            parameters.Setup(item => item.Resolve(conto.Id, "QuotaRata", date)).ReturnsAsync(new ParametroConto { Value = 0.1m });
            parameters.Setup(item => item.Resolve(conto.Id, "Plafond", date)).ReturnsAsync(new ParametroConto { Type = TipoParametroConto.Importo, Value = 1600m });
            parameters.Setup(item => item.Resolve(conto.Id, "PercentualeScoperto", date)).ReturnsAsync(new ParametroConto { Type = TipoParametroConto.Percentuale, Value = 0.1m });
            var service = new RevolvingIndicatorsService(parameters.Object);

            // Act
            RevolvingIndicators? result = await service.Get(conto, balance, date);

            // Assert
            result.Should().Be(new RevolvingIndicators(1600m, 160m, remaining, includingOverdraft));
        }

        [Fact]
        public async Task Get_WhenOrdinaryAccount_ReturnsNull()
        {
            // Arrange
            var service = new RevolvingIndicatorsService(Mock.Of<IParametroContoService>());

            // Act
            RevolvingIndicators? result = await service.Get(new Conto(), 100m, new DateOnly(2026, 9, 15));

            // Assert
            result.Should().BeNull();
        }
    }
}

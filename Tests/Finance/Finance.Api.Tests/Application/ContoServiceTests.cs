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
            var service = new ContoService(repository.Object);

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
            var service = new ContoService(repository.Object);

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
            var service = new ContoService(Mock.Of<IContoRepository>());

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
            var service = new ContoService(repository.Object);

            // Act
            var result = await service.GetConti();

            // Assert
            result.Select(conto => conto.Name).Should().ContainInOrder("Beta", "Alfa", "Zulu");
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
            var service = new ContoService(repository.Object);

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
            var service = new ContoService(repository.Object);

            // Act
            var action = () => service.UpdateConto(contoId, "American Express", null, null);

            // Assert
            await action.Should().ThrowAsync<DuplicateNameException>();
            repository.Verify(item => item.UpdateConto(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>()), Times.Never);
        }
    }
}

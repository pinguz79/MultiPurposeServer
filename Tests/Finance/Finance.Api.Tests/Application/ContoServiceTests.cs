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
            repository.Setup(item => item.Create("AmericanExpress", "American Express", 123.45m))
                .ReturnsAsync(new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 123.45m });
            var service = new ContoService(repository.Object);

            // Act
            var result = await service.Create("american express", " American Express ", 123.45m);

            // Assert
            result.Name.Should().Be("AmericanExpress");
            repository.Verify(item => item.Create("AmericanExpress", "American Express", 123.45m), Times.Once);
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
            var action = () => service.Create("American Express", "American Express", 0m);

            // Assert
            await action.Should().ThrowAsync<DuplicateNameException>();
            repository.Verify(item => item.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task CreateRejectsAmountWithMoreThanTwoDecimals()
        {
            // Arrange
            var service = new ContoService(Mock.Of<IContoRepository>());

            // Act
            var action = () => service.Create("Conto", "Conto", 1.001m);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetAllOrdersByDisplayNameThenNameIgnoringCase()
        {
            // Arrange
            var repository = new Mock<IContoRepository>();
            repository.Setup(item => item.GetAll()).ReturnsAsync(
            [
                new Conto { Name = "Zulu", DisplayName = "conto" },
                new Conto { Name = "Alfa", DisplayName = "Conto" },
                new Conto { Name = "Beta", DisplayName = "Altro" },
            ]);
            var service = new ContoService(repository.Object);

            // Act
            var result = await service.GetAll();

            // Assert
            result.Select(conto => conto.Name).Should().ContainInOrder("Beta", "Alfa", "Zulu");
        }
    }
}

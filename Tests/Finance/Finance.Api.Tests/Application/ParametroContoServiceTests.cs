using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Application
{
    public class ParametroContoServiceTests
    {
        [Fact]
        public async Task CreateNormalizesNameAndPreservesDefinitionOrder()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var repository = new Mock<IParametroContoRepository>();
            repository.Setup(item => item.NameExists(contoId, "PercentualeScoperto")).ReturnsAsync(false);
            repository.Setup(item => item.Replace(contoId, null, "PercentualeScoperto", TipoParametroConto.Percentuale,
                    It.IsAny<IReadOnlyList<ParametroConto>>()))
                .Returns<Guid, string?, string, TipoParametroConto, IReadOnlyList<ParametroConto>>((_, _, _, _, definitions)
                    => Task.FromResult(definitions));
            var service = CreateService(repository.Object, contoId);
            ParametroContoDefinitionRequest[] definitions =
            [
                new(null, "Eccezione", 0.15m, new DateOnly(2027, 1, 1), null),
                new(null, "Percentuale scoperto", 0.10m, null, null),
            ];

            // Act
            IReadOnlyList<ParametroConto> result = await service.Create("helloCard", "percentuale scoperto",
                TipoParametroConto.Percentuale, definitions);

            // Assert
            result.Select(definition => definition.Index).Should().ContainInOrder(0, 1);
            repository.Verify(item => item.Replace(contoId, null, "PercentualeScoperto", TipoParametroConto.Percentuale,
                It.IsAny<IReadOnlyList<ParametroConto>>()), Times.Once);
        }

        [Fact]
        public async Task CreateRejectsDefinitionsWithoutPermanentFallback()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var repository = new Mock<IParametroContoRepository>();
            repository.Setup(item => item.NameExists(contoId, "Plafond")).ReturnsAsync(false);
            var service = CreateService(repository.Object, contoId);
            ParametroContoDefinitionRequest[] definitions =
            [
                new(null, "Plafond", 5_000m, new DateOnly(2026, 1, 1), null),
            ];

            // Act
            var action = () => service.Create("helloCard", "plafond", TipoParametroConto.Importo, definitions);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
            repository.Verify(item => item.Replace(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<TipoParametroConto>(), It.IsAny<IReadOnlyList<ParametroConto>>()), Times.Never);
        }

        [Fact]
        public async Task CreateRejectsMonetaryValueWithMoreThanTwoDecimalPlaces()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var repository = new Mock<IParametroContoRepository>();
            repository.Setup(item => item.NameExists(contoId, "Plafond")).ReturnsAsync(false);
            var service = CreateService(repository.Object, contoId);
            ParametroContoDefinitionRequest[] definitions =
            [
                new(null, "Plafond", 5_000.001m, null, null),
            ];

            // Act
            var action = () => service.Create("helloCard", "plafond", TipoParametroConto.Importo, definitions);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteRejectsParameterReferencedByFormula()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var repository = new Mock<IParametroContoRepository>();
            repository.Setup(item => item.GetByName(contoId, "plafond")).ReturnsAsync([
                new ParametroConto { Name = "Plafond", DisplayName = "Plafond", Value = 5_000m },
            ]);
            repository.Setup(item => item.IsReferenced("HelloCard.Plafond")).ReturnsAsync(true);
            var service = CreateService(repository.Object, contoId);

            // Act
            var action = () => service.Delete("helloCard", "plafond");

            // Assert
            await action.Should().ThrowAsync<ParametroContoReferencedException>();
            repository.Verify(item => item.Delete(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        private static ParametroContoService CreateService(IParametroContoRepository repository, Guid contoId)
        {
            var contoRepository = new Mock<IContoRepository>();
            contoRepository.Setup(item => item.GetByName("helloCard"))
                .ReturnsAsync(new Conto { Id = contoId, Name = "HelloCard", DisplayName = "Hello Card" });
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            var context = new FinanceContext(options);

            return new ParametroContoService(repository, contoRepository.Object,
                new EntityFrameworkPersistenceCoordinator<FinanceContext>(context));
        }
    }
}

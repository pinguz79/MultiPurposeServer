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
    public class VoceRicorrenteServiceTests
    {
        [Fact]
        public async Task CreateNormalizesNameAndPreservesDefinitionOrder()
        {
            // Arrange
            var repository = new Mock<IVoceRicorrenteRepository>();
            repository.Setup(item => item.NameExists("RataMutuo")).ReturnsAsync(false);
            repository.Setup(item => item.Replace(null, "RataMutuo", It.IsAny<IReadOnlyList<VoceRicorrente>>()))
                .Returns<string?, string, IReadOnlyList<VoceRicorrente>>((_, _, definitions) => Task.FromResult(definitions));
            var service = CreateService(repository.Object);
            VoceRicorrenteDefinitionRequest[] definitions =
            [
                new(null, "Eccezione", 650m, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
                new(null, "Rata ordinaria", 600m, null, null),
            ];

            // Act
            IReadOnlyList<VoceRicorrente> result = await service.Create("rata mutuo", definitions);

            // Assert
            result.Select(definition => definition.Index).Should().ContainInOrder(0, 1);
            repository.Verify(item => item.Replace(null, "RataMutuo", It.IsAny<IReadOnlyList<VoceRicorrente>>()), Times.Once);
        }

        [Fact]
        public async Task CreateRejectsInvalidDateRange()
        {
            // Arrange
            var repository = new Mock<IVoceRicorrenteRepository>();
            repository.Setup(item => item.NameExists("Affitto")).ReturnsAsync(false);
            var service = CreateService(repository.Object);
            VoceRicorrenteDefinitionRequest[] definitions =
            [
                new(null, "Affitto", 600m, new DateOnly(2026, 8, 31), new DateOnly(2026, 8, 1)),
            ];

            // Act
            var action = () => service.Create("affitto", definitions);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
            repository.Verify(item => item.Replace(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<VoceRicorrente>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRejectsDefinitionOwnedByAnotherEntry()
        {
            // Arrange
            var persistedId = Guid.NewGuid();
            var repository = new Mock<IVoceRicorrenteRepository>();
            repository.Setup(item => item.GetByName("Affitto")).ReturnsAsync([new VoceRicorrente { Id = persistedId, Name = "Affitto" }]);
            var service = CreateService(repository.Object);
            VoceRicorrenteDefinitionRequest[] definitions =
            [
                new(Guid.NewGuid(), "Affitto", 600m, null, null),
            ];

            // Act
            var action = () => service.Update("Affitto", "Affitto", definitions);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
            repository.Verify(item => item.Replace(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<VoceRicorrente>>()), Times.Never);
        }

        private static VoceRicorrenteService CreateService(IVoceRicorrenteRepository repository)
        {
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite("Data Source=:memory:").Options;
            var context = new FinanceContext(options);

            return new VoceRicorrenteService(repository, new EntityFrameworkPersistenceCoordinator<FinanceContext>(context));
        }
    }
}

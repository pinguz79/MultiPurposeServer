using FluentAssertions;

using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Api.Tests.Infrastructure.Persistence.Repositories;

namespace Portfolio.Api.Tests.Infrastructure.Persistence
{
    public class EntityFrameworkPersistenceCoordinatorTests : RepositoryTestBase
    {
        [Fact]
        public async Task BeginTransaction_FromDifferentRepositories_WhenTransactionIsActive_RejectsNestedTransaction()
        {
            // Arrange
            var albumRepository = new AlbumRepository(DbContext, PersistenceCoordinator);
            var fotoRepository = new FotoRepository(DbContext, PersistenceCoordinator);
            await using var transaction = await albumRepository.BeginTransaction();

            // Act
            Func<Task> action = fotoRepository.BeginTransaction;

            // Assert
            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A persistence transaction is already active.");
        }

        [Fact]
        public async Task SaveIfRequired_FromDifferentRepository_WhenTransactionIsActive_DefersSave()
        {
            // Arrange
            var albumRepository = new AlbumRepository(DbContext, PersistenceCoordinator);
            var fotoRepository = new FotoRepository(DbContext, PersistenceCoordinator);
            await using var transaction = await albumRepository.BeginTransaction();

            // Act
            var affectedRows = await fotoRepository.SaveIfRequired();

            // Assert
            affectedRows.Should().Be(0);
        }
    }
}

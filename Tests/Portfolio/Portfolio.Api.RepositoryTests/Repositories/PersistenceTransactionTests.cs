using FluentAssertions;
using Moq;
using Portfolio.Api.Infrastructure.Persistence.Transactions;

namespace Portfolio.Api.RepositoryTests.Repositories
{
    public class PersistenceTransactionTests
    {
        [Fact]
        public async Task Commit_WhenTransactionIsActive_CommitsRepositoryTransaction()
        {
            // Arrange
            var repository = new Mock<ITransactionalRepository>();
            var transaction = new PersistenceTransaction(repository.Object);

            // Act
            await transaction.Commit();

            // Assert
            repository.Verify(value => value.CommitTransaction(), Times.Once);
            repository.Verify(value => value.RollbackTransaction(), Times.Never);
        }

        [Fact]
        public async Task Commit_WhenTransactionIsAlreadyCommitted_DoesNotCommitAgain()
        {
            // Arrange
            var repository = new Mock<ITransactionalRepository>();
            var transaction = new PersistenceTransaction(repository.Object);

            // Act
            await transaction.Commit();
            await transaction.Commit();

            // Assert
            repository.Verify(value => value.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task Commit_WhenTransactionIsDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var repository = new Mock<ITransactionalRepository>();
            var transaction = new PersistenceTransaction(repository.Object);

            await transaction.DisposeAsync();

            // Act
            Func<Task> action = transaction.Commit;

            // Assert
            await action.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsNotCommitted_RollsBackRepositoryTransaction()
        {
            // Arrange
            var repository = new Mock<ITransactionalRepository>();
            var transaction = new PersistenceTransaction(repository.Object);

            // Act
            await transaction.DisposeAsync();

            // Assert
            repository.Verify(value => value.RollbackTransaction(), Times.Once);
            repository.Verify(value => value.CommitTransaction(), Times.Never);
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsCommitted_DoesNotRollbackRepositoryTransaction()
        {
            // Arrange
            var repository = new Mock<ITransactionalRepository>();
            var transaction = new PersistenceTransaction(repository.Object);

            await transaction.Commit();

            // Act
            await transaction.DisposeAsync();

            // Assert
            repository.Verify(value => value.CommitTransaction(), Times.Once);
            repository.Verify(value => value.RollbackTransaction(), Times.Never);
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsAlreadyDisposed_DoesNotRollbackAgain()
        {
            // Arrange
            var repository = new Mock<ITransactionalRepository>();
            var transaction = new PersistenceTransaction(repository.Object);

            // Act
            await transaction.DisposeAsync();
            await transaction.DisposeAsync();

            // Assert
            repository.Verify(value => value.RollbackTransaction(), Times.Once);
        }
    }
}
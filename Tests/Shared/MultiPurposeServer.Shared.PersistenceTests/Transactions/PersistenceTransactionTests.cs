using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Transactions;

namespace MultiPurposeServer.Shared.PersistenceTests.Transactions
{
    public class PersistenceTransactionTests
    {
        [Fact]
        public async Task Commit_WhenTransactionIsActive_CommitsPersistence()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var transaction = new PersistenceTransaction(persistence.Object);

            // Act
            await transaction.Commit();

            // Assert
            persistence.Verify(value => value.CommitTransaction(), Times.Once);
            persistence.Verify(value => value.RollbackTransaction(), Times.Never);
        }

        [Fact]
        public async Task Commit_WhenTransactionIsAlreadyCommitted_DoesNotCommitAgain()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var transaction = new PersistenceTransaction(persistence.Object);

            // Act
            await transaction.Commit();
            await transaction.Commit();

            // Assert
            persistence.Verify(value => value.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task Commit_WhenPersistenceCommitFails_DoesNotRollbackOnDisposeAndPreservesException()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var expectedException = new InvalidOperationException("Commit failed.");
            var transaction = new PersistenceTransaction(persistence.Object);

            persistence.Setup(value => value.CommitTransaction()).ThrowsAsync(expectedException);

            // Act
            var action = async () =>
            {
                await using (transaction)
                {
                    await transaction.Commit();
                }
            };

            // Assert
            var exception = await action.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Should().BeSameAs(expectedException);
            persistence.Verify(value => value.CommitTransaction(), Times.Once);
            persistence.Verify(value => value.RollbackTransaction(), Times.Never);
        }

        [Fact]
        public async Task Commit_WhenTransactionIsDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var transaction = new PersistenceTransaction(persistence.Object);

            await transaction.DisposeAsync();

            // Act
            Func<Task> action = transaction.Commit;

            // Assert
            await action.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsNotCommitted_RollsBackPersistenceTransaction()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var transaction = new PersistenceTransaction(persistence.Object);

            // Act
            await transaction.DisposeAsync();

            // Assert
            persistence.Verify(value => value.RollbackTransaction(), Times.Once);
            persistence.Verify(value => value.CommitTransaction(), Times.Never);
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsCommitted_DoesNotRollbackPersistenceTransaction()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var transaction = new PersistenceTransaction(persistence.Object);

            await transaction.Commit();

            // Act
            await transaction.DisposeAsync();

            // Assert
            persistence.Verify(value => value.CommitTransaction(), Times.Once);
            persistence.Verify(value => value.RollbackTransaction(), Times.Never);
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsAlreadyDisposed_DoesNotRollbackAgain()
        {
            // Arrange
            var persistence = new Mock<ITransactionalPersistence>();
            var transaction = new PersistenceTransaction(persistence.Object);

            // Act
            await transaction.DisposeAsync();
            await transaction.DisposeAsync();

            // Assert
            persistence.Verify(value => value.RollbackTransaction(), Times.Once);
        }
    }
}

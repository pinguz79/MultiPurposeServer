using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Operations;
using MultiPurposeServer.Shared.Persistence.Transactions;

namespace MultiPurposeServer.Shared.PersistenceTests.Operations
{
    public class ApplicationOperationTests
    {
        [Fact]
        public async Task BeginCheckpoint_WhenOperationIsActive_BeginsPersistenceCheckpoint()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var persistenceCheckpoint = new Mock<IPersistenceCheckpoint>();
            transaction.Setup(value => value.BeginCheckpoint()).ReturnsAsync(persistenceCheckpoint.Object);
            var operation = new ApplicationOperation(transaction.Object);

            // Act
            await using IApplicationOperationCheckpoint checkpoint = await operation.BeginCheckpoint();
            await checkpoint.Complete();

            // Assert
            transaction.Verify(value => value.BeginCheckpoint(), Times.Once);
            persistenceCheckpoint.Verify(value => value.Complete(), Times.Once);
        }

        [Fact]
        public async Task Complete_WhenOperationIsActive_CommitsPersistenceTransaction()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var operation = new ApplicationOperation(transaction.Object);

            // Act
            await operation.Complete();

            // Assert
            transaction.Verify(value => value.Commit(), Times.Once);
        }

        [Fact]
        public async Task Complete_WhenOperationIsAlreadyCompleted_DoesNotCommitAgain()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var operation = new ApplicationOperation(transaction.Object);

            // Act
            await operation.Complete();
            await operation.Complete();

            // Assert
            transaction.Verify(value => value.Commit(), Times.Once);
        }

        [Fact]
        public async Task Complete_WhenOperationIsDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var operation = new ApplicationOperation(transaction.Object);

            await operation.DisposeAsync();

            // Act
            Func<Task> action = operation.Complete;

            // Assert
            await action.Should().ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task Dispose_WhenOperationIsNotCompleted_DisposesPersistenceTransaction()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var operation = new ApplicationOperation(transaction.Object);

            // Act
            await operation.DisposeAsync();

            // Assert
            transaction.Verify(value => value.DisposeAsync(), Times.Once);
            transaction.Verify(value => value.Commit(), Times.Never);
        }

        [Fact]
        public async Task Dispose_WhenOperationIsCompleted_DisposesPersistenceTransaction()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var operation = new ApplicationOperation(transaction.Object);

            await operation.Complete();

            // Act
            await operation.DisposeAsync();

            // Assert
            transaction.Verify(value => value.Commit(), Times.Once);
            transaction.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Dispose_WhenOperationIsAlreadyDisposed_DoesNotDisposeAgain()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();
            var operation = new ApplicationOperation(transaction.Object);

            // Act
            await operation.DisposeAsync();
            await operation.DisposeAsync();

            // Assert
            transaction.Verify(value => value.DisposeAsync(), Times.Once);
        }
    }
}

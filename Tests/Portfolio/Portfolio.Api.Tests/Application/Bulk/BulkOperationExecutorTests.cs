using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Contracts.Responses;

using Portfolio.Api.Application.Bulk;
using Portfolio.Api.Application.Operations;

namespace Portfolio.Api.Tests.Application.Bulk
{
    public class BulkOperationExecutorTests
    {
        private static readonly BulkError PersistenceError = new(BulkErrorKind.Persistence, "PersistenceError", "Persistence failed.");

        #region AllOrNothing

        [Fact]
        public async Task Execute_AllOrNothingEvaluateAll_WhenAnItemFails_EvaluatesAllAndPersistsNone()
        {
            // Arrange
            TestBulkItem[] items = [new(Guid.NewGuid(), "One"), new(Guid.NewGuid(), "Two"), new(Guid.NewGuid(), "Three")];
            var operation = new Mock<IApplicationOperation>();
            Mock<IApplicationOperationCheckpoint> firstCheckpoint = CreateCheckpoint();
            Mock<IApplicationOperationCheckpoint> failedCheckpoint = CreateCheckpoint();
            Mock<IApplicationOperationCheckpoint> thirdCheckpoint = CreateCheckpoint();
            failedCheckpoint.Setup(checkpoint => checkpoint.Complete()).ThrowsAsync(new InvalidOperationException());
            operation.SetupSequence(value => value.BeginCheckpoint())
                .ReturnsAsync(firstCheckpoint.Object)
                .ReturnsAsync(failedCheckpoint.Object)
                .ReturnsAsync(thirdCheckpoint.Object);
            var options = new BulkOptions(BulkPersistenceStrategy.AllOrNothing, BulkEvaluationStrategy.EvaluateAll);

            // Act
            BulkResponse<Guid, string> response = await BulkOperationExecutor.Execute(
                items,
                options,
                item => item.Id,
                item => Task.FromResult(item.Value!),
                () => Task.FromResult(operation.Object),
                MapPersistenceError);

            // Assert
            response.Outcome.Should().Be(BulkOutcome.Failed);
            response.Items.Select(item => item.Outcome).Should().Equal(BulkItemOutcome.Succeeded, BulkItemOutcome.Failed, BulkItemOutcome.Succeeded);
            response.Items.Should().OnlyContain(item => !item.Persisted);
            operation.Verify(value => value.Complete(), Times.Never);
            firstCheckpoint.Verify(value => value.Complete(), Times.Once);
            failedCheckpoint.Verify(value => value.DisposeAsync(), Times.Once);
            thirdCheckpoint.Verify(value => value.Complete(), Times.Once);
        }

        [Fact]
        public async Task Execute_AllOrNothingStopOnFirstFailure_WhenAnItemFails_StopsAndMarksRemainingItems()
        {
            // Arrange
            TestBulkItem[] items = [new(Guid.NewGuid(), "One"), new(Guid.NewGuid(), "Two"), new(Guid.NewGuid(), "Three")];
            var operation = new Mock<IApplicationOperation>();
            Mock<IApplicationOperationCheckpoint> firstCheckpoint = CreateCheckpoint();
            Mock<IApplicationOperationCheckpoint> failedCheckpoint = CreateCheckpoint();
            failedCheckpoint.Setup(checkpoint => checkpoint.Complete()).ThrowsAsync(new InvalidOperationException());
            operation.SetupSequence(value => value.BeginCheckpoint())
                .ReturnsAsync(firstCheckpoint.Object)
                .ReturnsAsync(failedCheckpoint.Object);
            var options = new BulkOptions(BulkPersistenceStrategy.AllOrNothing, BulkEvaluationStrategy.StopOnFirstFailure);

            // Act
            BulkResponse<Guid, string> response = await BulkOperationExecutor.Execute(
                items,
                options,
                item => item.Id,
                item => Task.FromResult(item.Value!),
                () => Task.FromResult(operation.Object),
                MapPersistenceError);

            // Assert
            response.Outcome.Should().Be(BulkOutcome.Failed);
            response.Items.Select(item => item.Outcome).Should().Equal(BulkItemOutcome.Succeeded, BulkItemOutcome.Failed, BulkItemOutcome.NotProcessed);
            response.Items.Should().OnlyContain(item => !item.Persisted);
            operation.Verify(value => value.BeginCheckpoint(), Times.Exactly(2));
            operation.Verify(value => value.Complete(), Times.Never);
        }

        #endregion

        #region PartialSuccess

        [Fact]
        public async Task Execute_PartialSuccessEvaluateAll_WhenAnItemFails_EvaluatesAllAndPersistsSuccessfulItems()
        {
            // Arrange
            TestBulkItem[] items = [new(Guid.NewGuid(), "One"), new(Guid.NewGuid(), "Two"), new(Guid.NewGuid(), "Three")];
            Mock<IApplicationOperation> firstOperation = CreateOperation();
            Mock<IApplicationOperation> failedOperation = CreateOperation();
            Mock<IApplicationOperation> thirdOperation = CreateOperation();
            failedOperation.Setup(operation => operation.Complete()).ThrowsAsync(new InvalidOperationException());
            var operations = new Queue<IApplicationOperation>([firstOperation.Object, failedOperation.Object, thirdOperation.Object]);
            var options = new BulkOptions(BulkPersistenceStrategy.PartialSuccess, BulkEvaluationStrategy.EvaluateAll);

            // Act
            BulkResponse<Guid, string> response = await BulkOperationExecutor.Execute(
                items,
                options,
                item => item.Id,
                item => Task.FromResult(item.Value!),
                () => Task.FromResult(operations.Dequeue()),
                MapPersistenceError);

            // Assert
            response.Outcome.Should().Be(BulkOutcome.PartiallySucceeded);
            response.Items.Select(item => item.Outcome).Should().Equal(BulkItemOutcome.Succeeded, BulkItemOutcome.Failed, BulkItemOutcome.Succeeded);
            response.Items.Select(item => item.Persisted).Should().Equal(true, false, true);
            firstOperation.Verify(operation => operation.Complete(), Times.Once);
            failedOperation.Verify(operation => operation.DisposeAsync(), Times.Once);
            thirdOperation.Verify(operation => operation.Complete(), Times.Once);
        }

        [Fact]
        public async Task Execute_PartialSuccessStopOnFirstFailure_WhenValidationFails_DoesNotBeginOperationsAndMarksRemainingItems()
        {
            // Arrange
            TestBulkItem[] items = [new(Guid.NewGuid(), null), new(Guid.NewGuid(), "Two"), new(Guid.NewGuid(), "Three")];
            var beginOperationCalls = 0;
            var options = new BulkOptions(BulkPersistenceStrategy.PartialSuccess, BulkEvaluationStrategy.StopOnFirstFailure);

            // Act
            BulkResponse<Guid, string> response = await BulkOperationExecutor.Execute(
                items,
                options,
                item => item.Id,
                item => Task.FromResult(item.Value!),
                () =>
                {
                    beginOperationCalls++;
                    return Task.FromResult(CreateOperation().Object);
                },
                MapPersistenceError);

            // Assert
            response.Outcome.Should().Be(BulkOutcome.Failed);
            response.Items.Select(item => item.Outcome).Should().Equal(BulkItemOutcome.Failed, BulkItemOutcome.NotProcessed, BulkItemOutcome.NotProcessed);
            response.Items.First().Errors.Should().ContainSingle(error => error.Kind == BulkErrorKind.Validation);
            beginOperationCalls.Should().Be(0);
        }

        #endregion

        private static Mock<IApplicationOperationCheckpoint> CreateCheckpoint()
        {
            var checkpoint = new Mock<IApplicationOperationCheckpoint>();
            checkpoint.Setup(value => value.DisposeAsync()).Returns(ValueTask.CompletedTask);

            return checkpoint;
        }

        private static Mock<IApplicationOperation> CreateOperation()
        {
            var operation = new Mock<IApplicationOperation>();
            operation.Setup(value => value.DisposeAsync()).Returns(ValueTask.CompletedTask);

            return operation;
        }

        private static BulkError? MapPersistenceError(Exception exception) => exception is InvalidOperationException ? PersistenceError : null;
    }
}

using FluentAssertions;

using MultiPurposeServer.Shared.Logging.Models;
using MultiPurposeServer.Shared.Logging.Services;

namespace MultiPurposeServer.Shared.LoggingTests
{
    public sealed class LoggingContextAccessorTests
    {
        [Fact]
        public void Push_WhenScopeIsDisposed_RestoresPreviousContext()
        {
            // Arrange
            var accessor = new LoggingContextAccessor();
            var outerContext = new LoggingContext("Portfolio", "correlation-1");
            var innerContext = new LoggingContext("SampleApp", "correlation-2");

            // Act
            using (accessor.Push(outerContext))
            {
                using (accessor.Push(innerContext))
                {
                    accessor.Current.Should().Be(innerContext);
                }

                accessor.Current.Should().Be(outerContext);
            }

            // Assert
            accessor.Current.Should().Be(LoggingContext.Empty);
        }
    }
}

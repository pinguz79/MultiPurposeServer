using FluentAssertions;

using Microsoft.Extensions.Logging;

using MultiPurposeServer.Shared.Logging.Models;
using MultiPurposeServer.Shared.Logging.Services;

namespace MultiPurposeServer.Shared.LoggingTests
{
    public sealed class DiagnosticStateRegistryTests
    {
        [Fact]
        public void Get_WhenDomainHasNoState_ReturnsOff()
        {
            // Arrange
            var registry = CreateRegistry();

            // Act
            var result = registry.Get("Portfolio");

            // Assert
            result.Should().BeEquivalentTo(DiagnosticState.Off("Portfolio"));
        }

        [Fact]
        public void Enable_WhenDiagnosticDurationIsValid_StoresState()
        {
            // Arrange
            var timeProvider = new TestTimeProvider();
            var registry = CreateRegistry(timeProvider);

            // Act
            registry.Enable("Portfolio", DiagnosticMode.Diagnostic, TimeSpan.FromMinutes(30));
            var result = registry.Get("Portfolio");

            // Assert
            result.Mode.Should().Be(DiagnosticMode.Diagnostic);
            result.ExpiresAt.Should().Be(timeProvider.GetUtcNow().AddMinutes(30));
        }

        [Fact]
        public void Get_WhenStateHasExpired_ReturnsOff()
        {
            // Arrange
            var timeProvider = new TestTimeProvider();
            var registry = CreateRegistry(timeProvider);
            registry.Enable("Portfolio", DiagnosticMode.Verbose, TimeSpan.FromMinutes(10));
            timeProvider.Advance(TimeSpan.FromMinutes(11));

            // Act
            var result = registry.Get("Portfolio");

            // Assert
            result.Should().BeEquivalentTo(DiagnosticState.Off("Portfolio"));
        }

        [Fact]
        public void Get_WhenStateHasExpired_LogsExpiration()
        {
            // Arrange
            var timeProvider = new TestTimeProvider();
            var logger = new TestLogger<DiagnosticStateRegistry>();
            var registry = CreateRegistry(timeProvider, logger);
            registry.Enable("Portfolio", DiagnosticMode.Diagnostic, TimeSpan.FromMinutes(30));
            timeProvider.Advance(TimeSpan.FromMinutes(31));

            // Act
            registry.Get("Portfolio");

            // Assert
            var entry = logger.Entries.Should().ContainSingle().Subject;
            entry.Level.Should().Be(LogLevel.Information);
            entry.EventId.Name.Should().Be("Shared.Logging.DiagnosticsExpired");
            entry.Message.Should().Contain("Portfolio");
        }

        [Fact]
        public void Enable_WhenVerboseDurationExceedsLimit_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var registry = CreateRegistry();

            // Act
            var action = () => registry.Enable("Portfolio", DiagnosticMode.Verbose, TimeSpan.FromMinutes(16));

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static DiagnosticStateRegistry CreateRegistry(
            TestTimeProvider? timeProvider = null,
            TestLogger<DiagnosticStateRegistry>? logger = null) => new(
            new DiagnosticOptions(),
            timeProvider ?? new TestTimeProvider(),
            logger ?? new TestLogger<DiagnosticStateRegistry>());
    }
}

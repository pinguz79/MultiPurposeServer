using FluentAssertions;

using Microsoft.Extensions.Logging;

using MultiPurposeServer.Shared.Logging.Models;
using MultiPurposeServer.Shared.Logging.Services;

namespace MultiPurposeServer.Shared.LoggingTests
{
    public sealed class LoggerServiceTests
    {
        [Fact]
        public void Debug_WhenDiagnosticModeIsOff_DoesNotWriteEvent()
        {
            // Arrange
            var contextAccessor = CreateContextAccessor();
            var logger = new TestLogger<LoggerServiceTests>();
            var service = new LoggerService<LoggerServiceTests>(logger, contextAccessor, CreateRegistry());

            // Act
            service.Debug(new LogEventId("Portfolio.Test.Debug"), "Evento diagnostico");

            // Assert
            logger.Entries.Should().BeEmpty();
        }

        [Fact]
        public void Debug_WhenDiagnosticModeIsEnabled_WritesInformationWithOriginalLevel()
        {
            // Arrange
            var contextAccessor = CreateContextAccessor();
            var registry = CreateRegistry();
            registry.Enable("Portfolio", DiagnosticMode.Diagnostic, TimeSpan.FromMinutes(10));
            var logger = new TestLogger<LoggerServiceTests>();
            var service = new LoggerService<LoggerServiceTests>(logger, contextAccessor, registry);

            // Act
            service.Debug(new LogEventId("Portfolio.Test.Debug"), "Evento diagnostico {ItemId}", 42);

            // Assert
            var entry = logger.Entries.Should().ContainSingle().Subject;
            entry.Level.Should().Be(LogLevel.Information);
            entry.EventId.Name.Should().Be("Portfolio.Test.Debug");
            entry.Message.Should().Be("Evento diagnostico 42");
            entry.Scope.Should().Contain("OriginalLevel", "Debug").And.Contain("Domain", "Portfolio").And.Contain("IsDiagnostic", true);
        }

        [Fact]
        public void Trace_WhenDiagnosticModeIsEnabled_DoesNotWriteEvent()
        {
            // Arrange
            var contextAccessor = CreateContextAccessor();
            var registry = CreateRegistry();
            registry.Enable("Portfolio", DiagnosticMode.Diagnostic, TimeSpan.FromMinutes(10));
            var logger = new TestLogger<LoggerServiceTests>();
            var service = new LoggerService<LoggerServiceTests>(logger, contextAccessor, registry);

            // Act
            service.Trace(new LogEventId("Portfolio.Test.Trace"), "Evento dettagliato");

            // Assert
            logger.Entries.Should().BeEmpty();
        }

        [Fact]
        public void Trace_WhenVerboseModeIsEnabled_WritesInformationWithOriginalLevel()
        {
            // Arrange
            var contextAccessor = CreateContextAccessor();
            var registry = CreateRegistry();
            registry.Enable("Portfolio", DiagnosticMode.Verbose, TimeSpan.FromMinutes(10));
            var logger = new TestLogger<LoggerServiceTests>();
            var service = new LoggerService<LoggerServiceTests>(logger, contextAccessor, registry);

            // Act
            service.Trace(new LogEventId("Portfolio.Test.Trace"), "Evento dettagliato");

            // Assert
            var entry = logger.Entries.Should().ContainSingle().Subject;
            entry.Level.Should().Be(LogLevel.Information);
            entry.Scope.Should().Contain("OriginalLevel", "Trace").And.Contain("DiagnosticMode", "Verbose");
        }

        [Fact]
        public void Information_WhenDiagnosticModeIsOff_WritesOriginalEventAndContext()
        {
            // Arrange
            var contextAccessor = CreateContextAccessor();
            var logger = new TestLogger<LoggerServiceTests>();
            var service = new LoggerService<LoggerServiceTests>(logger, contextAccessor, CreateRegistry());

            // Act
            service.Information(new LogEventId("Portfolio.Test.Completed"), "Operazione completata");

            // Assert
            var entry = logger.Entries.Should().ContainSingle().Subject;
            entry.Level.Should().Be(LogLevel.Information);
            entry.Scope.Should().Contain("CorrelationId", "correlation-id").And.Contain("RequestId", "request-id").And.Contain("IsDiagnostic", false);
        }

        private static DiagnosticStateRegistry CreateRegistry() => new(new DiagnosticOptions(), new TestTimeProvider());

        private static LoggingContextAccessor CreateContextAccessor()
        {
            var accessor = new LoggingContextAccessor();
            accessor.Push(new LoggingContext("Portfolio", "correlation-id", "request-id"));
            return accessor;
        }
    }
}

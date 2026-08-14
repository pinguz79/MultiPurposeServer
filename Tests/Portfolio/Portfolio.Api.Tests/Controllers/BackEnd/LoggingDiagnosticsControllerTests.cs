using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Models;
using MultiPurposeServer.Shared.Logging.Services;

using Portfolio.Api.Application.Diagnostics;
using Portfolio.Api.Controllers.BackEnd;
using Portfolio.Contracts.Enums;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Tests.Controllers.BackEnd
{
    public sealed class LoggingDiagnosticsControllerTests
    {
        private readonly DiagnosticOptions _options = new();
        private readonly Mock<ILoggerService<LoggingDiagnosticsController>> _logger = new();
        private readonly LoggingDiagnosticsController _controller;

        public LoggingDiagnosticsControllerTests()
        {
            var registry = new DiagnosticStateRegistry(
                _options,
                TimeProvider.System,
                Mock.Of<Microsoft.Extensions.Logging.ILogger<DiagnosticStateRegistry>>());

            _controller = new LoggingDiagnosticsController(registry, _options, _logger.Object);
        }

        [Fact]
        public void Get_WhenDiagnosticsAreDisabled_ReturnsOffState()
        {
            // Arrange

            // Act
            var result = _controller.Get();

            // Assert
            var state = GetState(result);
            state.Domain.Should().Be("Portfolio");
            state.Mode.Should().Be(nameof(DiagnosticMode.Off));
            state.ExpiresAt.Should().BeNull();
        }

        [Fact]
        public void Enable_WhenDiagnosticRequestIsValid_EnablesDiagnosticsAndLogsEvent()
        {
            // Arrange
            var request = new EnableLoggingDiagnosticRequest(LoggingDiagnosticMode.Diagnostic, 30);

            // Act
            var result = _controller.Enable(request);

            // Assert
            var state = GetState(result);
            state.Mode.Should().Be(nameof(DiagnosticMode.Diagnostic));
            state.ExpiresAt.Should().NotBeNull();
            _logger.Verify(logger => logger.Information(
                PortfolioLogEvents.LoggingDiagnosticsEnabled,
                It.IsAny<string>(),
                It.IsAny<object?[]>()), Times.Once);
        }

        [Fact]
        public void Enable_WhenDurationExceedsModeLimit_ReturnsBadRequestWithoutChangingState()
        {
            // Arrange
            var request = new EnableLoggingDiagnosticRequest(LoggingDiagnosticMode.Verbose, 16);

            // Act
            var result = _controller.Enable(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            GetState(_controller.Get()).Mode.Should().Be(nameof(DiagnosticMode.Off));
            _logger.VerifyNoOtherCalls();
        }

        [Fact]
        public void Disable_WhenDiagnosticsAreEnabled_DisablesDiagnosticsAndLogsEvent()
        {
            // Arrange
            _controller.Enable(new EnableLoggingDiagnosticRequest(LoggingDiagnosticMode.Diagnostic, 30));
            _logger.Invocations.Clear();

            // Act
            var result = _controller.Disable();

            // Assert
            var state = GetState(result);
            state.Mode.Should().Be(nameof(DiagnosticMode.Off));
            state.ExpiresAt.Should().BeNull();
            _logger.Verify(logger => logger.Information(
                PortfolioLogEvents.LoggingDiagnosticsDisabled,
                It.IsAny<string>(),
                It.IsAny<object?[]>()), Times.Once);
        }

        private static LoggingDiagnosticStateDto GetState(ActionResult<LoggingDiagnosticStateDto> result) =>
            result.Result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().BeOfType<LoggingDiagnosticStateDto>().Subject;
    }
}

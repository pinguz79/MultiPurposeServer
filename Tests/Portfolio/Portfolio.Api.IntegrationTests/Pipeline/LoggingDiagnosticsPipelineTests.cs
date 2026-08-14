using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Portfolio.Api.IntegrationTests.Infrastructure;
using Portfolio.Contracts.Enums;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.IntegrationTests.Pipeline
{
    public sealed class LoggingDiagnosticsPipelineTests
    {
        private const string Endpoint = "/Portfolio/BackEnd/Diagnostics/Logging";

        [Fact]
        public async Task ManageDiagnostics_WhenRequestsAreValid_UpdatesAndDisablesRuntimeState()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new EnableLoggingDiagnosticRequest(LoggingDiagnosticMode.Diagnostic, 30);

            // Act
            var enableResponse = await host.Client.PutAsJsonAsync(Endpoint, request);
            var enabledState = await enableResponse.Content.ReadFromJsonAsync<LoggingDiagnosticStateDto>();
            var getResponse = await host.Client.GetAsync(Endpoint);
            var currentState = await getResponse.Content.ReadFromJsonAsync<LoggingDiagnosticStateDto>();
            var disableResponse = await host.Client.DeleteAsync(Endpoint);
            var disabledState = await disableResponse.Content.ReadFromJsonAsync<LoggingDiagnosticStateDto>();

            // Assert
            enableResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            enabledState.Should().NotBeNull();
            enabledState.Mode.Should().Be("Diagnostic");
            enabledState.ExpiresAt.Should().NotBeNull();
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            currentState.Should().BeEquivalentTo(enabledState);
            disableResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            disabledState.Should().NotBeNull();
            disabledState.Mode.Should().Be("Off");
            disabledState.ExpiresAt.Should().BeNull();
        }

        [Fact]
        public async Task EnableDiagnostics_WhenDurationExceedsLimit_ReturnsBadRequest()
        {
            // Arrange
            await using var host = new PortfolioApiTestHost();
            var request = new EnableLoggingDiagnosticRequest(LoggingDiagnosticMode.Verbose, 16);

            // Act
            var response = await host.Client.PutAsJsonAsync(Endpoint, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}

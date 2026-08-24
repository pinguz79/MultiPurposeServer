using System.Net;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Services
{
    public class FinanceApiClientTests
    {
        [Fact]
        public async Task CreateContoSendsInitialBalanceInEuros()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            using var httpClient = new HttpClient(handler);
            var configuration = new ApiConfiguration
            {
                BaseUrl = "https://localhost/",
                HeaderName = "X-Finance-Api-Key",
                ApiKey = "test-key",
            };
            var client = new FinanceApiClient(httpClient, configuration);
            var request = new CreateConto("HelloBank", "Hello Bank", 3081.69m);

            // Act
            await client.CreateConto(request);
            using var payload = JsonDocument.Parse(handler.RequestContent!);

            // Assert
            handler.Request.Should().NotBeNull();
            handler.Request!.Method.Should().Be(HttpMethod.Post);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/Conto/Create"));
            handler.Request.Headers.GetValues(configuration.HeaderName).Should().ContainSingle().Which.Should().Be(configuration.ApiKey);
            payload.RootElement.GetProperty("initialBalance").GetDecimal().Should().Be(3081.69m);
        }
    }
}

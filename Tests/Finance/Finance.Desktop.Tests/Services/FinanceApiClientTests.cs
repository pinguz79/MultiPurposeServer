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
        public async Task GetContiUsesListRoute()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = "[]",
            };
            using var httpClient = new HttpClient(handler);
            var configuration = new ApiConfiguration
            {
                BaseUrl = "https://localhost/",
                HeaderName = "X-Finance-Api-Key",
                ApiKey = "test-key",
            };
            var client = new FinanceApiClient(httpClient, configuration);

            // Act
            var result = await client.GetConti();

            // Assert
            result.Should().BeEmpty();
            handler.Request.Should().NotBeNull();
            handler.Request!.Method.Should().Be(HttpMethod.Get);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/FrontEnd/Conto/List"));
            handler.Request.Headers.GetValues(configuration.HeaderName).Should().ContainSingle().Which.Should().Be(configuration.ApiKey);
        }

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
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/Conto"));
            handler.Request.Headers.GetValues(configuration.HeaderName).Should().ContainSingle().Which.Should().Be(configuration.ApiKey);
            payload.RootElement.GetProperty("initialBalance").GetDecimal().Should().Be(3081.69m);
        }

        [Fact]
        public async Task GetMovimentiUsesLogicalContoNameAndSelectedMonth()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = "{\"conto\":{\"id\":\"00000000-0000-0000-0000-000000000001\",\"name\":\"AmericanExpress\",\"displayName\":\"American Express\",\"balance\":0},\"selectedMonth\":8,\"selectedYear\":2026,\"from\":\"2026-07-01\",\"to\":\"2026-09-30\",\"openingBalance\":0,\"closingBalance\":0,\"items\":[]}",
            };
            using var httpClient = new HttpClient(handler);
            var configuration = new ApiConfiguration
            {
                BaseUrl = "https://localhost/",
                HeaderName = "X-Finance-Api-Key",
                ApiKey = "test-key",
            };
            var client = new FinanceApiClient(httpClient, configuration);

            // Act
            ContoMovimenti result = await client.GetMovimenti("AmericanExpress", 8, 2026);

            // Assert
            result.SelectedMonth.Should().Be(8);
            result.SelectedYear.Should().Be(2026);
            handler.Request!.RequestUri.Should().Be(new Uri("https://localhost/Finance/FrontEnd/Conto/AmericanExpress/Movimento/List?month=8&year=2026"));
        }

        [Fact]
        public async Task GetVociRicorrentiUsesBackEndListRoute()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = "[]",
            };
            using var httpClient = new HttpClient(handler);
            var configuration = new ApiConfiguration
            {
                BaseUrl = "https://localhost/",
                HeaderName = "X-Finance-Api-Key",
                ApiKey = "test-key",
            };
            var client = new FinanceApiClient(httpClient, configuration);

            // Act
            var result = await client.GetVociRicorrenti();

            // Assert
            result.Should().BeEmpty();
            handler.Request!.Method.Should().Be(HttpMethod.Get);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/VoceRicorrente/List"));
        }
    }
}

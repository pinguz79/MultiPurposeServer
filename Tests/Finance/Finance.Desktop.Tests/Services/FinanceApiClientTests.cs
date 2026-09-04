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
        public async Task GetContiReadsFirstNegativeBalanceForecast()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = "[{\"id\":\"00000000-0000-0000-0000-000000000001\",\"name\":\"HelloBank\",\"displayName\":\"Hello Bank\",\"balance\":3000,\"firstNegativeBalanceDate\":\"2027-03-10\",\"firstNegativeBalance\":-500}]",
            };
            using var httpClient = new HttpClient(handler);
            var client = new FinanceApiClient(httpClient, new ApiConfiguration
            {
                BaseUrl = "https://localhost/",
                HeaderName = "X-Finance-Api-Key",
                ApiKey = "test-key",
            });

            // Act
            Conto result = (await client.GetConti()).Single();

            // Assert
            result.FirstNegativeBalanceDate.Should().Be(new DateOnly(2027, 3, 10));
            result.FirstNegativeBalance.Should().Be(-500m);
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
        public async Task GetCicliUsesLogicalContoNameAndClosingMonth()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = """
                    {
                      "conto": { "id": "00000000-0000-0000-0000-000000000001", "name": "HelloCard", "displayName": "Hello Card", "balance": 0 },
                      "selectedMonth": 8,
                      "selectedYear": 2026,
                      "from": "2026-06-22",
                      "to": "2026-09-21",
                      "openingBalance": 0,
                      "closingBalance": 0,
                      "cycles": []
                    }
                    """,
            };
            using var httpClient = new HttpClient(handler);
            var client = new FinanceApiClient(httpClient, CreateConfiguration());

            // Act
            ContoCicli result = await client.GetCicli("HelloCard", 8, 2026);

            // Assert
            result.SelectedMonth.Should().Be(8);
            result.SelectedYear.Should().Be(2026);
            handler.Request!.RequestUri.Should().Be(new Uri("https://localhost/Finance/FrontEnd/Conto/HelloCard/Ciclo/List?month=8&year=2026"));
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

        [Fact]
        public async Task GetCategorieUsesBackEndListRoute()
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
            var result = await client.GetCategorie();

            // Assert
            result.Should().BeEmpty();
            handler.Request!.Method.Should().Be(HttpMethod.Get);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/Categoria/List"));
        }

        [Fact]
        public async Task GetParametriContoUsesSelectedAccountRoute()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = "[]",
            };
            using var httpClient = new HttpClient(handler);
            var client = new FinanceApiClient(httpClient, CreateConfiguration());

            // Act
            IReadOnlyList<ParametroConto> result = await client.GetParametriConto("HelloCard");

            // Assert
            result.Should().BeEmpty();
            handler.Request!.Method.Should().Be(HttpMethod.Get);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/Conto/HelloCard/Parametro/List"));
        }

        [Fact]
        public async Task ConfigureCartaASaldoUsesAtomicBootstrapRoute()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.Created,
                ResponseContent = """
                    {
                      "contoName": "HelloCard",
                      "pianificazioneAddebitoId": "00000000-0000-0000-0000-000000000001",
                      "pianificazioneRipristinoId": "00000000-0000-0000-0000-000000000002",
                      "created": true
                    }
                    """,
            };
            using var httpClient = new HttpClient(handler);
            var client = new FinanceApiClient(httpClient, CreateConfiguration());
            var request = new ConfigureCartaASaldo(5_000m, 0.10m, 21, 5, 6, "HelloBank",
                new DateOnly(2026, 9, 4), new DateOnly(2036, 12, 31));

            // Act
            CartaASaldo result = await client.ConfigureCartaASaldo("HelloCard", request);

            // Assert
            result.Created.Should().BeTrue();
            handler.Request!.Method.Should().Be(HttpMethod.Post);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/Conto/HelloCard/Configurazione/CartaASaldo"));
        }

        [Fact]
        public async Task DeleteCategoriaReturnsUsageWhenConfirmationIsRequired()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.Conflict,
                ResponseContent = "{\"vociRicorrenti\":2,\"pianificazioni\":1,\"movimenti\":8,\"total\":11}",
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
            CategoriaUsage? result = await client.DeleteCategoria("Casa", false);

            // Assert
            result.Should().Be(new CategoriaUsage(2, 1, 8));
            handler.Request!.Method.Should().Be(HttpMethod.Delete);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/BackEnd/Categoria/Casa?confirmReferences=false"));
        }

        [Fact]
        public async Task PreviewPianificazioneUsesFrontEndPreviewRoute()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler
            {
                ResponseStatusCode = HttpStatusCode.OK,
                ResponseContent = "{\"formula\":\"[Affitto]\",\"description\":\"Affitto\",\"occurrenceCount\":0,\"isValid\":true,\"errors\":[],\"dependencies\":[],\"occurrences\":[]}",
            };
            using var httpClient = new HttpClient(handler);
            var configuration = new ApiConfiguration
            {
                BaseUrl = "https://localhost/",
                HeaderName = "X-Finance-Api-Key",
                ApiKey = "test-key",
            };
            var client = new FinanceApiClient(httpClient, configuration);
            var request = new CreatePianificazione(
                "HelloBank",
                "Affitto",
                "Affitto",
                "[Affitto]",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 12, 31),
                1,
                5,
                false);

            // Act
            PianificazionePreview result = await client.PreviewPianificazione(request);

            // Assert
            result.IsValid.Should().BeTrue();
            handler.Request!.Method.Should().Be(HttpMethod.Post);
            handler.Request.RequestUri.Should().Be(new Uri("https://localhost/Finance/FrontEnd/Pianificazione/Preview"));
        }

        private static ApiConfiguration CreateConfiguration() => new()
        {
            BaseUrl = "https://localhost/",
            HeaderName = "X-Finance-Api-Key",
            ApiKey = "test-key",
        };
    }
}

using System.Net;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Services
{
    public class FinanceStartupTests
    {
        private const string FormulaError = """
            {"movimentoId":"00000000-0000-0000-0000-000000000001","date":"2026-09-05","description":"Addebito HelloCard","formula":"[Missing]","errorCode":"FormulaEvaluationFailed","errorMessage":"Riferimento mancante"}
            """;

        [Fact]
        public async Task Initialize_WhenConsolidationSucceeds_PostsBeforeLoadingAccounts()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, "{\"consolidatedCount\":2}"));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);

            // Act
            IReadOnlyList<Conto> result = await client.Initialize();

            // Assert
            result.Should().BeEmpty();
            handler.Requests.Select(request => request.Method).Should().Equal(HttpMethod.Post, HttpMethod.Get);
            handler.Requests.Select(request => request.RequestUri!.AbsolutePath).Should().Equal("/Finance/BackEnd/Movimento/Consolida", "/Finance/FrontEnd/Conto/List");
            handler.Requests[0].Content.Should().BeNull();
            handler.Requests.Should().OnlyContain(request => request.Headers.Contains("X-Finance-Api-Key"));
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task Initialize_WhenConsolidationFails_DoesNotLoadAccounts(HttpStatusCode status)
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = status, ResponseContent = "{\"detail\":\"Consolidamento fallito\"}" };
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);

            // Act
            Func<Task> action = async () => await client.Initialize();

            // Assert
            await action.Should().ThrowAsync<FinanceApiException>().WithMessage("Consolidamento fallito");
            handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public async Task Initialize_WhenFormulaFails_ReportsMovementDetails()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.UnprocessableEntity, ResponseContent = FormulaError };
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);

            // Act
            Func<Task> action = async () => await client.Initialize();

            // Assert
            await action.Should().ThrowAsync<FinanceApiException>().WithMessage("*Addebito HelloCard*05/09/2026*00000000-0000-0000-0000-000000000001*[Missing]*Riferimento mancante*");
            handler.Requests.Should().ContainSingle();
        }

        [Fact]
        public async Task Initialize_WhenRetriedAfterFailure_ConsolidatesAgainBeforeLoadingAccounts()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.UnprocessableEntity, FormulaError));
            handler.Responses.Enqueue((HttpStatusCode.OK, "{\"consolidatedCount\":0}"));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);
            try
            {
                await client.Initialize();
            }
            catch (FinanceApiException)
            {
                // Il secondo tentativo rappresenta il comando Riprova dopo l'errore mostrato all'utente.
            }

            // Act
            IReadOnlyList<Conto> result = await client.Initialize();

            // Assert
            result.Should().BeEmpty();
            handler.Requests.Select(request => request.Method).Should().Equal(HttpMethod.Post, HttpMethod.Post, HttpMethod.Get);
        }

        private static FinanceApiClient CreateClient(HttpClient http) => new(http, new ApiConfiguration
        {
            BaseUrl = "https://localhost/",
            HeaderName = "X-Finance-Api-Key",
            ApiKey = "test-key",
        });
    }
}

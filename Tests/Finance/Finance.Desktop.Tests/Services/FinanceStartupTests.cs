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
        public async Task Initialize_LoadsAccountsWithoutWriting()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);

            // Act
            IReadOnlyList<Conto> result = await client.Initialize();

            // Assert
            result.Should().BeEmpty();
            handler.Requests.Select(request => request.Method).Should().Equal(HttpMethod.Get);
            handler.Requests.Select(request => request.RequestUri!.AbsolutePath).Should().Equal("/Finance/FrontEnd/Conto/List");
            handler.Requests[0].Content.Should().BeNull();
            handler.Requests.Should().OnlyContain(request => request.Headers.Contains("X-Finance-Api-Key"));
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task Initialize_WhenLoadingFails_ReportsFailure(HttpStatusCode status)
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = status, ResponseContent = "{\"detail\":\"Consolidamento fallito\"}" };
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);

            // Act
            Func<Task> action = async () => await client.Initialize();

            // Assert
            await action.Should().ThrowAsync<FinanceApiException>().WithMessage("Consolidamento fallito");
            handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Get);
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
        public async Task Initialize_WhenRetriedAfterFailure_OnlyReadsAccounts()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.UnprocessableEntity, FormulaError));
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
            handler.Requests.Select(request => request.Method).Should().Equal(HttpMethod.Get, HttpMethod.Get);
        }

        [Fact]
        public async Task ConfirmMovimenti_SendsOnlySelectedIdentifiers()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = "{}" };
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);
            Guid id = Guid.NewGuid();

            // Act
            await client.ConfirmMovimenti([id]);

            // Assert
            handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Post);
            handler.RequestContent.Should().Be($"{{\"ids\":[\"{id}\"]}}");
        }

        [Fact]
        public async Task SaveMovimento_WhenEditing_UsesPatchAndExplicitConfirmation()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = "{}" };
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);
            Guid id = Guid.NewGuid();

            // Act
            await client.SaveMovimento(id, new SaveMovimento("HelloBank", new DateOnly(2026, 10, 1), "Spesa", "-20.00", false, null));

            // Assert
            handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Patch);
            handler.RequestContent.Should().Contain("\"isConfirmed\":false").And.Contain("\"clearCategory\":true").And.Contain("2026-10-01");
        }

        [Fact]
        public async Task GetMovimentiForReview_UsesReadOnlyPendingQuery()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = "[]" };
            using var http = new HttpClient(handler);
            FinanceApiClient client = CreateClient(http);

            // Act
            await client.GetMovimentiForReview();

            // Assert
            handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Get);
            handler.Request!.RequestUri!.Query.Should().Be("?pendingOnly=true");
        }

        private static FinanceApiClient CreateClient(HttpClient http) => new(http, new ApiConfiguration
        {
            BaseUrl = "https://localhost/",
            HeaderName = "X-Finance-Api-Key",
            ApiKey = "test-key",
        });
    }
}

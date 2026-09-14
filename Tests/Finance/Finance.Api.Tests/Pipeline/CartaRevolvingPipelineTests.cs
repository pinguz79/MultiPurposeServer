using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Tests.Pipeline
{
    public class CartaRevolvingPipelineTests
    {
        private const string Route = "/Finance/BackEnd/Conto/AmEx/Configurazione/CartaRevolving";
        private static readonly ConfigureCartaRevolvingRequest Request = new(1600m, 0.1m, 0.1m, 72.32m, 0.12m, 2m, 70m, 6, 19, "HelloBank", new(2026, 9, 1), new(2026, 12, 31));

        [Fact]
        public async Task ConfigureCartaRevolving_WhenUnauthorized_DoesNotOpenOperation()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();

            // Act
            using HttpResponseMessage response = await host.Client.PostAsJsonAsync(Route, Request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            host.CartaRevolvingService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true, HttpStatusCode.Created)]
        [InlineData(false, HttpStatusCode.OK)]
        public async Task ConfigureCartaRevolving_WhenSuccessful_CommitsAndReturnsPlanIdentifiers(bool created, HttpStatusCode expectedStatus)
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var operation = new Mock<IApplicationOperation>(MockBehavior.Strict);
            operation.Setup(item => item.Complete()).Returns(Task.CompletedTask);
            operation.Setup(item => item.DisposeAsync()).Returns(ValueTask.CompletedTask);
            var dto = new CartaRevolvingDto("AmEx", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), created);
            host.CartaRevolvingService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            host.CartaRevolvingService.Setup(service => service.Configure("AmEx", Request)).ReturnsAsync(dto);

            // Act
            using HttpResponseMessage response = await host.Client.PostAsJsonAsync(Route, Request);

            // Assert
            response.StatusCode.Should().Be(expectedStatus);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("pianificazioneAddebitoId").GetGuid().Should().Be(dto.PianificazioneAddebitoId);
            body.GetProperty("pianificazioneRimborsoId").GetGuid().Should().Be(dto.PianificazioneRimborsoId);
            operation.Verify(item => item.Complete(), Times.Once);
            operation.Verify(item => item.DisposeAsync(), Times.Once);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task ConfigureCartaRevolving_WhenRejected_DisposesWithoutCommit(HttpStatusCode expectedStatus)
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var operation = new Mock<IApplicationOperation>(MockBehavior.Strict);
            operation.Setup(item => item.DisposeAsync()).Returns(ValueTask.CompletedTask);
            Exception failure = expectedStatus switch
            {
                HttpStatusCode.NotFound => new KeyNotFoundException("Conto assente."),
                HttpStatusCode.Conflict => new CartaRevolvingConflictException("Profilo incompatibile."),
                _ => new ArgumentException("Configurazione non valida."),
            };
            host.CartaRevolvingService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            host.CartaRevolvingService.Setup(service => service.Configure("AmEx", Request)).ThrowsAsync(failure);

            // Act
            using HttpResponseMessage response = await host.Client.PostAsJsonAsync(Route, Request);

            // Assert
            response.StatusCode.Should().Be(expectedStatus);
            operation.Verify(item => item.Complete(), Times.Never);
            operation.Verify(item => item.DisposeAsync(), Times.Once);
        }
    }
}

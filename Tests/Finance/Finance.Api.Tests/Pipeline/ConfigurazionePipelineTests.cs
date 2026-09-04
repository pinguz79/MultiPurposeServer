using System.Net;
using System.Net.Http.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class ConfigurazionePipelineTests
    {
        [Fact]
        public async Task ConfigureCartaASaldoReturnsCreatedForNewBootstrap()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var request = new ConfigureCartaASaldoRequest(
                5_000m,
                0.10m,
                21,
                5,
                6,
                "HelloBank",
                new DateOnly(2026, 9, 4),
                new DateOnly(2036, 12, 31));
            var result = new CartaASaldoDto("HelloCard", Guid.NewGuid(), Guid.NewGuid(), true);
            host.CartaASaldoService.Setup(service => service.Configure("HelloCard", request)).ReturnsAsync(result);

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync(
                "/Finance/BackEnd/Conto/HelloCard/Configurazione/CartaASaldo",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task ConfigureCartaASaldoReturnsConflictForIncompatibleBootstrap()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var request = new ConfigureCartaASaldoRequest(
                5_000m,
                0.10m,
                21,
                5,
                6,
                "HelloBank",
                new DateOnly(2026, 9, 4),
                new DateOnly(2036, 12, 31));
            host.CartaASaldoService.Setup(service => service.Configure("HelloCard", request))
                .ThrowsAsync(new CartaASaldoConflictException("Incompatible."));

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync(
                "/Finance/BackEnd/Conto/HelloCard/Configurazione/CartaASaldo",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}

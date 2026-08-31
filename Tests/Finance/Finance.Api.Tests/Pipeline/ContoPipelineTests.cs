using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class ContoPipelineTests
    {
        [Fact]
        public async Task CreateContoWithoutApiKeyReturnsUnauthorized()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            var request = new CreateContoRequest("AmericanExpress", "American Express", 0m);

            // Act
            var response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            host.ContoService.Verify(service => service.CreateConto(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task CreateContoWithApiKeyReturnsCreatedConfiguration()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 123.45m };
            var request = new CreateContoRequest("american express", "American Express", 123.45m);
            host.ContoService.Setup(service => service.CreateConto(request.Name, request.DisplayName, request.InitialBalance)).ReturnsAsync(conto);
            host.ContoService.Setup(service => service.GetBalance(conto)).ReturnsAsync(conto.InitialBalance);

            // Act
            var response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto", request);
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result.RootElement.GetProperty("id").GetGuid().Should().Be(conto.Id);
            result.RootElement.GetProperty("name").GetString().Should().Be(conto.Name);
            result.RootElement.GetProperty("displayName").GetString().Should().Be(conto.DisplayName);
            result.RootElement.GetProperty("initialBalance").GetDecimal().Should().Be(conto.InitialBalance);
            result.RootElement.GetProperty("balance").GetDecimal().Should().Be(conto.InitialBalance);
        }

        [Fact]
        public async Task GetContiWithApiKeyReturnsMappedConti()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 123.45m };
            host.ContoService.Setup(service => service.GetConti()).ReturnsAsync([conto]);
            host.ContoService.Setup(service => service.GetBalance(conto)).ReturnsAsync(conto.InitialBalance);

            // Act
            var response = await host.Client.GetAsync("/Finance/FrontEnd/Conto/List");
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.RootElement.GetArrayLength().Should().Be(1);
            result.RootElement[0].GetProperty("id").GetGuid().Should().Be(conto.Id);
            result.RootElement[0].GetProperty("name").GetString().Should().Be(conto.Name);
            result.RootElement[0].GetProperty("displayName").GetString().Should().Be(conto.DisplayName);
            result.RootElement[0].GetProperty("balance").GetDecimal().Should().Be(conto.InitialBalance);
        }

        [Fact]
        public async Task CreateDuplicateContoReturnsConflictBoundToName()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var request = new CreateContoRequest("AmericanExpress", "American Express", 0m);
            host.ContoService.Setup(service => service.CreateConto(request.Name, request.DisplayName, request.InitialBalance))
                .ThrowsAsync(new DuplicateNameException(request.Name));

            // Act
            var response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto", request);
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            problem.RootElement.GetProperty("field").GetString().Should().Be("Name");
        }

        [Fact]
        public async Task UpdateContoNameReturnsUpdatedConfiguration()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var contoId = Guid.NewGuid();
            var conto = new Conto { Id = contoId, Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 123.45m };
            var request = new UpdateContoRequest("American Express", null, null);
            host.ContoService.Setup(service => service.UpdateConto(contoId, request.Name, request.DisplayName, request.InitialBalance)).ReturnsAsync(conto);
            host.ContoService.Setup(service => service.GetBalance(conto)).ReturnsAsync(conto.InitialBalance);

            // Act
            var response = await host.Client.PatchAsJsonAsync($"/Finance/BackEnd/Conto/{contoId}", request);
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.RootElement.GetProperty("name").GetString().Should().Be(conto.Name);
        }

        [Fact]
        public async Task UpdateContoDuplicateNameReturnsConflictBoundToName()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var contoId = Guid.NewGuid();
            var request = new UpdateContoRequest("AmericanExpress", null, null);
            host.ContoService.Setup(service => service.UpdateConto(contoId, request.Name, request.DisplayName, request.InitialBalance))
                .ThrowsAsync(new DuplicateNameException("AmericanExpress"));

            // Act
            var response = await host.Client.PatchAsJsonAsync($"/Finance/BackEnd/Conto/{contoId}", request);
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            problem.RootElement.GetProperty("field").GetString().Should().Be("Name");
        }
    }
}

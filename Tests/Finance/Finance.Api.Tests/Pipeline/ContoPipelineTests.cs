using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
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
            var response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto/Create", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            host.ContoService.Verify(service => service.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task CreateContoWithApiKeyReturnsCreatedConfiguration()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express", InitialBalance = 123.45m };
            var request = new CreateContoRequest("american express", "American Express", 123.45m);
            host.ContoService.Setup(service => service.Create(request.Name, request.DisplayName, request.InitialBalance)).ReturnsAsync(conto);

            // Act
            var response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto/Create", request);
            var result = await response.Content.ReadFromJsonAsync<ContoConfigurationDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Should().BeEquivalentTo(new ContoConfigurationDto(conto.Id, conto.Name, conto.DisplayName, conto.InitialBalance, conto.Balance));
        }

        [Fact]
        public async Task CreateDuplicateContoReturnsConflictBoundToName()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var request = new CreateContoRequest("AmericanExpress", "American Express", 0m);
            host.ContoService.Setup(service => service.Create(request.Name, request.DisplayName, request.InitialBalance))
                .ThrowsAsync(new DuplicateNameException(request.Name));

            // Act
            var response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto/Create", request);
            using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            problem.RootElement.GetProperty("field").GetString().Should().Be("Name");
        }
    }
}

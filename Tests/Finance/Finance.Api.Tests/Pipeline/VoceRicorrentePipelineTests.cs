using System.Net;
using System.Net.Http.Json;

using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class VoceRicorrentePipelineTests
    {
        [Fact]
        public async Task CreateReturnsCreatedAggregate()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var definition = new VoceRicorrente { Id = Guid.NewGuid(), Name = "Affitto", DisplayName = "Affitto", Value = 615.75m, Index = 0 };
            host.VoceRicorrenteService.Setup(service => service.Create("affitto", It.IsAny<IReadOnlyList<VoceRicorrenteDefinitionRequest>>()))
                .ReturnsAsync([definition]);
            var request = new CreateVoceRicorrenteRequest("affitto", [new VoceRicorrenteDefinitionRequest(null, "Affitto", 615.75m, null, null)]);

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/VoceRicorrente", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            host.VoceRicorrenteService.Verify(service => service.Create("affitto", It.IsAny<IReadOnlyList<VoceRicorrenteDefinitionRequest>>()), Times.Once);
        }

        [Fact]
        public async Task GetMissingRecurringEntryReturnsNotFound()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            host.VoceRicorrenteService.Setup(service => service.GetByName("Missing")).ReturnsAsync([]);

            // Act
            HttpResponseMessage response = await host.Client.GetAsync("/Finance/BackEnd/VoceRicorrente/Missing");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

using System.Net;
using System.Net.Http.Json;

using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class ParametroContoPipelineTests
    {
        [Fact]
        public async Task CreateReturnsCreatedAggregate()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var definition = new ParametroConto
            {
                Id = Guid.NewGuid(),
                Name = "Plafond",
                DisplayName = "Plafond",
                Type = TipoParametroConto.Importo,
                Value = 5_000m,
                Index = 0,
            };
            host.ParametroContoService.Setup(service => service.Create("HelloCard", "plafond", TipoParametroConto.Importo,
                    It.IsAny<IReadOnlyList<ParametroContoDefinitionRequest>>()))
                .ReturnsAsync([definition]);
            var request = new CreateParametroContoRequest("plafond", TipoParametroConto.Importo,
                [new ParametroContoDefinitionRequest(null, "Plafond", 5_000m, null, null)]);

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Conto/HelloCard/Parametro", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            host.ParametroContoService.Verify(service => service.Create("HelloCard", "plafond", TipoParametroConto.Importo,
                It.IsAny<IReadOnlyList<ParametroContoDefinitionRequest>>()), Times.Once);
        }

        [Fact]
        public async Task GetMissingParameterReturnsNotFound()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            host.ParametroContoService.Setup(service => service.GetByName("HelloCard", "Missing")).ReturnsAsync([]);

            // Act
            HttpResponseMessage response = await host.Client.GetAsync("/Finance/BackEnd/Conto/HelloCard/Parametro/Missing");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

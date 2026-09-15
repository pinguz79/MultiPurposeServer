using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Finance.Api.Tests.Infrastructure;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Tests.Pipeline
{
    public class MovimentoNaturaPipelineTests
    {
        [Theory]
        [InlineData(false, NaturaMovimento.Ordinario)]
        [InlineData(false, NaturaMovimento.Interessi)]
        [InlineData(true, NaturaMovimento.Ordinario)]
        [InlineData(true, NaturaMovimento.Rimborso)]
        public async Task Update_WhenOnlyNatureProvided_ForwardsAndReturnsNature(bool bulk, NaturaMovimento natura)
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var movement = new Movimento { Id = Guid.NewGuid(), Conto = new Conto { Name = "AmEx" }, Natura = natura };
            host.MovimentoService.Setup(service => service.Update(movement.Id, null, null, null, null, null, natura)).ReturnsAsync(movement);
            var operation = new Mock<IApplicationOperation>();
            operation.Setup(item => item.BeginCheckpoint()).ReturnsAsync(Mock.Of<IApplicationOperationCheckpoint>());
            host.MovimentoService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            object body = bulk ? new { options = new { persistenceStrategy = 0, evaluationStrategy = 0 }, items = new[] { new { id = movement.Id, natura } } } : new { natura };
            string route = bulk ? "/Finance/BackEnd/Bulk/Movimento/Update" : $"/Finance/BackEnd/Movimento/{movement.Id}";

            // Act
            using HttpResponseMessage response = await host.Client.PatchAsJsonAsync(route, body);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            host.MovimentoService.Verify(service => service.Update(movement.Id, null, null, null, null, null, natura), Times.Once);
            using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
            JsonElement value = bulk ? payload!.RootElement.GetProperty("items")[0].GetProperty("value") : payload!.RootElement;
            value.GetProperty("natura").GetInt32().Should().Be((int)natura);
        }
    }
}

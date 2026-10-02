using System.Net;
using System.Net.Http.Json;

using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Tests.Pipeline
{
    public class TrasferimentoPipelineTests
    {
        [Fact]
        public async Task UnauthorizedTransferCannotWrite()
        {
            await using var host = new FinanceApiTestHost();
            using HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Trasferimento", Request());
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            host.MovimentoService.VerifyNoOtherCalls();
            host.TrasferimentoService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateReturnsBothMovementsAndCommitsOnce()
        {
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var operation = new Mock<IApplicationOperation>();
            operation.Setup(item => item.Complete()).Returns(Task.CompletedTask);
            operation.Setup(item => item.DisposeAsync()).Returns(ValueTask.CompletedTask);
            host.MovimentoService.Setup(item => item.BeginOperation()).ReturnsAsync(operation.Object);
            var account = new Conto { Name = "Test" };
            host.TrasferimentoService.Setup(item => item.Create(It.IsAny<CreateTrasferimentoRequest>()))
                .ReturnsAsync([new Movimento { Id = Guid.NewGuid(), Conto = account }, new Movimento { Id = Guid.NewGuid(), Conto = account }]);

            using HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Trasferimento", Request());

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            operation.Verify(item => item.Complete(), Times.Once);
        }

        [Fact]
        public async Task MissingGroupReturnsNotFound()
        {
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            host.TrasferimentoService.Setup(item => item.ResolveGroup(It.IsAny<Guid>())).ThrowsAsync(new KeyNotFoundException());
            using HttpResponseMessage response = await host.Client.GetAsync($"/Finance/BackEnd/GruppoMovimenti/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static CreateTrasferimentoRequest Request() => new("Carta", "Banca", 500m, new DateOnly(2026, 10, 24), "Copertura");
    }
}

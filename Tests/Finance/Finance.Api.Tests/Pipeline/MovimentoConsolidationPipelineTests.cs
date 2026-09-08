using System.Net;
using System.Net.Http.Json;

using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Tests.Pipeline
{
    public class MovimentoConsolidationPipelineTests
    {
        [Fact]
        public async Task Consolidate_WhenUnauthorized_DoesNotOpenOperation()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();

            // Act
            using HttpResponseMessage response = await host.Client.PostAsync("/Finance/BackEnd/Movimento/Consolida", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            host.MovimentoService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Consolidate_WhenSuccessful_CompletesOperationAndReturnsCount()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var operation = new Mock<IApplicationOperation>(MockBehavior.Strict);
            operation.Setup(item => item.Complete()).Returns(Task.CompletedTask);
            operation.Setup(item => item.DisposeAsync()).Returns(ValueTask.CompletedTask);
            host.MovimentoService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            host.MovimentoService.Setup(service => service.Consolidate(today)).ReturnsAsync(3);

            // Act
            using HttpResponseMessage response = await host.Client.PostAsync("/Finance/BackEnd/Movimento/Consolida", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadFromJsonAsync<ConsolidamentoMovimentiDto>())!.ConsolidatedCount.Should().Be(3);
            operation.Verify(item => item.Complete(), Times.Once);
            operation.Verify(item => item.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Consolidate_WhenFormulaFails_DisposesWithoutCommitAndReturnsMovement()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var operation = new Mock<IApplicationOperation>(MockBehavior.Strict);
            operation.Setup(item => item.DisposeAsync()).Returns(ValueTask.CompletedTask);
            host.MovimentoService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            var movement = new Movimento { Id = Guid.NewGuid(), Date = DateOnly.FromDateTime(DateTime.Today).AddDays(-1), Description = "Errore", Formula = "[Missing]" };
            host.MovimentoService.Setup(service => service.Consolidate(It.IsAny<DateOnly>()))
                .ThrowsAsync(new FormulaEvaluationException(movement, new InvalidOperationException("Riferimento mancante.")));

            // Act
            using HttpResponseMessage response = await host.Client.PostAsync("/Finance/BackEnd/Movimento/Consolida", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            string content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(movement.Id.ToString()).And.Contain("FormulaEvaluationFailed");
            operation.Verify(item => item.Complete(), Times.Never);
            operation.Verify(item => item.DisposeAsync(), Times.Once);
        }
    }
}

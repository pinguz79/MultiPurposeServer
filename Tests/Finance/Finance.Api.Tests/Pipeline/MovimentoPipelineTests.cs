using System.Net;
using System.Net.Http.Json;

using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Bulk.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Contracts;

namespace Finance.Api.Tests.Pipeline
{
    public class MovimentoPipelineTests
    {
        [Fact]
        public async Task GetTimelineWithoutPeriodUsesCurrentMonthAndYear()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            DateTime today = DateTime.Today;
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmericanExpress", DisplayName = "American Express" };
            var responseDto = new ContoMovimentiDto(new ContoDto(conto, conto.InitialBalance), today.Month, today.Year, new DateOnly(today.Year, today.Month, 1).AddMonths(-1), new DateOnly(today.Year, today.Month, 1).AddMonths(2).AddDays(-1), 0m, 0m, []);
            host.MovimentoService.Setup(service => service.GetTimeline(conto.Name, today.Month, today.Year)).ReturnsAsync(responseDto);

            // Act
            HttpResponseMessage response = await host.Client.GetAsync($"/Finance/FrontEnd/Conto/{conto.Name}/Movimento/List");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            host.MovimentoService.VerifyAll();
        }

        [Fact]
        public async Task GetTimelineInvalidMonthReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();

            // Act
            HttpResponseMessage response = await host.Client.GetAsync("/Finance/FrontEnd/Conto/AmericanExpress/Movimento/List?month=13&year=2026");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            host.MovimentoService.Verify(service => service.GetTimeline(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BulkCreateMissingContoReturnsNotFoundBeforeProcessingItems()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var request = new BulkCreateMovimentoRequest(new BulkOptions(), [new BulkCreateMovimentoItem(1, new DateOnly(2026, 8, 26), "Movimento", "10,00")]);
            host.ContoRepository.Setup(repository => repository.GetByName("Missing")).ReturnsAsync((Conto?)null);

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Bulk/Conto/Missing/Movimento/Create", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            host.MovimentoService.Verify(service => service.Create(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}

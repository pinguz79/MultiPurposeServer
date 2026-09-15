using System.Net.Http.Json;
using System.Text.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class RevolvingIndicatorsPipelineTests
    {
        [Fact]
        public async Task GetList_WhenRevolving_SerializesDebtAndAvailabilitySeparately()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var conto = new Conto { Id = Guid.NewGuid(), Name = "AmEx", DisplayName = "American Express" };
            host.ContoService.Setup(service => service.GetConti()).ReturnsAsync([conto]);
            host.ContoService.Setup(service => service.GetStatus(conto)).ReturnsAsync(new ContoStatus(1650m, null, null, RevolvingIndicators: new RevolvingIndicators(1600m, 160m, -50m, 110m)));

            // Act
            using HttpResponseMessage response = await host.Client.GetAsync("/Finance/FrontEnd/Conto/List");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var account = payload!.RootElement[0];
            account.GetProperty("balance").GetDecimal().Should().Be(1650m);
            account.GetProperty("revolvingIndicators").GetProperty("remainingIncludingOverdraft").GetDecimal().Should().Be(110m);
        }
    }
}

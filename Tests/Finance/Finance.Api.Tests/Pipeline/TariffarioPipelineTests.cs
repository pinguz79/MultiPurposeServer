using System.Net;
using System.Net.Http.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class TariffarioPipelineTests
    {
        [Theory]
        [InlineData("Casello/List")]
        [InlineData("Tariffa/List")]
        public async Task Get_WhenUnauthenticated_ReturnsUnauthorized(string path)
        {
            // Arrange
            await using var host = new FinanceApiTestHost();

            // Act
            HttpResponseMessage response = await host.Client.GetAsync($"/Finance/BackEnd/{path}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateCasello_WhenNameDuplicated_ReturnsConflict()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            host.TariffarioService.Setup(service => service.CreateCasello("Varazze")).ThrowsAsync(new DuplicateNameException("Varazze"));

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Casello", new SaveCaselloRequest("Varazze"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetTariffa_WhenSameStation_ReturnsBadRequest()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            Guid id = Guid.NewGuid();
            host.TariffarioService.Setup(service => service.GetTratta(id, id)).ThrowsAsync(new ArgumentException("Entrata e uscita uguali."));

            // Act
            HttpResponseMessage response = await host.Client.GetAsync($"/Finance/BackEnd/Tariffa/{id}/{id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCasello_WhenValid_ReturnsCreated()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            host.TariffarioService.Setup(service => service.CreateCasello("Varazze")).ReturnsAsync(new Casello { Id = Guid.NewGuid(), Name = "Varazze" });

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Casello", new SaveCaselloRequest("Varazze"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}

using System.Net;
using System.Net.Http.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Tests.Pipeline
{
    public class TariffarioPipelineTests
    {
        [Theory]
        [InlineData(false, 204)]
        [InlineData(true, 204)]
        [InlineData(false, 404)]
        [InlineData(true, 404)]
        [InlineData(false, 409)]
        [InlineData(true, 409)]
        public async Task Delete_WhenAuthenticated_ReturnsExpectedStatus(bool route, int expected)
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var operation = new Mock<IApplicationOperation>();
            operation.Setup(item => item.Complete()).Returns(Task.CompletedTask);
            operation.Setup(item => item.DisposeAsync()).Returns(ValueTask.CompletedTask);
            host.TariffarioService.Setup(item => item.BeginOperation()).ReturnsAsync(operation.Object);
            Guid a = Guid.NewGuid();
            Guid b = Guid.NewGuid();
            var setup = route ? host.TariffarioService.Setup(item => item.DeleteTratta(a, b)) : host.TariffarioService.Setup(item => item.DeleteCasello(a));
            if (expected == 409)
            {
                setup.ThrowsAsync(new TariffarioInUseException());
            }
            else
            {
                setup.ReturnsAsync(expected == 204);
            }

            // Act
            using HttpResponseMessage response = await host.Client.DeleteAsync(route ? $"/Finance/BackEnd/Tariffa/{a}/{b}" : $"/Finance/BackEnd/Casello/{a}");

            // Assert
            ((int)response.StatusCode).Should().Be(expected);
            operation.Verify(item => item.Complete(), expected == 409 ? Times.Never() : Times.Once());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Delete_WhenUnauthenticated_DoesNotCallService(bool route)
        {
            // Arrange
            await using var host = new FinanceApiTestHost();

            // Act
            using HttpResponseMessage response = await host.Client.DeleteAsync(route ? $"/Finance/BackEnd/Tariffa/{Guid.NewGuid()}/{Guid.NewGuid()}" : $"/Finance/BackEnd/Casello/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            host.TariffarioService.VerifyNoOtherCalls();
        }
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

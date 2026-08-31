using System.Net;
using System.Net.Http.Json;

using Finance.Api.Application;
using Finance.Api.Tests.Infrastructure;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Pipeline
{
    public class CategoriaPipelineTests
    {
        [Fact]
        public async Task CreateReturnsNormalizedCategory()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            var categoria = new Categoria { Id = Guid.NewGuid(), Name = "Casa", DisplayName = "Casa" };
            host.CategoriaService.Setup(service => service.Create("casa", "Casa")).ReturnsAsync(categoria);
            var request = new CreateCategoriaRequest("casa", "Casa");

            // Act
            HttpResponseMessage response = await host.Client.PostAsJsonAsync("/Finance/BackEnd/Categoria", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            CategoriaDto? result = await response.Content.ReadFromJsonAsync<CategoriaDto>();
            result.Should().Be(new CategoriaDto("Casa", "Casa", 0));
        }

        [Fact]
        public async Task GetMissingCategoryReturnsNotFound()
        {
            // Arrange
            await using var host = new FinanceApiTestHost();
            host.Authenticate();
            host.CategoriaService.Setup(service => service.GetByName("Missing"))
                .ReturnsAsync(((Categoria Categoria, CategoriaUsage Usage)?)null);

            // Act
            HttpResponseMessage response = await host.Client.GetAsync("/Finance/BackEnd/Categoria/Missing");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

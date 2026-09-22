using System.Text.Json;

using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Bulk.Requests;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Enums;

using BulkContoController = Finance.Api.Controllers.BackEnd.Bulk.ContoController;

namespace Finance.Api.Tests.Infrastructure
{
    public class MovimentoCategoriaTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("Prestiti")]
        public async Task CreateMovimenti_PersistsOptionalCategory(string? categoryName)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            var categories = new CategoriaService(new CategoriaRepository(scenario.Context, scenario.Persistence), scenario.Persistence);
            var category = await categories.Create("Prestiti", "Prestiti");
            var service = new MovimentoService(scenario.Accounts, scenario.Movements, scenario.Evaluator, scenario.Persistence, categories);
            var controller = new BulkContoController(scenario.Accounts, service);
            var request = new BulkCreateMovimentoRequest(new BulkOptions(),
                [new BulkCreateMovimentoItem(1, new DateOnly(2026, 8, 31), "Interessi", "49,59", CategoryName: categoryName)]);

            // Act
            IActionResult response = await controller.CreateMovimenti("AmEx", request);

            // Assert
            var ok = response.Should().BeOfType<OkObjectResult>().Subject;
            var payload = JsonSerializer.SerializeToElement(ok.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            payload.GetProperty("items")[0].GetProperty("persisted").GetBoolean().Should().BeTrue();
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.Movimenti.SingleAsync()).CategoriaId.Should().Be(categoryName is null ? null : category.Id);
        }

        [Theory]
        [InlineData(BulkPersistenceStrategy.PartialSuccess, 1)]
        [InlineData(BulkPersistenceStrategy.AllOrNothing, 0)]
        public async Task CreateMovimenti_MissingCategoryRespectsPersistenceStrategy(BulkPersistenceStrategy strategy, int expectedCount)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            var categories = new CategoriaService(new CategoriaRepository(scenario.Context, scenario.Persistence), scenario.Persistence);
            var service = new MovimentoService(scenario.Accounts, scenario.Movements, scenario.Evaluator, scenario.Persistence, categories);
            var controller = new BulkContoController(scenario.Accounts, service);
            var request = new BulkCreateMovimentoRequest(new BulkOptions(strategy),
                [new BulkCreateMovimentoItem(1, new DateOnly(2026, 8, 31), "Valido", "1,00"),
                new BulkCreateMovimentoItem(2, new DateOnly(2026, 8, 31), "Non valido", "2,00", CategoryName: "Missing")]);

            // Act
            IActionResult response = await controller.CreateMovimenti("AmEx", request);

            // Assert
            var ok = response.Should().BeOfType<OkObjectResult>().Subject;
            JsonSerializer.Serialize(ok.Value).Should().Contain("CategoriaNotFound");
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(expectedCount);
        }
    }
}

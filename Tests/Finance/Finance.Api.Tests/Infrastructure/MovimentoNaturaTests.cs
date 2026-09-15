using System.Text.Json;

using Finance.Api.Application;
using Finance.Contracts.Bulk.Requests;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Enums;

using BulkContoController = Finance.Api.Controllers.BackEnd.Bulk.ContoController;

namespace Finance.Api.Tests.Infrastructure
{
    public class MovimentoNaturaTests
    {
        [Theory]
        [InlineData(NaturaMovimento.Ordinario)]
        [InlineData(NaturaMovimento.Interessi)]
        [InlineData(NaturaMovimento.Bollo)]
        [InlineData(NaturaMovimento.Rimborso)]
        public async Task CreateMovimenti_WhenNatureProvided_PersistsAndReturnsNature(NaturaMovimento natura)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            var service = CreateService(scenario);
            var controller = new BulkContoController(scenario.Accounts, service);
            var request = new BulkCreateMovimentoRequest(new BulkOptions(), [new BulkCreateMovimentoItem(1, new DateOnly(2023, 2, 6), "Importazione", "2,00", natura)]);

            // Act
            IActionResult response = await controller.CreateMovimenti("AmEx", request);

            // Assert
            var ok = response.Should().BeOfType<OkObjectResult>().Subject;
            var payload = JsonSerializer.SerializeToElement(ok.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            payload.GetProperty("items")[0].GetProperty("persisted").GetBoolean().Should().BeTrue();
            payload.GetProperty("items")[0].GetProperty("value").GetProperty("natura").GetInt32().Should().Be((int)natura);
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.Movimenti.SingleAsync()).Natura.Should().Be(natura);
        }

        [Fact]
        public async Task CreateMovimenti_WhenNatureOmitted_DefaultsToOrdinary()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            var controller = new BulkContoController(scenario.Accounts, CreateService(scenario));
            var request = new BulkCreateMovimentoRequest(new BulkOptions(), [new BulkCreateMovimentoItem(1, new DateOnly(2023, 2, 6), "Acquisto", "2,00")]);

            // Act
            await controller.CreateMovimenti("AmEx", request);

            // Assert
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.Movimenti.SingleAsync()).Natura.Should().Be(NaturaMovimento.Ordinario);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        public async Task CreateMovimenti_WhenNatureInvalid_RollsBackBatchWithSpecificError(int natura)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            var controller = new BulkContoController(scenario.Accounts, CreateService(scenario));
            var request = new BulkCreateMovimentoRequest(new BulkOptions(BulkPersistenceStrategy.AllOrNothing),
                [new BulkCreateMovimentoItem(1, new DateOnly(2023, 2, 6), "Valido", "2,00"), new BulkCreateMovimentoItem(2, new DateOnly(2023, 2, 6), "Non valido", "2,00", (NaturaMovimento)natura)]);

            // Act
            IActionResult response = await controller.CreateMovimenti("AmEx", request);

            // Assert
            var ok = response.Should().BeOfType<OkObjectResult>().Subject;
            JsonSerializer.Serialize(ok.Value).Should().Contain("InvalidNatura");
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(0);
        }

        [Theory]
        [InlineData(null, NaturaMovimento.Bollo)]
        [InlineData(NaturaMovimento.Ordinario, NaturaMovimento.Ordinario)]
        [InlineData(NaturaMovimento.Interessi, NaturaMovimento.Interessi)]
        public async Task Update_WhenNatureOptional_PreservesOrReplacesWithoutChangingOtherFields(NaturaMovimento? natura, NaturaMovimento expected)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            Movimento original = await scenario.Add(new DateOnly(2023, 2, 6), "2.00", NaturaMovimento.Bollo);
            Guid id = original.Id;
            string description = original.Description;
            var service = CreateService(scenario);

            // Act
            await service.Update(id, null, null, null, natura: natura);

            // Assert
            scenario.Context.ChangeTracker.Clear();
            Movimento saved = await scenario.Context.Movimenti.SingleAsync();
            saved.Natura.Should().Be(expected);
            saved.Formula.Should().Be("2.00");
            saved.Description.Should().Be(description);
            saved.Date.Should().Be(new DateOnly(2023, 2, 6));
        }

        [Fact]
        public async Task Update_WhenNatureInvalid_LeavesMovementUnchanged()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            Movimento original = await scenario.Add(new DateOnly(2023, 2, 6), "2.00", NaturaMovimento.Bollo);
            var service = CreateService(scenario);

            // Act
            var action = () => service.Update(original.Id, null, "Non salvare", null, natura: (NaturaMovimento)99);

            // Assert
            await action.Should().ThrowAsync<ArgumentOutOfRangeException>().Where(exception => exception.ParamName == "natura");
            scenario.Context.ChangeTracker.Clear();
            Movimento saved = await scenario.Context.Movimenti.SingleAsync();
            saved.Natura.Should().Be(NaturaMovimento.Bollo);
            saved.Description.Should().Be("Descrizione indipendente dalla natura");
        }

        private static MovimentoService CreateService(RevolvingFormulaScenario scenario) => new(scenario.Accounts, scenario.Movements, scenario.Evaluator, scenario.Persistence);
    }
}

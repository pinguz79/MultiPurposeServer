using Finance.Api.Application;
using Finance.Api.Controllers.BackEnd;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Finance.Api.Tests.Infrastructure
{
    public class CartaRevolvingConfigurationTests
    {
        private static readonly ConfigureCartaRevolvingRequest Request = new(1600m, 0.10m, 0.10m, 72.32m, 0.12m, 2m, 70m, 6, 19, "HelloBank", new(2026, 9, 1), new(2026, 12, 31));

        [Fact]
        public async Task ConfigureCartaRevolving_WhenTenYearsRequested_CreatesAndValidatesAllOccurrences()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 0m);
            ConfigurazioneController controller = CreateController(scenario);

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", Request with { ValidTo = new(2036, 8, 31) });

            // Assert
            ((ObjectResult)response).StatusCode.Should().Be(201);
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(480);
            scenario.Cache.CanReuseAcrossEvaluations.Should().BeFalse();
        }

        [Fact]
        public async Task ConfigureCartaRevolving_WhenAccountIsNew_CreatesParametersAndFourLinkedPlans()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 3000m);
            ConfigurazioneController controller = CreateController(scenario);

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", Request);

            // Assert
            var created = (ObjectResult)response;
            created.StatusCode.Should().Be(201);
            var dto = (CartaRevolvingDto)created.Value!;
            dto.Created.Should().BeTrue();
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.ParametriConto.CountAsync()).Should().Be(9);
            (await scenario.Context.Pianificazioni.CountAsync()).Should().Be(4);
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(16);
            (await scenario.Context.CorrelazioniPianificazioni.CountAsync()).Should().Be(1);
            Movimento interest = await scenario.Context.Movimenti.FirstAsync(movement => movement.PianificazioneId == dto.PianificazioneInteressiId);
            interest.Natura.Should().Be(NaturaMovimento.Interessi);
            interest.Date.Day.Should().Be(6);
            Movimento repayment = await scenario.Context.Movimenti.FirstAsync(movement => movement.PianificazioneId == dto.PianificazioneRimborsoId);
            repayment.Natura.Should().Be(NaturaMovimento.Rimborso);
            repayment.Conto.Name.Should().Be("AmEx");
            repayment.Date.Day.Should().Be(19);
            Movimento debit = await scenario.Context.Movimenti.FirstAsync(movement => movement.PianificazioneId == dto.PianificazioneAddebitoId);
            debit.Conto.Name.Should().Be("HelloBank");
            debit.Natura.Should().Be(NaturaMovimento.Ordinario);
            debit.Formula.Should().Be(repayment.Formula);
        }

        [Fact]
        public async Task ConfigureCartaRevolving_WhenRepeated_DoesNotDuplicateMovementsAndPreservesOverrides()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 0m);
            ConfigurazioneController controller = CreateController(scenario);
            var initial = (CartaRevolvingDto)((ObjectResult)await controller.ConfigureCartaRevolving("AmEx", Request)).Value!;
            IReadOnlyList<ParametroConto> existing = await scenario.Parameters.GetByName(scenario.Card.Id, "RataMinima");
            await scenario.Parameters.Replace(scenario.Card.Id, "RataMinima", "RataMinima", TipoParametroConto.Importo, [
                new ParametroConto { Index = 0, DisplayName = "Eccezione", Value = 90m, ValidFrom = new(2027, 1, 1) },
                new ParametroConto { Id = existing[0].Id, Index = 1, DisplayName = "Permanente", Value = 72.32m },
            ]);

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", Request with { RataMinima = 80m });

            // Assert
            var dto = (CartaRevolvingDto)((OkObjectResult)response).Value!;
            dto.Created.Should().BeFalse();
            dto.PianificazioneInteressiId.Should().Be(initial.PianificazioneInteressiId);
            dto.PianificazioneAddebitoId.Should().Be(initial.PianificazioneAddebitoId);
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(16);
            (await scenario.Context.CorrelazioniPianificazioni.CountAsync()).Should().Be(1);
            IReadOnlyList<ParametroConto> values = await scenario.Parameters.GetByName(scenario.Card.Id, "RataMinima");
            values.Select(value => value.Value).Should().Equal(90m, 80m);
        }

        [Theory]
        [InlineData("Plafond")]
        [InlineData("RipristinoPlafond")]
        public async Task ConfigureCartaRevolving_WhenProfileIsPartialOrBalanceCard_ReturnsConflictWithoutWrites(string existingParameter)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 0m);
            await scenario.SetParameter(existingParameter, 1600m);
            ConfigurazioneController controller = CreateController(scenario);

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", Request);

            // Assert
            response.Should().BeOfType<ConflictObjectResult>();
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.ParametriConto.CountAsync()).Should().Be(1);
            (await scenario.Context.Pianificazioni.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ConfigureCartaRevolving_WhenExistingFormulaFails_RollsBackAllNewData()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 0m);
            await scenario.Add(new DateOnly(2026, 9, 1), "[Missing]");
            ConfigurazioneController controller = CreateController(scenario);

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", Request);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.ParametriConto.CountAsync()).Should().Be(0);
            (await scenario.Context.Pianificazioni.CountAsync()).Should().Be(0);
            (await scenario.Context.CorrelazioniPianificazioni.CountAsync()).Should().Be(0);
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task ConfigureCartaRevolving_WhenDatabaseWriteFails_RollsBackParametersPlansAndMovements()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 0m);
            await scenario.Context.Database.ExecuteSqlRawAsync("CREATE TRIGGER FailCorrelation BEFORE INSERT ON CorrelazioniPianificazioni BEGIN SELECT RAISE(ABORT, 'Errore simulato'); END;");
            ConfigurazioneController controller = CreateController(scenario);

            // Act
            Func<Task> action = async () => await controller.ConfigureCartaRevolving("AmEx", Request);

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
            scenario.Context.ChangeTracker.Clear();
            (await scenario.Context.ParametriConto.CountAsync()).Should().Be(0);
            (await scenario.Context.Pianificazioni.CountAsync()).Should().Be(0);
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(0);
        }

        [Theory]
        [InlineData(6, 6)]
        [InlineData(30, 31)]
        public async Task ConfigureCartaRevolving_WhenClosingAndPaymentCoincide_RejectsWithoutWrites(int closingDay, int paymentDay)
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            ConfigurazioneController controller = CreateController(scenario);
            ConfigureCartaRevolvingRequest request = Request with { ChiusuraCiclo = closingDay, Addebito = paymentDay, ValidFrom = new(2027, 2, 1), ValidTo = new(2027, 2, 28) };

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", request);

            // Assert
            response.Should().BeOfType<BadRequestObjectResult>();
            (await scenario.Context.ParametriConto.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ConfigureCartaRevolving_WhenPlanIsMissing_ReturnsConflictInsteadOfRecreatingIt()
        {
            // Arrange
            await using var scenario = await RevolvingFormulaScenario.Create(configureParameters: false);
            await scenario.Accounts.CreateConto("HelloBank", "Hello Bank", 0m);
            ConfigurazioneController controller = CreateController(scenario);
            var initial = (CartaRevolvingDto)((ObjectResult)await controller.ConfigureCartaRevolving("AmEx", Request)).Value!;
            Pianificazione plan = await scenario.Context.Pianificazioni.SingleAsync(item => item.Id == initial.PianificazioneBolloId);
            await new PianificazioneRepository(scenario.Context, scenario.Persistence, scenario.Cache).Delete(plan, false);

            // Act
            IActionResult response = await controller.ConfigureCartaRevolving("AmEx", Request);

            // Assert
            response.Should().BeOfType<ConflictObjectResult>();
            (await scenario.Context.Pianificazioni.CountAsync()).Should().Be(3);
            (await scenario.Context.Movimenti.CountAsync()).Should().Be(16);
        }

        private static ConfigurazioneController CreateController(RevolvingFormulaScenario scenario)
        {
            var parameters = new ParametroContoService(scenario.Parameters, scenario.Accounts, scenario.Persistence);
            var service = new CartaRevolvingService(scenario.Accounts, parameters, new PianificazioneRepository(scenario.Context, scenario.Persistence, scenario.Cache),
                scenario.Movements, new CartaRevolvingRepository(scenario.Context, scenario.Persistence, scenario.Cache), scenario.Evaluator, scenario.Cache, scenario.Persistence);
            return new ConfigurazioneController(Mock.Of<ICartaASaldoService>(), service);
        }
    }
}

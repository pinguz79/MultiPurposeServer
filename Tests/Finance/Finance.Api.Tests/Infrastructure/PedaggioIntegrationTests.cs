using Finance.Api.Application;
using Finance.Api.Extensions;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

namespace Finance.Api.Tests.Infrastructure
{
    public sealed class PedaggioIntegrationTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private ServiceProvider _services = null!;
        private IServiceScope _scope = null!;
        private IServiceProvider Services => _scope.ServiceProvider;
        private ITariffarioService Tariffario => Services.GetRequiredService<ITariffarioService>();
        private IMovimentoService Movimenti => Services.GetRequiredService<IMovimentoService>();
        private IPedaggioService Pedaggi => Services.GetRequiredService<IPedaggioService>();
        private Guid _entrata;
        private Guid _uscita;

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();
            var services = new ServiceCollection();
            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(host => host.EnvironmentName).Returns(Environments.Development);
            services.AddLogging();
            services.AddFinance(new ConfigurationBuilder().Build().GetSection("Finance"), environment.Object);
            services.AddDbContext<FinanceContext>(options => options.UseSqlite(_connection));
            _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            _scope = _services.CreateScope();
            await Services.GetRequiredService<FinanceContext>().Database.MigrateAsync();
            Conto conto = await Services.GetRequiredService<IContoRepository>().CreateConto("Telepass", "Telepass", 0m);
            await Services.GetRequiredService<IParametroContoRepository>().Replace(conto.Id, null, "AbilitaPedaggi", TipoParametroConto.Booleano,
                [new ParametroConto { DisplayName = "Pedaggi", Value = 1m }]);
            _entrata = (await Tariffario.CreateCasello("Genova Est")).Id;
            _uscita = (await Tariffario.CreateCasello("Varazze")).Id;
        }

        [Fact]
        public async Task Create_WhenTariffUnknown_KeepsFormulaAndBlocksConfirmation()
        {
            // Arrange
            var request = new SavePedaggioRequest("Telepass", new DateOnly(2026, 9, 1), _entrata, _uscita);
            await using var operation = await Movimenti.BeginOperation();

            // Act
            Pedaggio pedaggio = await Pedaggi.Create(request);
            await operation.Complete();

            // Assert
            (await Tariffario.GetTratta(_uscita, _entrata)).Should().ContainSingle().Which.Formula.Should().Be("0.00");
            var review = await Movimenti.GetForReview(false, "Telepass", null, null);
            review.Should().ContainSingle().Which.CanConfirm.Should().BeFalse();
            review[0].Formula.Should().Be(PedaggioFormula.Create(_entrata, _uscita));
            var confirm = () => Movimenti.Confirm([pedaggio.MovimentoId]);
            await confirm.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Confirm_WhenTariffUpdated_FreezesAmountAndPreservesStations()
        {
            // Arrange
            Pedaggio pedaggio = await Pedaggi.Create(new SavePedaggioRequest("Telepass", new DateOnly(2026, 9, 1), _entrata, _uscita));
            await Tariffario.SaveTratta(_uscita, _entrata, [new TariffaTrattaDefinitionRequest(4.25m)]);

            // Act
            await Movimenti.Confirm([pedaggio.MovimentoId]);
            await Tariffario.SaveTratta(_entrata, _uscita, [new TariffaTrattaDefinitionRequest(5m)]);

            // Assert
            var movement = (await Movimenti.GetForReview(false, "Telepass", null, null)).Single();
            movement.Amount.Should().Be(4.25m);
            movement.IsConfirmed.Should().BeTrue();
            movement.Pedaggio!.EntrataId.Should().Be(_entrata);
            movement.Pedaggio.UscitaId.Should().Be(_uscita);
        }

        [Fact]
        public async Task Evaluate_WhenFutureExceptionExists_UsesInclusiveDatesAndBaseFallback()
        {
            // Arrange
            await Tariffario.SaveTratta(_entrata, _uscita,
                [new TariffaTrattaDefinitionRequest(6m, new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 31)), new TariffaTrattaDefinitionRequest(4m)]);
            var evaluator = Services.GetRequiredService<IFormulaEvaluator>();
            string formula = PedaggioFormula.Create(_uscita, _entrata);

            // Act
            var start = await evaluator.Evaluate(formula, new DateOnly(2027, 1, 1));
            var end = await evaluator.Evaluate(formula, new DateOnly(2027, 1, 31));
            var outside = await evaluator.Evaluate(formula, new DateOnly(2027, 2, 1));

            // Assert
            start.Value.Should().Be(6m);
            end.Value.Should().Be(6m);
            outside.Value.Should().Be(4m);
        }

        [Fact]
        public async Task CreateCasello_WhenNameDiffersOnlyByCaseAndSpaces_RejectsDuplicate()
        {
            // Arrange
            string name = "  genova   est  ";

            // Act
            var create = () => Tariffario.CreateCasello(name);

            // Assert
            await create.Should().ThrowAsync<DuplicateNameException>();
        }

        [Fact]
        public async Task Create_WhenUnknownAndConfirmed_RollsBackNewTariffAndMovement()
        {
            // Arrange
            var request = new SavePedaggioRequest("Telepass", new DateOnly(2026, 9, 1), _entrata, _uscita, IsConfirmed: true);

            // Act
            var create = async () =>
            {
                await using var operation = await Movimenti.BeginOperation();
                await Pedaggi.Create(request);
                await operation.Complete();
            };

            // Assert
            await create.Should().ThrowAsync<ArgumentException>();
            (await Tariffario.GetTratta(_entrata, _uscita)).Should().BeEmpty();
            (await Movimenti.GetForReview(false, "Telepass", null, null)).Should().BeEmpty();
            Services.GetRequiredService<FinanceContext>().Database.HasPendingModelChanges().Should().BeFalse();
        }

        [Fact]
        public async Task Update_WhenConfirmedAndStationsChange_PreservesAmount()
        {
            // Arrange
            await Tariffario.SaveTratta(_entrata, _uscita, [new TariffaTrattaDefinitionRequest(4m)]);
            var request = new SavePedaggioRequest("Telepass", new DateOnly(2026, 9, 1), _entrata, _uscita, IsConfirmed: true);
            Pedaggio pedaggio = await Pedaggi.Create(request);
            Guid other = (await Tariffario.CreateCasello("Chiavari")).Id;

            // Act
            await Pedaggi.Update(pedaggio.MovimentoId, request with { UscitaId = other, Date = request.Date.AddDays(1) });

            // Assert
            var movement = (await Movimenti.GetForReview(false, "Telepass", null, null)).Single();
            movement.Amount.Should().Be(4m);
            movement.IsConfirmed.Should().BeTrue();
            movement.Pedaggio!.UscitaId.Should().Be(other);
        }

        [Fact]
        public async Task SetConfirmation_WhenReopened_RestoresDynamicFormula()
        {
            // Arrange
            await Tariffario.SaveTratta(_entrata, _uscita, [new TariffaTrattaDefinitionRequest(4m)]);
            Pedaggio pedaggio = await Pedaggi.Create(new SavePedaggioRequest("Telepass", new DateOnly(2026, 9, 1), _entrata, _uscita, IsConfirmed: true));
            await Tariffario.SaveTratta(_entrata, _uscita, [new TariffaTrattaDefinitionRequest(5m)]);

            // Act
            await Movimenti.SetConfirmation(pedaggio.MovimentoId, false);

            // Assert
            var movement = (await Movimenti.GetForReview(false, "Telepass", null, null)).Single();
            movement.Amount.Should().Be(5m);
            movement.Formula.Should().Be(PedaggioFormula.Create(_entrata, _uscita));
            movement.IsConfirmed.Should().BeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task CreateParametro_WhenBooleanValueInvalid_RejectsValue(int value)
        {
            // Arrange
            var service = Services.GetRequiredService<IParametroContoService>();

            // Act
            var create = () => service.Create("Telepass", "TestBooleano", TipoParametroConto.Booleano,
                [new ParametroContoDefinitionRequest(null, "Test", value, null, null)]);

            // Assert
            await create.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Delete_WhenMovementIsPedaggio_DeletesLinkButRetainsTariff()
        {
            // Arrange
            Pedaggio pedaggio = await Pedaggi.Create(new SavePedaggioRequest("Telepass", new DateOnly(2026, 9, 1), _entrata, _uscita));

            // Act
            await Movimenti.Delete(pedaggio.MovimentoId);

            // Assert
            (await Pedaggi.Get(pedaggio.MovimentoId)).Should().BeNull();
            (await Tariffario.GetTratta(_entrata, _uscita)).Should().ContainSingle();
        }

        public async Task DisposeAsync()
        {
            _scope.Dispose();
            await _services.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

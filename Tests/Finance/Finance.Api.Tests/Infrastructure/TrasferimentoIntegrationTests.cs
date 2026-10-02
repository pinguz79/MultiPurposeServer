using System.Globalization;

using Finance.Api.Application;
using Finance.Api.Controllers.BackEnd;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Moq;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Infrastructure
{
    public sealed class TrasferimentoIntegrationTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly Mock<IFormulaEvaluator> _evaluator = new();
        private readonly Mock<IParametroContoService> _parameters = new();
        private FinanceContext _db = null!;
        private MovimentoService _movimenti = null!;
        private TrasferimentoService _service = null!;
        private GruppoMovimentiRepository _groups = null!;
        private readonly Conto _origin = new() { Id = Guid.NewGuid(), Name = "Origin", DisplayName = "Origin" };
        private readonly Conto _destination = new() { Id = Guid.NewGuid(), Name = "Destination", DisplayName = "Destination" };
        private static readonly DateOnly Date = new(2026, 10, 24);

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();
            _db = new FinanceContext(new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(_connection).Options);
            await _db.Database.MigrateAsync();
            _db.Conti.AddRange(_origin, _destination);
            await _db.SaveChangesAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(_db);
            var accounts = new ContoRepository(_db, persistence);
            var movements = new MovimentoRepository(_db, persistence);
            _groups = new GruppoMovimentiRepository(_db, persistence);
            _evaluator.Setup(item => item.Validate(It.IsAny<string>())).ReturnsAsync((string formula) => new FormulaValidationResult(formula, [], []));
            _evaluator.Setup(item => item.Evaluate(It.IsAny<string>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((string formula, DateOnly date) => new FormulaEvaluationResult(decimal.Parse(formula, CultureInfo.InvariantCulture), false, null));
            _movimenti = new MovimentoService(accounts, movements, _evaluator.Object, persistence, gruppoRepository: _groups);
            _service = new TrasferimentoService(accounts, _movimenti, movements, _groups, _parameters.Object);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        public async Task CreateUsesAccountSignsAndGroupsOnlyPending(bool originCard, bool destinationCard, bool confirmed)
        {
            // Arrange
            ConfigureCard(_origin, originCard);
            ConfigureCard(_destination, destinationCard);

            // Act
            IActionResult result = await new TrasferimentoController(_service, _movimenti).Create(Request(confirmed));
            _db.ChangeTracker.Clear();
            List<Movimento> items = await _db.Movimenti.ToListAsync();

            // Assert
            ((ObjectResult)result).StatusCode.Should().Be(201);
            items.Should().HaveCount(2);
            decimal.Parse(items.Single(item => item.ContoId == _origin.Id).Formula, CultureInfo.InvariantCulture).Should().Be(originCard ? 500m : -500m);
            decimal.Parse(items.Single(item => item.ContoId == _destination.Id).Formula, CultureInfo.InvariantCulture).Should().Be(destinationCard ? -500m : 500m);
            items.Should().OnlyContain(item => item.Date == Date && item.IsConfirmed == confirmed && item.CategoriaId == null);
            (await _db.GruppiMovimenti.CountAsync()).Should().Be(confirmed ? 0 : 1);
            if (!confirmed)
            {
                items[0].GruppoMovimenti!.Movimenti.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task ConfirmationIncludesFutureMembersAndDissolvesGroup()
        {
            // Arrange
            await new TrasferimentoController(_service, _movimenti).Create(Request());
            List<Movimento> items = await _db.Movimenti.ToListAsync();
            items[1].Date = Date.AddDays(7);
            await _db.SaveChangesAsync();

            // Act
            await using (var operation = await _movimenti.BeginOperation())
            {
                (await _movimenti.Confirm([items[0].Id, items[1].Id])).Should().Be(2);
                await operation.Complete();
            }
            _db.ChangeTracker.Clear();

            // Assert
            (await _db.Movimenti.ToListAsync()).Should().OnlyContain(item => item.IsConfirmed && item.GruppoMovimentiId == null && item.PianificazioneId == null);
            (await _db.Movimenti.FindAsync(items[1].Id))!.Date.Should().Be(Date.AddDays(7));
            (await _db.GruppiMovimenti.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task FailureEvaluatingOneMemberPreservesEntireGroup()
        {
            // Arrange
            await new TrasferimentoController(_service, _movimenti).Create(Request());
            List<Movimento> items = await _db.Movimenti.ToListAsync();
            _evaluator.Setup(item => item.Evaluate(items[1].Formula, Date)).ReturnsAsync(new FormulaEvaluationResult(null, false, "Errore"));

            // Act
            var result = await new MovimentoController(_movimenti).Consolidate(new ConfirmMovimentiRequest([items[0].Id]));
            _db.ChangeTracker.Clear();

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
            (await _db.Movimenti.ToListAsync()).Should().OnlyContain(item => !item.IsConfirmed && item.GruppoMovimentiId != null);
            (await _db.GruppiMovimenti.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteSelectionPreservesRemainingGroupThenDissolvesSingleton()
        {
            // Arrange
            var items = Enumerable.Range(0, 4).Select(index => new Movimento
            {
                Id = Guid.NewGuid(), ContoId = _origin.Id, Date = Date, Description = "Test", Formula = "1.00",
            }).ToList();
            _db.Movimenti.AddRange(items);
            await _db.SaveChangesAsync();
            GruppoMovimenti group = await _groups.Create(items);
            var controller = new GruppoMovimentiController(_service, _movimenti);

            // Act
            (await controller.Delete(group.Id, new DeleteGruppoMovimentiRequest([items[0].Id, items[1].Id]))).Should().BeOfType<NoContentResult>();
            _db.ChangeTracker.Clear();

            // Assert
            (await _groups.GetById(group.Id))!.Movimenti.Should().HaveCount(2);
            await new MovimentoController(_movimenti).Delete(items[2].Id);
            _db.ChangeTracker.Clear();
            (await _db.GruppiMovimenti.CountAsync()).Should().Be(0);
            (await _db.Movimenti.SingleAsync()).GruppoMovimentiId.Should().BeNull();
        }

        [Fact]
        public async Task FailedSecondPhaseDoesNotUndoInitialEdit()
        {
            // Arrange
            await new TrasferimentoController(_service, _movimenti).Create(Request());
            List<Movimento> items = await _db.Movimenti.ToListAsync();
            await new MovimentoController(_movimenti).Update(items[0].Id, Change("Prima modifica"));
            _evaluator.Setup(item => item.Validate("invalid")).ReturnsAsync(new FormulaValidationResult("invalid", [], ["Errore"]));
            var request = new UpdateGruppoMovimentiRequest([
                new(items[0].Id, Change("Seconda modifica")),
                new(items[1].Id, new UpdateMovimentoRequest(null, null, "invalid", null, null))]);

            // Act
            (await new GruppoMovimentiController(_service, _movimenti).Update(items[0].GruppoMovimentiId!.Value, request)).Should().BeOfType<BadRequestObjectResult>();
            _db.ChangeTracker.Clear();

            // Assert
            (await _db.Movimenti.FindAsync(items[0].Id))!.Description.Should().Be("Prima modifica");
            (await _db.Movimenti.FindAsync(items[1].Id))!.Description.Should().Be("Copertura");
        }

        [Fact]
        public async Task SecondMovementFailureRollsBackFirstInsert()
        {
            // Arrange
            _evaluator.Setup(item => item.Validate("500.00")).ReturnsAsync(new FormulaValidationResult("500.00", [], ["Errore"]));

            // Act
            (await new TrasferimentoController(_service, _movimenti).Create(Request())).Should().BeOfType<BadRequestObjectResult>();

            // Assert
            (await _db.Movimenti.CountAsync()).Should().Be(0);
            (await _db.GruppiMovimenti.CountAsync()).Should().Be(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1.001)]
        public async Task InvalidAmountCreatesNothing(decimal amount)
        {
            (await new TrasferimentoController(_service, _movimenti).Create(Request() with { Amount = amount })).Should().BeOfType<BadRequestObjectResult>();
            (await _db.Movimenti.CountAsync()).Should().Be(0);
        }

        private void ConfigureCard(Conto account, bool enabled)
        {
            if (enabled)
            {
                _parameters.Setup(item => item.Resolve(account.Id, It.IsAny<string>(), Date)).ReturnsAsync(new ParametroConto { Value = 1 });
            }
        }

        [Fact]
        public async Task SameAccountAndForeignSelectionAreRejected()
        {
            (await new TrasferimentoController(_service, _movimenti).Create(Request() with { DestinazioneName = "origin" })).Should().BeOfType<BadRequestObjectResult>();
            (await _db.Movimenti.CountAsync()).Should().Be(0);
            await new TrasferimentoController(_service, _movimenti).Create(Request());
            Guid groupId = (await _db.GruppiMovimenti.SingleAsync()).Id;
            (await new GruppoMovimentiController(_service, _movimenti).Delete(groupId, new DeleteGruppoMovimentiRequest([Guid.NewGuid()])))
                .Should().BeOfType<BadRequestObjectResult>();
            (await _db.Movimenti.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task PatchConfirmationAlsoConfirmsGroup()
        {
            await new TrasferimentoController(_service, _movimenti).Create(Request());
            Guid id = (await _db.Movimenti.FirstAsync()).Id;
            await new MovimentoController(_movimenti).Update(id, new UpdateMovimentoRequest(null, null, null, null, null, IsConfirmed: true));
            _db.ChangeTracker.Clear();
            (await _db.Movimenti.ToListAsync()).Should().OnlyContain(item => item.IsConfirmed && item.GruppoMovimentiId == null);
            (await _db.GruppiMovimenti.CountAsync()).Should().Be(0);
        }

        private static UpdateMovimentoRequest Change(string description) => new(null, description, null, null, null);

        private static CreateTrasferimentoRequest Request(bool confirmed = false) => new("Origin", "Destination", 500m, Date, "Copertura", confirmed);

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

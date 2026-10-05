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
            _movimenti = new MovimentoService(accounts, movements, _evaluator.Object, persistence, parametroContoService: _parameters.Object, gruppoRepository: _groups);
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

        [Theory]
        [InlineData(false, true, "-20.00", "20.00")]
        [InlineData(true, false, "20.00", "-20.00")]
        [InlineData(true, true, "20.00", "20.00")]
        [InlineData(false, false, "-20.00", "-20.00")]
        [InlineData(false, true, "20.00", "-20.00")]
        [InlineData(false, true, "0.00", "0.00")]
        public async Task ChangeAccountPreservesEconomicMeaning(bool sourceCard, bool targetCard, string before, string after)
        {
            ConfigureCard(_origin, sourceCard);
            ConfigureCard(_destination, targetCard);
            Movimento movement = await _movimenti.Create(_origin.Id, Date, "Spesa", before);
            var result = await new MovimentoController(_movimenti).Update(movement.Id,
                new UpdateMovimentoRequest(null, null, null, null, null, ContoName: "Destination"));
            result.Should().BeOfType<OkObjectResult>();
            _db.ChangeTracker.Clear();
            Movimento saved = (await _db.Movimenti.FindAsync(movement.Id))!;
            saved.ContoId.Should().Be(_destination.Id);
            saved.Formula.Should().Be(after);
            saved.IsConfirmed.Should().BeFalse();
            saved.Date.Should().Be(Date);
            saved.Description.Should().Be("Spesa");
        }

        [Fact]
        public async Task ChangeAccountCanConfirmRecurringFormulaAtomically()
        {
            ConfigureCard(_destination, true);
            Movimento movement = await _movimenti.Create(_origin.Id, Date, "Spesa", "-[Gas]");
            _evaluator.Setup(item => item.Evaluate("-(-[Gas])", Date)).ReturnsAsync(new FormulaEvaluationResult(20m, false, null));
            var result = await new MovimentoController(_movimenti).Update(movement.Id,
                new UpdateMovimentoRequest(null, null, null, null, null, IsConfirmed: true, ContoName: "Destination"));
            result.Should().BeOfType<OkObjectResult>();
            _db.ChangeTracker.Clear();
            Movimento saved = (await _db.Movimenti.FindAsync(movement.Id))!;
            saved.ContoId.Should().Be(_destination.Id);
            saved.IsConfirmed.Should().BeTrue();
            decimal.Parse(saved.Formula, CultureInfo.InvariantCulture).Should().Be(20m);
        }

        [Theory]
        [InlineData(NaturaMovimento.Interessi)]
        [InlineData(NaturaMovimento.Bollo)]
        [InlineData(NaturaMovimento.Rimborso)]
        public async Task TechnicalAccountChangeRollsBackOtherEdits(NaturaMovimento natura)
        {
            Movimento movement = await _movimenti.Create(_origin.Id, Date, "Originale", "20.00", natura);
            var result = await new MovimentoController(_movimenti).Update(movement.Id,
                new UpdateMovimentoRequest(null, "Modificata", null, null, null, ContoName: "Destination"));
            result.Should().BeOfType<BadRequestObjectResult>();
            _db.ChangeTracker.Clear();
            Movimento saved = (await _db.Movimenti.FindAsync(movement.Id))!;
            saved.Description.Should().Be("Originale");
            saved.ContoId.Should().Be(_origin.Id);
        }

        [Fact]
        public async Task AccountBoundFormulaAndSameAccountTransferAreRejected()
        {
            Movimento movement = await _movimenti.Create(_origin.Id, Date, "Rata", "[Origin.Rata]");
            _evaluator.Setup(item => item.Validate("[Origin.Rata]")).ReturnsAsync(new FormulaValidationResult("[Origin.Rata]", ["Origin.Rata"], []));
            (await new MovimentoController(_movimenti).Update(movement.Id,
                new UpdateMovimentoRequest(null, null, null, null, null, ContoName: "Destination"))).Should().BeOfType<BadRequestObjectResult>();
            await new TrasferimentoController(_service, _movimenti).Create(Request());
            Movimento grouped = await _db.Movimenti.FirstAsync(item => item.GruppoMovimentiId != null && item.ContoId == _origin.Id);
            (await new MovimentoController(_movimenti).Update(grouped.Id,
                new UpdateMovimentoRequest(null, null, null, null, null, ContoName: "Destination"))).Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AccountChangePreservesPlanCategoryAndOtherOccurrences()
        {
            var plan = new Pianificazione { Id = Guid.NewGuid(), Conto = _origin,
                Periodicita = new Periodicita { Id = Guid.NewGuid(), Intervallo = 1 }, ValidFrom = Date, ValidTo = Date.AddYears(1) };
            var category = new Categoria { Id = Guid.NewGuid(), Name = "Ballo", DisplayName = "Ballo" };
            Movimento first = await _movimenti.Create(_origin.Id, Date, "Spesa", "-20.00");
            Movimento second = await _movimenti.Create(_origin.Id, Date.AddMonths(1), "Spesa", "-20.00");
            first.Pianificazione = plan;
            second.Pianificazione = plan;
            first.Categoria = category;
            _db.Pianificazioni.Add(plan);
            _db.Categorie.Add(category);
            await _db.SaveChangesAsync();
            (await new MovimentoController(_movimenti).Update(first.Id,
                new UpdateMovimentoRequest(null, null, null, null, null, ContoName: "Destination"))).Should().BeOfType<OkObjectResult>();
            _db.ChangeTracker.Clear();
            Movimento saved = (await _db.Movimenti.FindAsync(first.Id))!;
            saved.PianificazioneId.Should().Be(plan.Id);
            saved.CategoriaId.Should().Be(category.Id);
            saved.ContoId.Should().Be(_destination.Id);
            (await _db.Movimenti.FindAsync(second.Id))!.ContoId.Should().Be(_origin.Id);
            (await _db.Pianificazioni.FindAsync(plan.Id))!.ContoId.Should().Be(_origin.Id);
        }

        [Fact]
        public async Task FailedConfirmationRollsBackAccountAndFormula()
        {
            ConfigureCard(_destination, true);
            Movimento movement = await _movimenti.Create(_origin.Id, Date, "Spesa", "-20.00");
            _evaluator.Setup(item => item.Evaluate("20.00", Date)).ReturnsAsync(new FormulaEvaluationResult(null, false, "Errore"));
            (await new MovimentoController(_movimenti).Update(movement.Id,
                new UpdateMovimentoRequest(null, null, null, null, null, IsConfirmed: true, ContoName: "Destination"))).Should().BeOfType<UnprocessableEntityObjectResult>();
            _db.ChangeTracker.Clear();
            Movimento saved = (await _db.Movimenti.FindAsync(movement.Id))!;
            saved.ContoId.Should().Be(_origin.Id);
            saved.Formula.Should().Be("-20.00");
            saved.IsConfirmed.Should().BeFalse();
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

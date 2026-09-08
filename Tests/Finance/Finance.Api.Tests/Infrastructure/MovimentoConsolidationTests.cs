using Finance.Api.Application;
using Finance.Api.Controllers.BackEnd;
using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Responses;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Infrastructure
{
    public class MovimentoConsolidationTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Consolidate_WhenPastAndFutureMovementsExist_OnlyChangesPastMovements(bool repeat)
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using FinanceContext context = await CreateContext(connection);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank", DisplayName = "Hello Bank" };
            var plan = new Pianificazione
            {
                Id = Guid.NewGuid(),
                Conto = conto,
                Periodicita = new Periodicita { Id = Guid.NewGuid(), Frequenza = FrequenzaPeriodicita.Mensile, Intervallo = 1 },
                ValidFrom = today.AddMonths(-2),
                ValidTo = today.AddYears(1),
            };
            Movimento past = CreateMovement(conto, today.AddDays(-1), "Passato", "1 + 2");
            Movimento constant = CreateMovement(conto, today.AddDays(-2), "Costante pianificata", "0.00");
            Movimento current = CreateMovement(conto, today, "Oggi", "1 + 2");
            Movimento future = CreateMovement(conto, today.AddDays(1), "Futuro", "1 + 2");
            foreach (Movimento movimento in new[] { past, constant, current, future })
            {
                movimento.Pianificazione = plan;
            }

            context.Movimenti.AddRange(past, constant, current, future);
            await context.SaveChangesAsync();
            MovimentoController controller = CreateController(context);
            if (repeat)
            {
                await controller.Consolidate();
            }

            // Act
            IActionResult response = await controller.Consolidate();

            // Assert
            var result = (ConsolidamentoMovimentiDto)((OkObjectResult)response).Value!;
            result.ConsolidatedCount.Should().Be(repeat ? 0 : 2);
            context.ChangeTracker.Clear();
            Movimento savedPast = await context.Movimenti.SingleAsync(item => item.Id == past.Id);
            savedPast.Formula.Should().Be("3.00");
            savedPast.PianificazioneId.Should().BeNull();
            (await context.Movimenti.SingleAsync(item => item.Id == constant.Id)).PianificazioneId.Should().BeNull();
            IReadOnlyList<Movimento> untouched = await context.Movimenti.Where(item => item.Date >= today).ToListAsync();
            untouched.Should().HaveCount(2).And.OnlyContain(item => item.Formula == "1 + 2" && item.PianificazioneId == plan.Id);
            (await context.Pianificazioni.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Consolidate_WhenFormulaFails_ReturnsMovementErrorWithoutChanges()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using FinanceContext context = await CreateContext(connection);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank" };
            Movimento valid = CreateMovement(conto, today.AddDays(-2), "Valido", "1 + 2");
            Movimento invalid = CreateMovement(conto, today.AddDays(-1), "Non valido", "[Missing]");
            context.Movimenti.AddRange(valid, invalid);
            await context.SaveChangesAsync();
            MovimentoController controller = CreateController(context);

            // Act
            IActionResult response = await controller.Consolidate();

            // Assert
            var error = (FormulaEvaluationErrorDto)((UnprocessableEntityObjectResult)response).Value!;
            error.MovimentoId.Should().Be(invalid.Id);
            error.Formula.Should().Be("[Missing]");
            context.ChangeTracker.Clear();
            (await context.Movimenti.SingleAsync(item => item.Id == valid.Id)).Formula.Should().Be("1 + 2");
            (await context.Movimenti.SingleAsync(item => item.Id == invalid.Id)).Formula.Should().Be("[Missing]");
        }

        [Fact]
        public async Task Consolidate_WhenDatabaseWriteFails_RollsBackAllChanges()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using FinanceContext context = await CreateContext(connection);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank" };
            context.Movimenti.AddRange(CreateMovement(conto, today.AddDays(-2), "Primo", "1 + 2"), CreateMovement(conto, today.AddDays(-1), "Errore scrittura", "2 + 3"));
            await context.SaveChangesAsync();
            await context.Database.ExecuteSqlRawAsync("CREATE TRIGGER FailConsolidation BEFORE UPDATE ON Movimenti WHEN OLD.Description = 'Errore scrittura' BEGIN SELECT RAISE(ABORT, 'Errore simulato'); END;");
            MovimentoController controller = CreateController(context);

            // Act
            Func<Task> action = async () => await controller.Consolidate();

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
            context.ChangeTracker.Clear();
            (await context.Movimenti.OrderBy(item => item.Date).Select(item => item.Formula).ToListAsync()).Should().Equal("1 + 2", "2 + 3");
        }

        [Fact]
        public async Task Consolidate_WhenCardResetsAndBankDebitDependOnHistory_PreservesCalculatedAmounts()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using FinanceContext context = await CreateContext(connection);
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly month = new DateOnly(today.Year, today.Month, 1).AddMonths(-3);
            var card = new Conto { Id = Guid.NewGuid(), Name = "HelloCard" };
            var bank = new Conto { Id = Guid.NewGuid(), Name = "HelloBank" };
            context.ParametriConto.Add(new ParametroConto { Id = Guid.NewGuid(), Conto = card, Name = "ChiusuraCiclo", DisplayName = "Chiusura", Type = TipoParametroConto.Intero, Value = 21m });
            context.VociRicorrenti.Add(new VoceRicorrente { Id = Guid.NewGuid(), Name = "Spesa", DisplayName = "Spesa", Value = 100m });
            Movimento spending = CreateMovement(card, month.AddDays(9), "Spesa", "[Spesa]");
            Movimento reset = CreateMovement(card, month.AddMonths(1).AddDays(5), "Ripristino", "-[HelloCard.SaldoUltimoCicloChiuso]");
            Movimento debit = CreateMovement(bank, month.AddMonths(2).AddDays(4), "Addebito", "-[HelloCard.SaldoUltimoCicloChiuso]");
            context.Movimenti.AddRange(spending, reset, CreateMovement(card, month.AddMonths(1).AddDays(9), "Spesa successiva", "30.00"), debit);
            await context.SaveChangesAsync();
            MovimentoController controller = CreateController(context);

            // Act
            IActionResult response = await controller.Consolidate();

            // Assert
            ((ConsolidamentoMovimentiDto)((OkObjectResult)response).Value!).ConsolidatedCount.Should().Be(3);
            context.ChangeTracker.Clear();
            (await context.Movimenti.SingleAsync(item => item.Id == spending.Id)).Formula.Should().Be("100.00");
            (await context.Movimenti.SingleAsync(item => item.Id == reset.Id)).Formula.Should().Be("-100.00");
            (await context.Movimenti.SingleAsync(item => item.Id == debit.Id)).Formula.Should().Be("-30.00");
        }

        private static Movimento CreateMovement(Conto conto, DateOnly date, string description, string formula)
            => new() { Id = Guid.NewGuid(), Conto = conto, Date = date, Description = description, Formula = formula };

        private static async Task<FinanceContext> CreateContext(SqliteConnection connection)
        {
            var context = new FinanceContext(new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options);
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        private static MovimentoController CreateController(FinanceContext context)
        {
            var cache = new FormulaEvaluationCache();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var conti = new ContoRepository(context, persistence, cache);
            var movimenti = new MovimentoRepository(context, persistence, cache);
            var parametri = new ParametroContoRepository(context, persistence, cache);
            var voci = new VoceRicorrenteRepository(context, persistence, cache);
            var resolver = new FormulaResolver(voci, conti, parametri, cache);
            var evaluator = new FormulaEvaluator(resolver, conti, movimenti, parametri, cache);

            return new MovimentoController(new MovimentoService(conti, movimenti, evaluator, persistence));
        }
    }
}

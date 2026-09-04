using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Application
{
    public class FormulaAccountEvaluationTests
    {
        [Fact]
        public async Task SaldoUltimoCicloChiusoIncludesPreviousResetAndCurrentCycleMovements()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var contoRepository = new ContoRepository(context, persistence);
            var movimentoRepository = new MovimentoRepository(context, persistence);
            var parametroRepository = new ParametroContoRepository(context, persistence);
            var voceRepository = new VoceRicorrenteRepository(context, persistence);
            Conto conto = await contoRepository.CreateConto("HelloCard", "Hello Card", 0m);
            await parametroRepository.Replace(conto.Id, null, "ChiusuraCiclo", TipoParametroConto.Intero, [
                new ParametroConto { DisplayName = "Chiusura ciclo", Value = 21m },
            ]);
            await movimentoRepository.Create(conto.Id, new DateOnly(2026, 1, 10), "Spesa gennaio", "100.00");
            await movimentoRepository.Create(conto.Id, new DateOnly(2026, 2, 6), "Ripristino plafond",
                "-[HelloCard.SaldoUltimoCicloChiuso]");
            await movimentoRepository.Create(conto.Id, new DateOnly(2026, 2, 10), "Spesa febbraio", "30.00");
            var evaluator = new FormulaEvaluator(
                new FormulaResolver(voceRepository, contoRepository, parametroRepository),
                contoRepository,
                movimentoRepository,
                parametroRepository);

            // Act
            FormulaEvaluationResult january = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", new DateOnly(2026, 2, 6));
            FormulaEvaluationResult february = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", new DateOnly(2026, 2, 22));

            // Assert
            january.Value.Should().Be(100m);
            february.Value.Should().Be(30m);
        }

        [Fact]
        public async Task SaldoUltimoCicloChiusoUsesPreviousMonthBeforeClosingDay()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var contoRepository = new ContoRepository(context, persistence);
            var movimentoRepository = new MovimentoRepository(context, persistence);
            var parametroRepository = new ParametroContoRepository(context, persistence);
            var voceRepository = new VoceRicorrenteRepository(context, persistence);
            Conto conto = await contoRepository.CreateConto("HelloCard", "Hello Card", 0m);
            await parametroRepository.Replace(conto.Id, null, "ChiusuraCiclo", TipoParametroConto.Intero, [
                new ParametroConto { DisplayName = "Chiusura ciclo", Value = 21m },
            ]);
            await movimentoRepository.Create(conto.Id, new DateOnly(2026, 1, 21), "Spesa gennaio", "100.00");
            await movimentoRepository.Create(conto.Id, new DateOnly(2026, 2, 10), "Spesa febbraio", "30.00");
            var evaluator = new FormulaEvaluator(
                new FormulaResolver(voceRepository, contoRepository, parametroRepository),
                contoRepository,
                movimentoRepository,
                parametroRepository);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", new DateOnly(2026, 2, 20));

            // Assert
            result.Value.Should().Be(100m);
        }
    }
}

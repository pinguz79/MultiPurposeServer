using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Moq;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Application
{
    public class CartaASaldoServiceTests
    {
        [Fact]
        public async Task ConfigureCreatesAtomicBootstrapAndRepeatedRequestIsIdempotent()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var contoRepository = new ContoRepository(context, persistence);
            await contoRepository.CreateConto("HelloBank", "Hello Bank", 0m);
            await contoRepository.CreateConto("HelloCard", "Hello Card", 0m);
            var parametroRepository = new ParametroContoRepository(context, persistence);
            var parametroService = new ParametroContoService(parametroRepository, contoRepository, persistence);
            var categoriaRepository = new CategoriaRepository(context, persistence);
            var categoriaService = new CategoriaService(categoriaRepository, persistence);
            var movimentoRepository = new MovimentoRepository(context, persistence);
            var voceRepository = new VoceRicorrenteRepository(context, persistence);
            var formulaEvaluator = new FormulaEvaluator(
                new FormulaResolver(voceRepository, contoRepository, parametroRepository),
                contoRepository,
                movimentoRepository,
                parametroRepository);
            var pianificazioneService = new PianificazioneService(
                contoRepository,
                movimentoRepository,
                new PianificazioneRepository(context, persistence),
                voceRepository,
                formulaEvaluator,
                categoriaService,
                persistence);
            var service = new CartaASaldoService(
                contoRepository,
                parametroService,
                categoriaService,
                pianificazioneService,
                new CartaASaldoRepository(context),
                persistence);
            var request = new ConfigureCartaASaldoRequest(
                5_000m,
                0.10m,
                21,
                5,
                6,
                "HelloBank",
                new DateOnly(2026, 9, 4),
                new DateOnly(2026, 12, 31));

            // Act
            CartaASaldoDto created = await service.Configure("HelloCard", request);
            int movementsAfterCreate = await context.Movimenti.CountAsync();
            CartaASaldoDto repeated = await service.Configure("HelloCard", request);

            // Assert
            created.Created.Should().BeTrue();
            repeated.Created.Should().BeFalse();
            repeated.PianificazioneAddebitoId.Should().Be(created.PianificazioneAddebitoId);
            repeated.PianificazioneRipristinoId.Should().Be(created.PianificazioneRipristinoId);
            (await context.ParametriConto.CountAsync()).Should().Be(5);
            (await context.Pianificazioni.CountAsync()).Should().Be(2);
            (await context.CorrelazioniPianificazioni.CountAsync()).Should().Be(1);
            (await context.Movimenti.CountAsync()).Should().Be(movementsAfterCreate);
            (await context.Categorie.SingleAsync()).Name.Should().Be("Tecnico");
        }

        [Fact]
        public async Task ConfigureRejectsResetBeforeDebit()
        {
            // Arrange
            var service = new CartaASaldoService(
                Mock.Of<IContoRepository>(),
                Mock.Of<IParametroContoService>(),
                Mock.Of<ICategoriaService>(),
                Mock.Of<IPianificazioneService>(),
                Mock.Of<ICartaASaldoRepository>(),
                null!);
            var request = new ConfigureCartaASaldoRequest(
                5_000m,
                0.10m,
                21,
                6,
                5,
                "HelloBank",
                new DateOnly(2026, 9, 4),
                new DateOnly(2036, 12, 31));

            // Act
            var action = () => service.Configure("HelloCard", request);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
    }
}

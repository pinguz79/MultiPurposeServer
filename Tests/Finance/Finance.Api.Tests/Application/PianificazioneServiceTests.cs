using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Tests.Application
{
    public class PianificazioneServiceTests
    {
        [Theory]
        [InlineData(30)]
        [InlineData(28)]
        public async Task DanceSchoolEveryFourWeeksCreatesTenWednesdays(int startDay)
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var categoriaService = new CategoriaService(new CategoriaRepository(context, persistence), persistence);
            Categoria ballo = await categoriaService.Create("Ballo", "Ballo");
            var contoRepository = new ContoRepository(context, persistence);
            Conto conto = await contoRepository.CreateConto("HelloBank", "Hello Bank", 0m);
            var voceRepository = new VoceRicorrenteRepository(context, persistence);
            await voceRepository.Replace(null, "ScuolaBallo", [new VoceRicorrente { DisplayName = "Scuola Ballo", Value = -50m, CategoriaId = ballo.Id }]);
            var repository = new PianificazioneRepository(context, persistence);
            var evaluator = new FormulaEvaluator(new FormulaResolver(voceRepository, contoRepository, new ParametroContoRepository(context, persistence)));
            var service = new PianificazioneService(contoRepository, new MovimentoRepository(context, persistence), repository, voceRepository, evaluator, categoriaService, persistence);
            var request = new CreatePianificazioneRequest(conto.Name, "Scuola Ballo", "Scuola Ballo", "[ScuolaBallo]",
                new DateOnly(2026, 9, startDay), new DateOnly(2027, 6, 30), 4, null, false, ModalitaCategoria.Ereditata, null, "ScuolaBallo", FrequenzaPeriodicita.Settimanale, DayOfWeek.Wednesday);
            DateOnly[] expected = [
                new(2026, 9, 30), new(2026, 10, 28), new(2026, 11, 25), new(2026, 12, 23), new(2027, 1, 20),
                new(2027, 2, 17), new(2027, 3, 17), new(2027, 4, 14), new(2027, 5, 12), new(2027, 6, 9),
            ];

            // Act
            var preview = await service.Preview(request);
            await using IApplicationOperation operation = await service.BeginOperation();
            var created = await service.Create(request);
            await operation.Complete();
            context.ChangeTracker.Clear();

            // Assert
            preview.IsValid.Should().BeTrue();
            preview.Occurrences.Select(item => item.Date).Should().Equal(expected);
            preview.Occurrences.Sum(item => item.Value).Should().Be(-500m);
            preview.Occurrences.Should().OnlyContain(item => item.Category != null && item.Category.Name == ballo.Name);
            created.MovimentoIds.Should().HaveCount(10);
            Periodicita periodicita = await context.Periodicita.SingleAsync();
            periodicita.Frequenza.Should().Be(FrequenzaPeriodicita.Settimanale);
            periodicita.Intervallo.Should().Be(4);
            periodicita.GiornoSettimana.Should().Be(DayOfWeek.Wednesday);
            (await context.Movimenti.OrderBy(item => item.Date).Select(item => item.Date).ToListAsync()).Should().Equal(expected);
            (await repository.GetOrCreateWeeklyPeriodicity(4, DayOfWeek.Wednesday)).Id.Should().Be(periodicita.Id);
        }

        [Theory]
        [InlineData(1, DayOfWeek.Wednesday, null, false, true)]
        [InlineData(0, DayOfWeek.Wednesday, null, false, false)]
        [InlineData(1, null, null, false, false)]
        [InlineData(1, (DayOfWeek)7, null, false, false)]
        [InlineData(1, DayOfWeek.Wednesday, 15, false, false)]
        [InlineData(1, DayOfWeek.Wednesday, null, true, false)]
        [InlineData(int.MaxValue, DayOfWeek.Wednesday, null, false, true)]
        public async Task WeeklyPreviewValidatesParameters(int interval, DayOfWeek? weekday, int? dayOfMonth, bool endOfMonth, bool valid)
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var categoriaService = new CategoriaService(new CategoriaRepository(context, persistence), persistence);
            Categoria ballo = await categoriaService.Create("Ballo", "Ballo");
            var contoRepository = new ContoRepository(context, persistence);
            Conto conto = await contoRepository.CreateConto("HelloBank", "Hello Bank", 0m);
            var voceRepository = new VoceRicorrenteRepository(context, persistence);
            await voceRepository.Replace(null, "ScuolaBallo", [new VoceRicorrente { DisplayName = "Scuola Ballo", Value = -50m, CategoriaId = ballo.Id }]);
            var repository = new PianificazioneRepository(context, persistence);
            var evaluator = new FormulaEvaluator(new FormulaResolver(voceRepository, contoRepository, new ParametroContoRepository(context, persistence)));
            var service = new PianificazioneService(contoRepository, new MovimentoRepository(context, persistence), repository, voceRepository, evaluator, categoriaService, persistence);
            var request = new CreatePianificazioneRequest(conto.Name, "Scuola Ballo", "Scuola Ballo", "[ScuolaBallo]",
                new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 7), interval, dayOfMonth, endOfMonth,
                Frequency: FrequenzaPeriodicita.Settimanale, DayOfWeek: weekday);

            // Act
            var preview = await service.Preview(request);

            // Assert
            preview.IsValid.Should().Be(valid);
            preview.OccurrenceCount.Should().Be(valid ? interval == 1 ? 2 : 1 : 0);
            context.Pianificazioni.Should().BeEmpty();
        }

        [Fact]
        public async Task InheritedCategoryIsResolvedForEveryOccurrenceAndCopiedToMovements()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var categoriaRepository = new CategoriaRepository(context, persistence);
            var categoriaService = new CategoriaService(categoriaRepository, persistence);
            Categoria casa = await categoriaService.Create("casa", "Casa");
            Categoria lavoro = await categoriaService.Create("lavoro", "Lavoro");
            var contoRepository = new ContoRepository(context, persistence);
            Conto conto = await contoRepository.CreateConto("HelloBank", "Hello Bank", 0m);
            var voceRepository = new VoceRicorrenteRepository(context, persistence);
            await voceRepository.Replace(null, "Rimborso", [
                new VoceRicorrente
                {
                    DisplayName = "Rimborso casa",
                    Value = 10m,
                    ValidTo = new DateOnly(2026, 8, 31),
                    CategoriaId = casa.Id,
                },
                new VoceRicorrente
                {
                    DisplayName = "Rimborso lavoro",
                    Value = 20m,
                    ValidFrom = new DateOnly(2026, 9, 1),
                    CategoriaId = lavoro.Id,
                },
            ]);
            var evaluator = new FormulaEvaluator(new FormulaResolver(
                voceRepository,
                contoRepository,
                new ParametroContoRepository(context, persistence)));
            var service = new PianificazioneService(
                contoRepository,
                new MovimentoRepository(context, persistence),
                new PianificazioneRepository(context, persistence),
                voceRepository,
                evaluator,
                categoriaService,
                persistence);
            var request = new CreatePianificazioneRequest(
                conto.Name,
                "Rimborsi",
                "Rimborso",
                "[Rimborso]",
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 9, 30),
                1,
                15,
                false,
                ModalitaCategoria.Ereditata,
                null,
                "Rimborso");

            // Act
            var preview = await service.Preview(request);
            await using IApplicationOperation operation = await service.BeginOperation();
            var created = await service.Create(request);
            await operation.Complete();
            context.ChangeTracker.Clear();

            // Assert
            preview.Occurrences.Select(item => item.Category?.Name).Should().ContainInOrder(casa.Name, lavoro.Name);
            created.MovimentoIds.Should().HaveCount(2);
            Pianificazione pianificazione = await context.Pianificazioni.SingleAsync();
            pianificazione.ModalitaCategoria.Should().Be(ModalitaCategoria.Ereditata);
            pianificazione.VoceRicorrenteCategoriaName.Should().Be("Rimborso");
            (await context.Movimenti.OrderBy(item => item.Date).Select(item => item.CategoriaId).ToListAsync())
                .Should().ContainInOrder(casa.Id, lavoro.Id);
        }
    }
}

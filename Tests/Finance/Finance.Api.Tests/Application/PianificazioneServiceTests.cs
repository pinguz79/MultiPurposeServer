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

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
    public class CategoriaServiceTests
    {
        [Fact]
        public async Task DeleteRequiresConfirmationAndThenRemovesEveryReference()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var repository = new CategoriaRepository(context, persistence);
            var service = new CategoriaService(repository, persistence);
            Categoria categoria = await service.Create("casa", "Casa");
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank", DisplayName = "Hello Bank" };
            var periodicita = new Periodicita { Id = Guid.NewGuid(), Frequenza = FrequenzaPeriodicita.Mensile, Intervallo = 1, GiornoMese = 1 };
            context.AddRange(
                conto,
                periodicita,
                new VoceRicorrente { Id = Guid.NewGuid(), Name = "Affitto", DisplayName = "Affitto", CategoriaId = categoria.Id },
                new Pianificazione
                {
                    Id = Guid.NewGuid(),
                    ContoId = conto.Id,
                    PeriodicitaId = periodicita.Id,
                    Description = "Affitto",
                    MovimentoDescription = "Affitto",
                    MovimentoFormula = "0.00",
                    ModalitaCategoria = ModalitaCategoria.Esplicita,
                    CategoriaId = categoria.Id,
                },
                new Movimento
                {
                    Id = Guid.NewGuid(),
                    ContoId = conto.Id,
                    Date = new DateOnly(2026, 8, 31),
                    Description = "Affitto",
                    Formula = "0.00",
                    CategoriaId = categoria.Id,
                });
            await context.SaveChangesAsync();

            // Act
            CategoriaDeleteResult preview = await service.Delete(categoria.Name, false);
            CategoriaDeleteResult result = await service.Delete(categoria.Name, true);
            context.ChangeTracker.Clear();

            // Assert
            preview.Deleted.Should().BeFalse();
            preview.Usage.Should().Be(new CategoriaUsage(1, 1, 1));
            result.Deleted.Should().BeTrue();
            (await context.Categorie.CountAsync()).Should().Be(0);
            (await context.VociRicorrenti.SingleAsync()).CategoriaId.Should().BeNull();
            (await context.Movimenti.SingleAsync()).CategoriaId.Should().BeNull();
            Pianificazione pianificazione = await context.Pianificazioni.SingleAsync();
            pianificazione.CategoriaId.Should().BeNull();
            pianificazione.ModalitaCategoria.Should().Be(ModalitaCategoria.Nessuna);
        }
    }
}

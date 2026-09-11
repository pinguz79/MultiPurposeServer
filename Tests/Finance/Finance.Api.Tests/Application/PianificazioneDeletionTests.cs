using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Application
{
    public class PianificazioneDeletionTests
    {
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public async Task DeleteOnlyAffectsSelectedPlanningAndLinkedMovements(bool deleteMovimenti, bool commit)
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var repository = new PianificazioneRepository(context, persistence);
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank" };
            var periodicita = new Periodicita { Id = Guid.NewGuid(), Frequenza = FrequenzaPeriodicita.Mensile, Intervallo = 1, GiornoMese = 11 };
            var selected = new Pianificazione { Id = Guid.NewGuid(), Conto = conto, Periodicita = periodicita, Description = "Rateo Spese" };
            var other = new Pianificazione { Id = Guid.NewGuid(), Conto = conto, Periodicita = periodicita, Description = "Altra pianificazione" };
            var linked = new Movimento { Id = Guid.NewGuid(), Conto = conto, Pianificazione = selected, Formula = "-170,27" };
            var detached = new Movimento { Id = Guid.NewGuid(), Conto = conto, Formula = "-170,27" };
            var otherMovement = new Movimento { Id = Guid.NewGuid(), Conto = conto, Pianificazione = other, Formula = "-170,27" };
            context.AddRange(linked, detached, otherMovement);
            context.CorrelazioniPianificazioni.Add(new CorrelazionePianificazione { Id = Guid.NewGuid(), PianificazioneA = selected, PianificazioneB = other });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            Pianificazione target = (await repository.Get(selected.Id))!;

            // Act
            await using (var transaction = await persistence.BeginTransaction())
            {
                await repository.Delete(target, deleteMovimenti);
                if (commit)
                {
                    await transaction.Commit();
                }
            }
            context.ChangeTracker.Clear();

            // Assert
            (await context.Pianificazioni.AnyAsync(item => item.Id == selected.Id)).Should().Be(!commit);
            (await repository.Get(other.Id)).Should().NotBeNull();
            (await context.Periodicita.CountAsync()).Should().Be(1);
            (await context.CorrelazioniPianificazioni.CountAsync()).Should().Be(commit ? 0 : 1);
            (await context.Movimenti.AnyAsync(item => item.Id == detached.Id)).Should().BeTrue();
            (await context.Movimenti.AnyAsync(item => item.Id == otherMovement.Id)).Should().BeTrue();
            Movimento? remaining = await context.Movimenti.SingleOrDefaultAsync(item => item.Id == linked.Id);
            if (deleteMovimenti && commit)
            {
                remaining.Should().BeNull();
            }
            else
            {
                remaining.Should().NotBeNull();
                remaining!.PianificazioneId.Should().Be(commit ? null : selected.Id);
            }
        }
    }
}

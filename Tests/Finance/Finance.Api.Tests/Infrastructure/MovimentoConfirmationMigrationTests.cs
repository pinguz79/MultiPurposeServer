using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Finance.Api.Tests.Infrastructure
{
    public class MovimentoConfirmationMigrationTests
    {
        [Fact]
        public async Task Migrate_ExistingAndNewMovementsDefaultToUnconfirmed()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using var context = new FinanceContext(new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options);
            await context.GetService<IMigrator>().MigrateAsync("20260914061735_AddNaturaMovimento");
            var conto = new Conto { Id = Guid.NewGuid(), Name = "Test", DisplayName = "Test" };
            context.Conti.Add(conto);
            await context.SaveChangesAsync();
            foreach (DateOnly date in new[] { new DateOnly(2026, 9, 22), new DateOnly(2026, 9, 23), new DateOnly(2026, 9, 24) })
            {
                await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Movimenti (Id, ContoId, Date, Description, Formula, Natura) VALUES ({Guid.NewGuid()}, {conto.Id}, {date}, {"Storico"}, {"12.00"}, {0})");
            }

            // Act
            await context.Database.MigrateAsync();
            await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Movimenti (Id, ContoId, Date, Description, Formula, Natura) VALUES ({Guid.NewGuid()}, {conto.Id}, {new DateOnly(2026, 9, 25)}, {"Nuovo"}, {"5.00"}, {0})");

            // Assert
            (await context.Movimenti.Where(item => item.Description == "Storico").ToListAsync()).Should().HaveCount(3).And.OnlyContain(item => !item.IsConfirmed && item.Formula == "12.00");
            context.Database.HasPendingModelChanges().Should().BeFalse();
            (await context.Movimenti.SingleAsync(item => item.Description == "Nuovo")).IsConfirmed.Should().BeFalse();
        }
    }
}

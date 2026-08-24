using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Finance.DataModelTests
{
    public class FinanceContextTests
    {
        [Fact]
        public async Task InitialBalanceRoundTripUsesIntegerMinorUnits()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloBank", DisplayName = "Hello Bank", InitialBalance = 3081.69m };

            // Act
            context.Conti.Add(conto);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var persistedMinorUnits = Convert.ToInt64(await context.Database.SqlQueryRaw<long>("SELECT InitialBalance AS Value FROM Conti").SingleAsync());
            var reloadedConto = await context.Conti.SingleAsync();

            // Assert
            persistedMinorUnits.Should().Be(308169);
            reloadedConto.InitialBalance.Should().Be(3081.69m);
            reloadedConto.Balance.Should().Be(3081.69m);
        }

        [Fact]
        public async Task MinorUnitsMigrationPreservesExistingBalance()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE "Conti" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_Conti" PRIMARY KEY,
                        "Name" TEXT COLLATE NOCASE NOT NULL,
                        "DisplayName" TEXT NOT NULL,
                        "InitialBalance" TEXT NOT NULL
                    );
                    CREATE UNIQUE INDEX "IX_Conti_Name" ON "Conti" ("Name");
                    CREATE TABLE "__EFMigrationsHistory" (
                        "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                        "ProductVersion" TEXT NOT NULL
                    );
                    INSERT INTO "Conti" ("Id", "Name", "DisplayName", "InitialBalance")
                    VALUES ('FD54970C-41B9-4060-88C3-37C0133ECBBE', 'HelloBank', 'Hello Bank', '308169.0');
                    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES ('20260821153903_InitialCreation', '10.0.10');
                    """;
                await command.ExecuteNonQueryAsync();
            }
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);

            // Act
            await context.Database.MigrateAsync();
            var storageType = await context.Database.SqlQueryRaw<string>("SELECT typeof(InitialBalance) AS Value FROM Conti").SingleAsync();
            var persistedMinorUnits = await context.Database.SqlQueryRaw<long>("SELECT InitialBalance AS Value FROM Conti").SingleAsync();
            var conto = await context.Conti.SingleAsync();

            // Assert
            storageType.Should().Be("integer");
            persistedMinorUnits.Should().Be(308169);
            conto.InitialBalance.Should().Be(3081.69m);
            conto.Balance.Should().Be(3081.69m);
        }
    }
}

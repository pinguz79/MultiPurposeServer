using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Transactions;

namespace Finance.Api.Tests.Infrastructure
{
    public class VoceRicorrenteRepositoryTests
    {
        [Fact]
        public async Task ReplaceCanReverseExistingDefinitionsWithoutViolatingUniqueIndex()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<FinanceContext>().UseSqlite(connection).Options;
            await using var context = new FinanceContext(options);
            await context.Database.EnsureCreatedAsync();
            var persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            var repository = new VoceRicorrenteRepository(context, persistence);
            IReadOnlyList<VoceRicorrente> created = await repository.Replace(null, "Affitto",
            [
                new VoceRicorrente { DisplayName = "Eccezione", Value = 650m, Index = 0 },
                new VoceRicorrente { DisplayName = "Regola generale", Value = 600m, Index = 1 },
            ]);
            await using IPersistenceTransaction transaction = await persistence.BeginTransaction();

            // Act
            await repository.Replace("Affitto", "Affitto", [created[1], created[0]]);
            await transaction.Commit();
            IReadOnlyList<VoceRicorrente> result = await repository.GetByName("Affitto");

            // Assert
            result.Select(definition => definition.DisplayName).Should().ContainInOrder("Regola generale", "Eccezione");
        }
    }
}

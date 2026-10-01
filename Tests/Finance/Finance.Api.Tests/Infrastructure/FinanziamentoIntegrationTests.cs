using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Infrastructure
{
    public sealed class FinanziamentoIntegrationTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private FinanceContext _db = null!;
        private FinanziamentoService _service = null!;
        private static readonly DateOnly Today = new(2026, 10, 1);

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();
            _db = new FinanceContext(new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(_connection).Options);
            await _db.Database.MigrateAsync();
            _service = new FinanziamentoService(new FinanziamentoRepository(_db, new EntityFrameworkPersistenceCoordinator<FinanceContext>(_db)));
        }

        [Fact]
        public async Task AlignmentsRoundTripAndLockOnlyContractualData()
        {
            // Arrange
            await _service.Create(Request());
            await _service.SaveAlignment("Findomestic", 7, 19422.49m, Today, true);
            _db.ChangeTracker.Clear();

            // Act
            Finanziamento loan = await _service.Resolve("findomestic");
            var change = () => _service.Update(loan.Name, new UpdateFinanziamentoRequest(Installment: 310m));
            await change.Should().ThrowAsync<FinanziamentoLockedException>();
            await _service.Update(loan.Name, new UpdateFinanziamentoRequest(DisplayName: "Prestito", IsClosed: true));

            // Assert
            loan.Riallineamenti.Single().Principal.Should().Be(19422.49m);
            loan.Installment.Should().Be(304m);
            (await _service.GetAll()).Should().BeEmpty();
            (await _service.GetAll(true)).Should().ContainSingle();
            await _service.Update(loan.Name, new UpdateFinanziamentoRequest(IsClosed: false));
            (await _service.GetAll()).Should().ContainSingle();
            (await _db.Database.SqlQueryRaw<long>("SELECT Principal AS Value FROM RiallineamentiFinanziamenti").SingleAsync()).Should().Be(1942249);
        }

        [Fact]
        public async Task AlignmentUpdateDeleteAndValidationPreservePlan()
        {
            // Arrange
            await _service.Create(Request());
            await _service.SaveAlignment("Findomestic", 7, 19000m, Today, true);

            // Act
            var duplicate = () => _service.SaveAlignment("Findomestic", 7, 18000m, Today, true);
            await duplicate.Should().ThrowAsync<DuplicateNameException>();
            await _service.SaveAlignment("Findomestic", 7, 19422.49m, Today, false);
            var future = () => _service.SaveAlignment("Findomestic", 8, 19000m, new DateOnly(2026, 10, 20), true);
            await future.Should().ThrowAsync<ArgumentException>();
            await _service.DeleteAlignment("Findomestic", 7);
            _db.ChangeTracker.Clear();
            await _service.Update("Findomestic", new UpdateFinanziamentoRequest(Installment: 305m));

            // Assert
            Finanziamento loan = await _service.Resolve("Findomestic");
            loan.Riallineamenti.Should().BeEmpty();
            loan.Installment.Should().Be(305m);
        }

        [Fact]
        public async Task InvalidContractAndDuplicateNameAreRejected()
        {
            // Arrange
            await _service.Create(Request());

            // Act
            var duplicate = () => _service.Create(Request() with { Name = "findomestic" });
            var invalid = () => _service.Create(Request() with { Name = "Other", Installment = 1m });
            var precision = () => _service.Create(Request() with { Name = "Other", Insurance = .001m });
            var date = () => _service.Create(Request() with { Name = "Other", DueDay = 31 });

            // Assert
            await duplicate.Should().ThrowAsync<DuplicateNameException>();
            await invalid.Should().ThrowAsync<ArgumentException>();
            await precision.Should().ThrowAsync<ArgumentException>();
            await date.Should().ThrowAsync<ArgumentException>();
            (await _service.GetAll()).Should().ContainSingle();
        }

        private static CreateFinanziamentoRequest Request() => new("Findomestic", "Findomestic", "Findomestic", 20000m, .1345m, 304m, new DateOnly(2026, 3, 20), 120, Insurance: 25.50m);

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

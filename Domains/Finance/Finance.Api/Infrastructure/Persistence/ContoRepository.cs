using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public sealed class ContoRepository(FinanceContext context, EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : IContoRepository
    {
        public async Task<Conto> Create(string name, string displayName, decimal initialBalance)
        {
            var conto = new Conto
            {
                Id = Guid.NewGuid(),
                Name = name,
                DisplayName = displayName,
                InitialBalance = initialBalance,
            };

            context.Conti.Add(conto);
            await SaveIfRequired();

            return conto;
        }

        public async Task<Conto?> Get(Guid id) => await context.Conti.FirstOrDefaultAsync(conto => conto.Id == id);

        public async Task<IReadOnlyList<Conto>> GetAll() => await context.Conti.ToListAsync();

        public async Task<bool> NameExists(string name) => await context.Conti.AnyAsync(conto => conto.Name.ToUpper() == name.ToUpper());

        public async Task<Conto> Update(Guid id, string? displayName, decimal? initialBalance)
        {
            var conto = await Get(id) ?? throw new KeyNotFoundException($"Conto '{id}' was not found.");

            conto.DisplayName = displayName ?? conto.DisplayName;
            conto.InitialBalance = initialBalance ?? conto.InitialBalance;
            await SaveIfRequired();

            return conto;
        }

        private async Task<int> SaveIfRequired() => persistence.IsTransactionActive ? 0 : await context.SaveChangesAsync();
    }
}

using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class ContoRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache? cache = null) : IContoRepository
    {
        public async Task<Conto> CreateConto(string name, string displayName, decimal initialBalance)
        {
            var conto = new Conto
            {
                Id = Guid.NewGuid(),
                Name = name,
                DisplayName = displayName,
                InitialBalance = initialBalance,
            };

            db.Conti.Add(conto);
            await SaveIfRequired();

            return conto;
        }

        public async Task<Conto?> GetById(Guid id) => await db.Conti.FirstOrDefaultAsync(conto => conto.Id == id);

        public async Task<Conto?> GetByName(string name) => await db.Conti.FirstOrDefaultAsync(conto => conto.Name == name);

        public async Task<IReadOnlyList<Conto>> GetConti() => await db.Conti.ToListAsync();

        public async Task<bool> NameExists(string name) => await db.Conti.AnyAsync(conto => conto.Name.ToUpper() == name.ToUpper());

        public async Task<bool> NameExists(string name, Guid excludedId) => await db.Conti.AnyAsync(conto => conto.Id != excludedId && conto.Name.ToUpper() == name.ToUpper());

        public async Task<Conto> UpdateConto(Guid id, string? name, string? displayName, decimal? initialBalance)
        {
            var conto = await GetById(id) ?? throw new KeyNotFoundException($"Conto '{id}' was not found.");

            conto.Name = name ?? conto.Name;
            conto.DisplayName = displayName ?? conto.DisplayName;
            conto.InitialBalance = initialBalance ?? conto.InitialBalance;
            await SaveIfRequired();

            return conto;
        }

        private async Task<int> SaveIfRequired()
        {
            cache?.Invalidate();

            return persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
        }
    }
}

using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class FinanziamentoRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : IFinanziamentoRepository
    {
        public async Task<IReadOnlyList<Finanziamento>> GetAll(bool includeClosed)
            => await db.Finanziamenti.Where(item => includeClosed || !item.IsClosed).OrderBy(item => item.DisplayName).ThenBy(item => item.Name).ToListAsync();

        public Task<Finanziamento?> GetByName(string name) => db.Finanziamenti.SingleOrDefaultAsync(item => item.Name == name);

        public async Task<Finanziamento> Create(Finanziamento loan)
        {
            db.Finanziamenti.Add(loan);
            await Save();
            return loan;
        }

        public async Task Save()
        {
            if (!persistence.IsTransactionActive)
            {
                await db.SaveChangesAsync();
            }
        }

        public async Task AddAlignment(RiallineamentoFinanziamento alignment)
        {
            db.RiallineamentiFinanziamenti.Add(alignment);
            await Save();
        }

        public async Task RemoveAlignment(RiallineamentoFinanziamento alignment)
        {
            db.RiallineamentiFinanziamenti.Remove(alignment);
            await Save();
        }
    }
}

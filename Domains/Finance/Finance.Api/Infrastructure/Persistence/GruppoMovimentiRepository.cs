using Finance.DataModel;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class GruppoMovimentiRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : IGruppoMovimentiRepository
    {
        public async Task<GruppoMovimenti?> GetById(Guid id) => await db.GruppiMovimenti.FindAsync(id);

        public async Task<GruppoMovimenti> Create(IReadOnlyList<Movimento> movements)
        {
            var group = new GruppoMovimenti { Id = Guid.NewGuid(), Movimenti = movements.ToList() };
            db.GruppiMovimenti.Add(group);
            await Save();
            return group;
        }

        public async Task Remove(Guid id)
        {
            GruppoMovimenti group = await GetById(id) ?? throw new KeyNotFoundException("Gruppo non trovato.");
            foreach (Movimento movement in group.Movimenti.ToList())
            {
                movement.GruppoMovimenti = null;
                movement.GruppoMovimentiId = null;
            }
            db.GruppiMovimenti.Remove(group);
            await Save();
        }

        private async Task Save()
        {
            if (!persistence.IsTransactionActive)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}

using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class PedaggioRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache cache) : IPedaggioRepository
    {
        public async Task<Pedaggio?> GetByMovimento(Guid movimentoId)
            => db.Pedaggi.Local.FirstOrDefault(pedaggio => pedaggio.MovimentoId == movimentoId) ?? await db.Pedaggi.SingleOrDefaultAsync(pedaggio => pedaggio.MovimentoId == movimentoId);

        public async Task<IReadOnlyList<Pedaggio>> GetByMovimenti(IReadOnlyList<Guid> movimentoIds)
            => await db.Pedaggi.Where(pedaggio => movimentoIds.Contains(pedaggio.MovimentoId)).ToListAsync();

        public async Task Save(Pedaggio pedaggio)
        {
            if (pedaggio.Id == Guid.Empty)
            {
                pedaggio.Id = Guid.NewGuid();
                db.Pedaggi.Add(pedaggio);
            }

            cache.Invalidate();
            if (!persistence.IsTransactionActive)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}

using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class CartaRevolvingRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache cache) : ICartaRevolvingRepository
    {
        public async Task<IReadOnlyList<Pianificazione>> GetPlans(string prefix) => await db.Pianificazioni.Where(plan => plan.Description.StartsWith(prefix)).ToListAsync();

        public async Task<bool> HasCorrelation(Guid firstId, Guid secondId)
            => await db.CorrelazioniPianificazioni.AnyAsync(link => (link.PianificazioneAId == firstId && link.PianificazioneBId == secondId) || (link.PianificazioneAId == secondId && link.PianificazioneBId == firstId));

        public Task CreateCorrelation(Guid firstId, Guid secondId)
        {
            db.CorrelazioniPianificazioni.Add(new CorrelazionePianificazione
            {
                Id = Guid.NewGuid(),
                PianificazioneAId = firstId.CompareTo(secondId) < 0 ? firstId : secondId,
                PianificazioneBId = firstId.CompareTo(secondId) < 0 ? secondId : firstId,
            });
            return Task.CompletedTask;
        }

        public async Task Save()
        {
            if (!persistence.IsTransactionActive)
            {
                throw new InvalidOperationException("La configurazione revolving richiede un'operazione attiva.");
            }

            cache.Invalidate();
            // Scrittura intermedia necessaria alle query delle formule, senza commit della transazione.
            await db.SaveChangesAsync();
        }
    }
}

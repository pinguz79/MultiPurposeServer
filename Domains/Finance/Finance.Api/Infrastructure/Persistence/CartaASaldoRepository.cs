using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

namespace Finance.Api.Infrastructure.Persistence
{
    public class CartaASaldoRepository(FinanceContext db, FormulaEvaluationCache? cache = null) : ICartaASaldoRepository
    {
        public Task<CorrelazionePianificazione> CreateCorrelation(Guid firstId, Guid secondId)
        {
            (Guid pianificazioneAId, Guid pianificazioneBId) = Canonicalize(firstId, secondId);
            var correlation = new CorrelazionePianificazione
            {
                Id = Guid.NewGuid(),
                PianificazioneAId = pianificazioneAId,
                PianificazioneBId = pianificazioneBId,
            };

            db.CorrelazioniPianificazioni.Add(correlation);

            return Task.FromResult(correlation);
        }

        public async Task<CorrelazionePianificazione?> GetCorrelation(Guid firstId, Guid secondId)
        {
            (Guid pianificazioneAId, Guid pianificazioneBId) = Canonicalize(firstId, secondId);

            return await db.CorrelazioniPianificazioni.FirstOrDefaultAsync(item => item.PianificazioneAId == pianificazioneAId
                && item.PianificazioneBId == pianificazioneBId);
        }

        public async Task<IReadOnlyList<Pianificazione>> GetPlans(string debitDescription, string resetDescription)
            => await db.Pianificazioni.Where(item => item.Description == debitDescription || item.Description == resetDescription).ToListAsync();

        public async Task Save()
        {
            cache?.Invalidate();
            await db.SaveChangesAsync();
        }

        private static (Guid A, Guid B) Canonicalize(Guid firstId, Guid secondId)
            => firstId.CompareTo(secondId) < 0 ? (firstId, secondId) : (secondId, firstId);
    }
}

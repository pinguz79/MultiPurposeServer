using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class PianificazioneRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : IPianificazioneRepository
    {
        public async Task<Pianificazione> Create(
            Guid contoId,
            Guid periodicitaId,
            string description,
            string movimentoDescription,
            string movimentoFormula,
            DateOnly validFrom,
            DateOnly validTo)
        {
            var pianificazione = new Pianificazione
            {
                Id = Guid.NewGuid(),
                ContoId = contoId,
                PeriodicitaId = periodicitaId,
                Description = description,
                MovimentoDescription = movimentoDescription,
                MovimentoFormula = movimentoFormula,
                ValidFrom = validFrom,
                ValidTo = validTo,
            };

            db.Pianificazioni.Add(pianificazione);
            await SaveIfRequired();

            return pianificazione;
        }

        public async Task<Periodicita> GetOrCreateMonthlyPeriodicity(int interval, int? dayOfMonth, bool endOfMonth)
        {
            Periodicita? periodicita = await db.Periodicita.FirstOrDefaultAsync(item => item.Frequenza == FrequenzaPeriodicita.Mensile
                && item.Intervallo == interval
                && item.GiornoSettimana == null
                && item.SettimanaMese == null
                && item.GiornoMese == dayOfMonth
                && item.MeseAnno == null
                && item.FineMese == endOfMonth);

            if (periodicita is not null)
            {
                return periodicita;
            }

            periodicita = new Periodicita
            {
                Id = Guid.NewGuid(),
                Frequenza = FrequenzaPeriodicita.Mensile,
                Intervallo = interval,
                GiornoMese = dayOfMonth,
                FineMese = endOfMonth,
            };

            db.Periodicita.Add(periodicita);
            await SaveIfRequired();

            return periodicita;
        }

        private async Task<int> SaveIfRequired() => persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
    }
}

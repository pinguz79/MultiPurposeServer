using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class PianificazioneRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache? cache = null) : IPianificazioneRepository
    {
        public async Task<Pianificazione?> Get(Guid id) => await db.Pianificazioni.SingleOrDefaultAsync(item => item.Id == id);

        public async Task<IReadOnlyList<Pianificazione>> GetList(string? contoName) => await db.Pianificazioni
            .Where(item => contoName == null || item.Conto.Name == contoName).OrderBy(item => item.Description).ThenBy(item => item.Id).ToListAsync();

        public async Task Delete(Pianificazione pianificazione, bool deleteMovimenti)
        {
            if (deleteMovimenti)
            {
                db.Movimenti.RemoveRange(pianificazione.Movimenti);
            }

            db.Pianificazioni.Remove(pianificazione);
            await SaveIfRequired();
        }

        public async Task<Pianificazione> Create(
            Guid contoId,
            Guid periodicitaId,
            string description,
            string movimentoDescription,
            string movimentoFormula,
            DateOnly validFrom,
            DateOnly validTo,
            ModalitaCategoria modalitaCategoria,
            string? voceRicorrenteCategoriaName,
            Guid? categoriaId)
        {
            var pianificazione = new Pianificazione
            {
                Id = Guid.NewGuid(),
                ContoId = contoId,
                PeriodicitaId = periodicitaId,
                Description = description,
                MovimentoDescription = movimentoDescription,
                MovimentoFormula = movimentoFormula,
                ModalitaCategoria = modalitaCategoria,
                VoceRicorrenteCategoriaName = voceRicorrenteCategoriaName,
                CategoriaId = categoriaId,
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

        public async Task<Periodicita> GetOrCreateWeeklyPeriodicity(int interval, DayOfWeek dayOfWeek)
        {
            Periodicita? periodicita = await db.Periodicita.FirstOrDefaultAsync(item => item.Frequenza == FrequenzaPeriodicita.Settimanale
                && item.Intervallo == interval && item.GiornoSettimana == dayOfWeek && item.SettimanaMese == null
                && item.GiornoMese == null && item.MeseAnno == null && !item.FineMese);

            if (periodicita is not null)
            {
                return periodicita;
            }

            periodicita = new Periodicita
            {
                Id = Guid.NewGuid(),
                Frequenza = FrequenzaPeriodicita.Settimanale,
                Intervallo = interval,
                GiornoSettimana = dayOfWeek,
            };
            db.Periodicita.Add(periodicita);
            await SaveIfRequired();

            return periodicita;
        }

        private async Task<int> SaveIfRequired()
        {
            cache?.Invalidate();

            return persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
        }
    }
}

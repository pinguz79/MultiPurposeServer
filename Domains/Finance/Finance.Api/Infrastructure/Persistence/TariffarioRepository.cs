using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class TariffarioRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache cache) : ITariffarioRepository
    {
        public async Task<IReadOnlyList<Casello>> GetCaselli() => await db.Caselli.OrderBy(casello => casello.Name).ToListAsync();

        public Task<Casello?> GetCasello(Guid id) => db.Caselli.SingleOrDefaultAsync(casello => casello.Id == id);

        public Task<bool> NameExists(string name, Guid? excludingId = null) => db.Caselli.AnyAsync(casello => casello.Name == name && (excludingId == null || casello.Id != excludingId));

        public async Task<Casello> SaveCasello(Casello casello, string name)
        {
            if (casello.Id == Guid.Empty)
            {
                casello.Id = Guid.NewGuid();
                db.Caselli.Add(casello);
            }

            casello.Name = name;
            await SaveIfRequired();
            return casello;
        }

        public async Task<IReadOnlyList<TariffaTratta>> GetTariffe(Guid? caselloAId = null, Guid? caselloBId = null)
        {
            await db.TariffeTratte.Where(tariffa => (caselloAId == null || tariffa.CaselloAId == caselloAId) && (caselloBId == null || tariffa.CaselloBId == caselloBId)).LoadAsync();
            // Anche le tariffe appena create nell'operazione corrente devono essere risolvibili prima del commit.
            return [.. db.TariffeTratte.Local.Where(tariffa => (caselloAId == null || tariffa.CaselloAId == caselloAId) && (caselloBId == null || tariffa.CaselloBId == caselloBId)).OrderBy(tariffa => tariffa.Index)];
        }

        public async Task<IReadOnlyList<TariffaTratta>> Replace(Guid caselloAId, Guid caselloBId, IReadOnlyList<TariffaTratta> definitions)
        {
            IReadOnlyList<TariffaTratta> existing = await GetTariffe(caselloAId, caselloBId);
            for (var index = 0; index < definitions.Count; index++)
            {
                TariffaTratta target = existing.FirstOrDefault(tariffa => tariffa.Index == index) ?? new TariffaTratta
                {
                    Id = Guid.NewGuid(), CaselloAId = caselloAId, CaselloBId = caselloBId, Index = index,
                };
                if (!existing.Contains(target))
                {
                    db.TariffeTratte.Add(target);
                }

                target.Formula = definitions[index].Formula;
                target.ValidFrom = definitions[index].ValidFrom;
                target.ValidTo = definitions[index].ValidTo;
            }

            db.TariffeTratte.RemoveRange(existing.Where(tariffa => tariffa.Index >= definitions.Count));
            await SaveIfRequired();
            return [.. db.TariffeTratte.Local.Where(tariffa => tariffa.CaselloAId == caselloAId && tariffa.CaselloBId == caselloBId).OrderBy(tariffa => tariffa.Index)];
        }

        private async Task SaveIfRequired()
        {
            cache.Invalidate();
            if (!persistence.IsTransactionActive)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}

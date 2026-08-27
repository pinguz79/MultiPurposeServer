using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class VoceRicorrenteRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : IVoceRicorrenteRepository
    {
        public async Task Delete(string name)
        {
            IReadOnlyList<VoceRicorrente> definitions = await GetByName(name);

            if (definitions.Count == 0)
            {
                throw new KeyNotFoundException($"VoceRicorrente '{name}' was not found.");
            }

            db.VociRicorrenti.RemoveRange(definitions);
            await SaveIfRequired();
        }

        public async Task<IReadOnlyList<VoceRicorrente>> GetAll()
            => await db.VociRicorrenti.OrderBy(voce => voce.Name).ThenBy(voce => voce.Index).ToListAsync();

        public async Task<IReadOnlyList<VoceRicorrente>> GetByName(string name)
            => await db.VociRicorrenti.Where(voce => voce.Name == name).OrderBy(voce => voce.Index).ToListAsync();

        public async Task<bool> NameExists(string name) => await db.VociRicorrenti.AnyAsync(voce => voce.Name == name);

        public async Task<IReadOnlyList<VoceRicorrente>> Replace(
            string? currentName,
            string name,
            IReadOnlyList<VoceRicorrente> definitions)
        {
            IReadOnlyList<VoceRicorrente> persisted = currentName is null ? [] : await GetByName(currentName);
            Dictionary<Guid, VoceRicorrente> persistedById = persisted.ToDictionary(voce => voce.Id);
            HashSet<Guid> retainedIds = [.. definitions.Where(voce => voce.Id != Guid.Empty).Select(voce => voce.Id)];

            for (var index = 0; index < persisted.Count; index++)
            {
                persisted[index].Index = -index - 1;
            }

            if (persisted.Count > 0)
            {
                await db.SaveChangesAsync();
            }

            db.VociRicorrenti.RemoveRange(persisted.Where(voce => !retainedIds.Contains(voce.Id)));
            var result = new List<VoceRicorrente>(definitions.Count);

            for (var index = 0; index < definitions.Count; index++)
            {
                VoceRicorrente source = definitions[index];
                VoceRicorrente target = source.Id != Guid.Empty && persistedById.TryGetValue(source.Id, out VoceRicorrente? existing)
                    ? existing : new VoceRicorrente { Id = Guid.NewGuid() };

                target.Name = name;
                target.DisplayName = source.DisplayName;
                target.Value = source.Value;
                target.ValidFrom = source.ValidFrom;
                target.ValidTo = source.ValidTo;
                target.Index = index;
                target.CategoriaId = null;

                if (target.Id != source.Id)
                {
                    db.VociRicorrenti.Add(target);
                }

                result.Add(target);
            }

            await SaveIfRequired();

            return result;
        }

        private async Task<int> SaveIfRequired() => persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
    }
}

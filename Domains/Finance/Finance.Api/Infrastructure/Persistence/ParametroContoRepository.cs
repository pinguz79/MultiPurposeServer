using Finance.Api.Infrastructure.Caching;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Infrastructure.Persistence
{
    public class ParametroContoRepository(
        FinanceContext db,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence,
        FormulaEvaluationCache? cache = null) : IParametroContoRepository
    {
        public async Task Delete(Guid contoId, string name)
        {
            IReadOnlyList<ParametroConto> definitions = await GetByName(contoId, name);

            if (definitions.Count == 0)
            {
                throw new KeyNotFoundException($"ParametroConto '{name}' was not found.");
            }

            db.ParametriConto.RemoveRange(definitions);
            await SaveIfRequired();
        }

        public async Task<IReadOnlyList<ParametroConto>> GetAll(Guid contoId) => await db.ParametriConto
            .Where(parametro => parametro.ContoId == contoId)
            .OrderBy(parametro => parametro.Name)
            .ThenBy(parametro => parametro.Index)
            .ToListAsync();

        public async Task<IReadOnlyList<ParametroConto>> GetByName(Guid contoId, string name) => await db.ParametriConto
            .Where(parametro => parametro.ContoId == contoId && parametro.Name == name)
            .OrderBy(parametro => parametro.Index)
            .ToListAsync();

        public async Task<bool> NameExists(Guid contoId, string name)
            => await db.ParametriConto.AnyAsync(parametro => parametro.ContoId == contoId && parametro.Name == name);

        public async Task<bool> IsReferenced(string reference)
        {
            string pattern = $"%[{reference}]%";

            return await db.Movimenti.AnyAsync(item => EF.Functions.Like(item.Formula, pattern))
                || await db.Pianificazioni.AnyAsync(item => EF.Functions.Like(item.MovimentoFormula, pattern));
        }

        public async Task<IReadOnlyList<ParametroConto>> Replace(
            Guid contoId,
            string? currentName,
            string name,
            TipoParametroConto type,
            IReadOnlyList<ParametroConto> definitions)
        {
            cache?.Invalidate();
            IReadOnlyList<ParametroConto> persisted = currentName is null ? [] : await GetByName(contoId, currentName);
            Dictionary<Guid, ParametroConto> persistedById = persisted.ToDictionary(parametro => parametro.Id);
            HashSet<Guid> retainedIds = [.. definitions.Where(parametro => parametro.Id != Guid.Empty).Select(parametro => parametro.Id)];

            for (var index = 0; index < persisted.Count; index++)
            {
                persisted[index].Index = -index - 1;
            }

            if (persisted.Count > 0)
            {
                await db.SaveChangesAsync();
            }

            db.ParametriConto.RemoveRange(persisted.Where(parametro => !retainedIds.Contains(parametro.Id)));
            var result = new List<ParametroConto>(definitions.Count);

            for (var index = 0; index < definitions.Count; index++)
            {
                ParametroConto source = definitions[index];
                ParametroConto target = source.Id != Guid.Empty && persistedById.TryGetValue(source.Id, out ParametroConto? existing)
                    ? existing : new ParametroConto { Id = Guid.NewGuid(), ContoId = contoId };

                target.Name = name;
                target.DisplayName = source.DisplayName;
                target.Type = type;
                target.Value = source.Value;
                target.ValidFrom = source.ValidFrom;
                target.ValidTo = source.ValidTo;
                target.Index = index;

                if (target.Id != source.Id)
                {
                    db.ParametriConto.Add(target);
                }

                result.Add(target);
            }

            await SaveIfRequired();

            return result;
        }

        private async Task<int> SaveIfRequired()
        {
            cache?.Invalidate();

            return persistence.IsTransactionActive ? 0 : await db.SaveChangesAsync();
        }
    }
}

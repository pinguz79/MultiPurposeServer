using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class FormulaResolver(IVoceRicorrenteRepository repository) : IFormulaResolver
    {
        public async Task<IReadOnlyList<ResolvedFormulaParameter>> Resolve(IReadOnlyList<string> dependencies, DateOnly date)
        {
            var result = new List<ResolvedFormulaParameter>(dependencies.Count);

            foreach (string dependency in dependencies)
            {
                IReadOnlyList<VoceRicorrente> definitions = await repository.GetByName(dependency);

                if (definitions.Count == 0)
                {
                    throw new KeyNotFoundException($"Recurring entry '{dependency}' was not found.");
                }

                VoceRicorrente? definition = definitions.FirstOrDefault(item => (item.ValidFrom is null || item.ValidFrom <= date)
                    && (item.ValidTo is null || item.ValidTo >= date));

                result.Add(new ResolvedFormulaParameter(definitions[0].Name, definition?.Value ?? 0m, definition is null));
            }

            return result;
        }

        public async Task<string?> ResolveCanonicalName(string dependency)
        {
            if (dependency.Contains('.'))
            {
                return null;
            }

            IReadOnlyList<VoceRicorrente> definitions = await repository.GetByName(dependency);

            return definitions.Count == 0 ? null : definitions[0].Name;
        }
    }
}

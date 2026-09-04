using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class FormulaResolver(
        IVoceRicorrenteRepository voceRicorrenteRepository,
        IContoRepository contoRepository,
        IParametroContoRepository parametroContoRepository) : IFormulaResolver
    {
        public const string SaldoUltimoCicloChiuso = "SaldoUltimoCicloChiuso";

        private readonly Dictionary<string, Conto?> _conti = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IReadOnlyList<ParametroConto>> _parametri = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IReadOnlyList<VoceRicorrente>> _vociRicorrenti = new(StringComparer.OrdinalIgnoreCase);

        public async Task<IReadOnlyList<ResolvedFormulaParameter>> Resolve(IReadOnlyList<string> dependencies, DateOnly date)
        {
            var result = new List<ResolvedFormulaParameter>(dependencies.Count);

            foreach (string dependency in dependencies)
            {
                if (dependency.Contains('.'))
                {
                    if (dependency.EndsWith($".{SaldoUltimoCicloChiuso}", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Calculated account property '{dependency}' must be evaluated by the formula evaluator.");
                    }

                    result.Add(await ResolveParametroConto(dependency, date));
                    continue;
                }

                IReadOnlyList<VoceRicorrente> definitions = await GetVoceRicorrenteDefinitions(dependency);

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
                string[] parts = dependency.Split('.', StringSplitOptions.TrimEntries);

                if (parts.Length != 2)
                {
                    return null;
                }

                Conto? conto = await GetConto(parts[0]);

                if (conto is null)
                {
                    return null;
                }

                if (string.Equals(parts[1], SaldoUltimoCicloChiuso, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{conto.Name}.{SaldoUltimoCicloChiuso}";
                }

                IReadOnlyList<ParametroConto> parameterDefinitions = await GetParametroDefinitions(conto.Id, parts[1]);

                return parameterDefinitions.Count == 0 ? null : $"{conto.Name}.{parameterDefinitions[0].Name}";
            }

            IReadOnlyList<VoceRicorrente> definitions = await GetVoceRicorrenteDefinitions(dependency);

            return definitions.Count == 0 ? null : definitions[0].Name;
        }

        private async Task<ResolvedFormulaParameter> ResolveParametroConto(string dependency, DateOnly date)
        {
            string[] parts = dependency.Split('.', StringSplitOptions.TrimEntries);
            Conto conto = await GetConto(parts[0])
                ?? throw new KeyNotFoundException($"Account '{parts[0]}' was not found.");
            IReadOnlyList<ParametroConto> definitions = await GetParametroDefinitions(conto.Id, parts[1]);

            if (definitions.Count == 0)
            {
                throw new KeyNotFoundException($"Account parameter '{dependency}' was not found.");
            }

            ParametroConto? definition = definitions.FirstOrDefault(item => (item.ValidFrom is null || item.ValidFrom <= date)
                && (item.ValidTo is null || item.ValidTo >= date));

            return new ResolvedFormulaParameter($"{conto.Name}.{definitions[0].Name}", definition?.Value ?? 0m, definition is null);
        }

        private async Task<Conto?> GetConto(string name)
        {
            if (!_conti.TryGetValue(name, out Conto? conto))
            {
                conto = await contoRepository.GetByName(name);
                _conti[name] = conto;
            }

            return conto;
        }

        private async Task<IReadOnlyList<ParametroConto>> GetParametroDefinitions(Guid contoId, string name)
        {
            string key = $"{contoId:N}:{name}";

            if (!_parametri.TryGetValue(key, out IReadOnlyList<ParametroConto>? definitions))
            {
                definitions = await parametroContoRepository.GetByName(contoId, name);
                _parametri[key] = definitions;
            }

            return definitions;
        }

        private async Task<IReadOnlyList<VoceRicorrente>> GetVoceRicorrenteDefinitions(string name)
        {
            if (!_vociRicorrenti.TryGetValue(name, out IReadOnlyList<VoceRicorrente>? definitions))
            {
                definitions = await voceRicorrenteRepository.GetByName(name);
                _vociRicorrenti[name] = definitions;
            }

            return definitions;
        }
    }
}

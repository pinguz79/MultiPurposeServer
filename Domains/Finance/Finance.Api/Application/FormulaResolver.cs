using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class FormulaResolver(
        IVoceRicorrenteRepository voceRicorrenteRepository,
        IContoRepository contoRepository,
        IParametroContoRepository parametroContoRepository,
        FormulaEvaluationCache? cache = null) : IFormulaResolver
    {
        public const string SaldoUltimoCicloChiuso = "SaldoUltimoCicloChiuso";
        public const string InteressiCiclo = "InteressiCiclo";
        public const string BolloCiclo = "BolloCiclo";
        public const string RataUltimoCicloChiuso = "RataUltimoCicloChiuso";

        private static readonly string[] CalculatedProperties = [SaldoUltimoCicloChiuso, InteressiCiclo, BolloCiclo, RataUltimoCicloChiuso];

        private readonly Dictionary<string, Conto?> _conti = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IReadOnlyList<ParametroConto>> _parametri = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IReadOnlyList<VoceRicorrente>> _vociRicorrenti = new(StringComparer.OrdinalIgnoreCase);
        private long _cacheRevision = -1;

        public async Task<IReadOnlyList<ResolvedFormulaParameter>> Resolve(IReadOnlyList<string> dependencies, DateOnly date)
        {
            InvalidateIfRequired();
            var result = new List<ResolvedFormulaParameter>(dependencies.Count);

            foreach (string dependency in dependencies)
            {
                if (dependency.Contains('.'))
                {
                    if (IsCalculatedProperty(dependency))
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
            InvalidateIfRequired();
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

                string? calculatedProperty = CalculatedProperties.FirstOrDefault(property => string.Equals(parts[1], property, StringComparison.OrdinalIgnoreCase));
                if (calculatedProperty is not null)
                {
                    return $"{conto.Name}.{calculatedProperty}";
                }

                IReadOnlyList<ParametroConto> parameterDefinitions = await GetParametroDefinitions(conto.Id, parts[1]);

                return parameterDefinitions.Count == 0 ? null : $"{conto.Name}.{parameterDefinitions[0].Name}";
            }

            IReadOnlyList<VoceRicorrente> definitions = await GetVoceRicorrenteDefinitions(dependency);

            return definitions.Count == 0 ? null : definitions[0].Name;
        }

        public static bool IsCalculatedProperty(string dependency) => CalculatedProperties.Any(property => dependency.EndsWith($".{property}", StringComparison.OrdinalIgnoreCase));

        private void InvalidateIfRequired()
        {
            if (cache is null || (cache.CanReuseAcrossEvaluations && _cacheRevision == cache.Revision))
            {
                return;
            }

            _conti.Clear();
            _parametri.Clear();
            _vociRicorrenti.Clear();
            _cacheRevision = cache.Revision;
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

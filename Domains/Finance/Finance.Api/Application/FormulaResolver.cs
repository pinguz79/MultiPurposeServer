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

                IReadOnlyList<VoceRicorrente> definitions = await voceRicorrenteRepository.GetByName(dependency);

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

                Conto? conto = await contoRepository.GetByName(parts[0]);

                if (conto is null)
                {
                    return null;
                }

                if (string.Equals(parts[1], SaldoUltimoCicloChiuso, StringComparison.OrdinalIgnoreCase))
                {
                    return $"{conto.Name}.{SaldoUltimoCicloChiuso}";
                }

                IReadOnlyList<ParametroConto> parameterDefinitions = await parametroContoRepository.GetByName(conto.Id, parts[1]);

                return parameterDefinitions.Count == 0 ? null : $"{conto.Name}.{parameterDefinitions[0].Name}";
            }

            IReadOnlyList<VoceRicorrente> definitions = await voceRicorrenteRepository.GetByName(dependency);

            return definitions.Count == 0 ? null : definitions[0].Name;
        }

        private async Task<ResolvedFormulaParameter> ResolveParametroConto(string dependency, DateOnly date)
        {
            string[] parts = dependency.Split('.', StringSplitOptions.TrimEntries);
            Conto conto = await contoRepository.GetByName(parts[0])
                ?? throw new KeyNotFoundException($"Account '{parts[0]}' was not found.");
            IReadOnlyList<ParametroConto> definitions = await parametroContoRepository.GetByName(conto.Id, parts[1]);

            if (definitions.Count == 0)
            {
                throw new KeyNotFoundException($"Account parameter '{dependency}' was not found.");
            }

            ParametroConto? definition = definitions.FirstOrDefault(item => (item.ValidFrom is null || item.ValidFrom <= date)
                && (item.ValidTo is null || item.ValidTo >= date));

            return new ResolvedFormulaParameter($"{conto.Name}.{definitions[0].Name}", definition?.Value ?? 0m, definition is null);
        }
    }
}

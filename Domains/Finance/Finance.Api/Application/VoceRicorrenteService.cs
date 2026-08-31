using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class VoceRicorrenteService(
        IVoceRicorrenteRepository repository,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence,
        ICategoriaService? categoriaService = null) : IVoceRicorrenteService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<IReadOnlyList<VoceRicorrente>> Create(
            string name,
            IReadOnlyList<VoceRicorrenteDefinitionRequest> definitions)
        {
            string normalizedName = ContoService.NormalizeName(name);

            return await repository.NameExists(normalizedName)
                ? throw new DuplicateNameException(normalizedName)
                : await repository.Replace(null, normalizedName, await MapDefinitions(definitions));
        }

        public Task Delete(string name) => repository.Delete(name);

        public async Task<IReadOnlyList<IReadOnlyList<VoceRicorrente>>> GetAll()
        {
            IReadOnlyList<VoceRicorrente> definitions = await repository.GetAll();

            return [.. definitions.GroupBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => (IReadOnlyList<VoceRicorrente>)[.. group.OrderBy(definition => definition.Index)])];
        }

        public Task<IReadOnlyList<VoceRicorrente>> GetByName(string name) => repository.GetByName(name);

        public async Task<IReadOnlyList<VoceRicorrente>> Update(
            string currentName,
            string name,
            IReadOnlyList<VoceRicorrenteDefinitionRequest> definitions)
        {
            IReadOnlyList<VoceRicorrente> persisted = await repository.GetByName(currentName);

            if (persisted.Count == 0)
            {
                throw new KeyNotFoundException($"VoceRicorrente '{currentName}' was not found.");
            }

            string normalizedName = ContoService.NormalizeName(name);

            if (!string.Equals(currentName, normalizedName, StringComparison.OrdinalIgnoreCase)
                && await repository.NameExists(normalizedName))
            {
                throw new DuplicateNameException(normalizedName);
            }

            HashSet<Guid> persistedIds = [.. persisted.Select(definition => definition.Id)];
            return definitions.Where(definition => definition.Id is not null).Any(definition => !persistedIds.Contains(definition.Id!.Value))
                ? throw new ArgumentException("A definition does not belong to the recurring entry.", nameof(definitions))
                : await repository.Replace(currentName, normalizedName, await MapDefinitions(definitions));
        }

        private async Task<IReadOnlyList<VoceRicorrente>> MapDefinitions(IReadOnlyList<VoceRicorrenteDefinitionRequest> definitions)
        {
            if (definitions.Count == 0)
            {
                throw new ArgumentException("At least one definition is required.", nameof(definitions));
            }

            Guid[] ids = [.. definitions.Where(definition => definition.Id is not null).Select(definition => definition.Id!.Value)];
            return ids.Distinct().Count() != ids.Length
                ? throw new ArgumentException("Definition identifiers must be unique.", nameof(definitions))
                : [.. await Task.WhenAll(definitions.Select(MapDefinition))];
        }

        private async Task<VoceRicorrente> MapDefinition(VoceRicorrenteDefinitionRequest definition, int index)
        {
            string displayName = definition.DisplayName.Trim();

            Guid? categoriaId = definition.CategoryName is null ? null
                : (await (categoriaService ?? throw new InvalidOperationException("Category service is not available.")).Resolve(definition.CategoryName)).Id;

            return displayName.Length == 0
                ? throw new ArgumentException("DisplayName cannot be empty.", nameof(definition))
                : definition.ValidFrom > definition.ValidTo
                ? throw new ArgumentException("ValidFrom cannot be later than ValidTo.", nameof(definition))
                : decimal.Round(definition.Value, 2, MidpointRounding.AwayFromZero) != definition.Value
                ? throw new ArgumentException("Monetary amounts cannot contain more than two decimal places.", nameof(definition))
                : new VoceRicorrente
                {
                    Id = definition.Id ?? Guid.Empty,
                    DisplayName = displayName,
                    Value = definition.Value,
                    ValidFrom = definition.ValidFrom,
                    ValidTo = definition.ValidTo,
                    Index = index,
                    CategoriaId = categoriaId,
                };
        }
    }
}

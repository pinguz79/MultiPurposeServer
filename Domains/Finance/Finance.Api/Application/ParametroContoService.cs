using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class ParametroContoService(
        IParametroContoRepository repository,
        IContoRepository contoRepository,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence) : IParametroContoService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<IReadOnlyList<ParametroConto>> Create(
            string contoName,
            string name,
            TipoParametroConto type,
            IReadOnlyList<ParametroContoDefinitionRequest> definitions)
        {
            Conto conto = await GetConto(contoName);
            string normalizedName = ContoService.NormalizeName(name);

            return await repository.NameExists(conto.Id, normalizedName)
                ? throw new DuplicateNameException(normalizedName)
                : await repository.Replace(conto.Id, null, normalizedName, type, MapDefinitions(type, definitions));
        }

        public async Task Delete(string contoName, string name)
        {
            Conto conto = await GetConto(contoName);
            IReadOnlyList<ParametroConto> definitions = await repository.GetByName(conto.Id, name);

            if (definitions.Count == 0)
            {
                throw new KeyNotFoundException($"ParametroConto '{name}' was not found.");
            }

            string reference = $"{conto.Name}.{definitions[0].Name}";

            if (await repository.IsReferenced(reference))
            {
                throw new ParametroContoReferencedException(reference);
            }

            await repository.Delete(conto.Id, definitions[0].Name);
        }

        public async Task<IReadOnlyList<IReadOnlyList<ParametroConto>>> GetAll(string contoName)
        {
            Conto conto = await GetConto(contoName);
            IReadOnlyList<ParametroConto> definitions = await repository.GetAll(conto.Id);

            return [.. definitions.GroupBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => (IReadOnlyList<ParametroConto>)[.. group.OrderBy(definition => definition.Index)])];
        }

        public async Task<IReadOnlyList<ParametroConto>> GetByName(string contoName, string name)
        {
            Conto conto = await GetConto(contoName);

            return await repository.GetByName(conto.Id, name);
        }

        public async Task<ParametroConto?> Resolve(Guid contoId, string name, DateOnly date)
        {
            IReadOnlyList<ParametroConto> definitions = await repository.GetByName(contoId, name);

            return definitions.FirstOrDefault(definition => (definition.ValidFrom is null || definition.ValidFrom <= date)
                && (definition.ValidTo is null || definition.ValidTo >= date));
        }

        public async Task<IReadOnlyList<ParametroConto>> Update(
            string contoName,
            string name,
            IReadOnlyList<ParametroContoDefinitionRequest> definitions)
        {
            Conto conto = await GetConto(contoName);
            IReadOnlyList<ParametroConto> persisted = await repository.GetByName(conto.Id, name);

            if (persisted.Count == 0)
            {
                throw new KeyNotFoundException($"ParametroConto '{name}' was not found.");
            }

            HashSet<Guid> persistedIds = [.. persisted.Select(definition => definition.Id)];
            if (definitions.Where(definition => definition.Id is not null).Any(definition => !persistedIds.Contains(definition.Id!.Value)))
            {
                throw new ArgumentException("A definition does not belong to the account parameter.", nameof(definitions));
            }

            TipoParametroConto type = persisted[0].Type;

            return await repository.Replace(conto.Id, name, persisted[0].Name, type, MapDefinitions(type, definitions));
        }

        private static IReadOnlyList<ParametroConto> MapDefinitions(
            TipoParametroConto type,
            IReadOnlyList<ParametroContoDefinitionRequest> definitions)
        {
            if (definitions.Count == 0)
            {
                throw new ArgumentException("At least one definition is required.", nameof(definitions));
            }

            if (definitions.Count(definition => definition.ValidFrom is null && definition.ValidTo is null) != 1)
            {
                throw new ArgumentException("Exactly one permanent definition is required.", nameof(definitions));
            }

            Guid[] ids = [.. definitions.Where(definition => definition.Id is not null).Select(definition => definition.Id!.Value)];

            return ids.Distinct().Count() == ids.Length
                ? [.. definitions.Select((definition, index) => MapDefinition(type, definition, index))]
                : throw new ArgumentException("Definition identifiers must be unique.", nameof(definitions));
        }

        private static ParametroConto MapDefinition(TipoParametroConto type, ParametroContoDefinitionRequest definition, int index)
        {
            string displayName = definition.DisplayName.Trim();

            return displayName.Length == 0
                ? throw new ArgumentException("DisplayName cannot be empty.", nameof(definition))
                : definition.ValidFrom > definition.ValidTo
                ? throw new ArgumentException("ValidFrom cannot be later than ValidTo.", nameof(definition))
                : type == TipoParametroConto.Importo && decimal.Round(definition.Value, 2, MidpointRounding.AwayFromZero) != definition.Value
                ? throw new ArgumentException("Monetary amounts cannot contain more than two decimal places.", nameof(definition))
                : type == TipoParametroConto.Intero && decimal.Truncate(definition.Value) != definition.Value
                ? throw new ArgumentException("Integer parameters cannot contain decimal places.", nameof(definition))
                : new ParametroConto
                {
                    Id = definition.Id ?? Guid.Empty,
                    DisplayName = displayName,
                    Type = type,
                    Value = definition.Value,
                    ValidFrom = definition.ValidFrom,
                    ValidTo = definition.ValidTo,
                    Index = index,
                };
        }

        private async Task<Conto> GetConto(string contoName)
            => await contoRepository.GetByName(contoName) ?? throw new KeyNotFoundException($"Conto '{contoName}' was not found.");
    }
}

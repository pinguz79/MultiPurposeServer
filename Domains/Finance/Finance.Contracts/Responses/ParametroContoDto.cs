using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public sealed record ParametroContoDto(
        string Name,
        string DisplayName,
        TipoParametroConto Type,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        decimal CurrentValue,
        IReadOnlyList<ParametroContoDefinitionDto> Definitions)
    {
        public ParametroContoDto(IReadOnlyList<ParametroConto> definitions, DateOnly today)
            : this(
                definitions[0].Name,
                Resolve(definitions, today).DisplayName,
                definitions[0].Type,
                definitions.Any(definition => definition.ValidFrom is null) ? null : definitions.Min(definition => definition.ValidFrom),
                definitions.Any(definition => definition.ValidTo is null) ? null : definitions.Max(definition => definition.ValidTo),
                Resolve(definitions, today).Value,
                [.. definitions.OrderBy(definition => definition.Index).Select(definition => new ParametroContoDefinitionDto(definition))])
        {
        }

        private static ParametroConto Resolve(IReadOnlyList<ParametroConto> definitions, DateOnly date)
            => definitions.First(definition => (definition.ValidFrom is null || definition.ValidFrom <= date)
                && (definition.ValidTo is null || definition.ValidTo >= date));
    }
}

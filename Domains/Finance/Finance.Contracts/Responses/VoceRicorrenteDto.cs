using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public sealed record VoceRicorrenteDto(
        string Name,
        string DisplayName,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        decimal? CurrentValue,
        IReadOnlyList<VoceRicorrenteDefinitionDto> Definitions)
    {
        public VoceRicorrenteDto(IReadOnlyList<VoceRicorrente> definitions, DateOnly today)
            : this(
                definitions[0].Name,
                definitions[0].DisplayName,
                definitions.Any(definition => definition.ValidFrom is null) ? null : definitions.Min(definition => definition.ValidFrom),
                definitions.Any(definition => definition.ValidTo is null) ? null : definitions.Max(definition => definition.ValidTo),
                definitions.FirstOrDefault(definition => (definition.ValidFrom is null || definition.ValidFrom <= today)
                    && (definition.ValidTo is null || definition.ValidTo >= today))?.Value,
                [.. definitions.OrderBy(definition => definition.Index).Select(definition => new VoceRicorrenteDefinitionDto(definition))])
        {
        }
    }
}

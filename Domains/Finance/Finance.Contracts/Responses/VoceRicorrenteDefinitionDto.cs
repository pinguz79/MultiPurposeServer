using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public sealed record VoceRicorrenteDefinitionDto(
        Guid Id,
        string DisplayName,
        decimal Value,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        int Index,
        CategoriaReferenceDto? Category)
    {
        public VoceRicorrenteDefinitionDto(VoceRicorrente definition)
            : this(definition.Id, definition.DisplayName, definition.Value, definition.ValidFrom, definition.ValidTo, definition.Index,
                definition.Categoria is null ? null : new CategoriaReferenceDto(definition.Categoria))
        {
        }
    }
}

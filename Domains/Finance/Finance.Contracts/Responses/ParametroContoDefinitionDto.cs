using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public sealed record ParametroContoDefinitionDto(Guid Id, string DisplayName, decimal Value, DateOnly? ValidFrom, DateOnly? ValidTo, int Index)
    {
        public ParametroContoDefinitionDto(ParametroConto definition)
            : this(definition.Id, definition.DisplayName, definition.Value, definition.ValidFrom, definition.ValidTo, definition.Index)
        {
        }
    }
}

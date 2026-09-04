namespace Finance.Desktop.Models
{
    public sealed record VoceRicorrenteDefinition(
        Guid? Id,
        string DisplayName,
        decimal Value,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        int Index,
        CategoriaReference? Category) : ITemporalDefinition;
}

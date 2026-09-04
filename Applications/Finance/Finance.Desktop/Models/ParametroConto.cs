namespace Finance.Desktop.Models
{
    public sealed record ParametroConto(
        string Name,
        string DisplayName,
        TipoParametroConto Type,
        decimal? CurrentValue,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        IReadOnlyList<ParametroContoDefinition> Definitions);
}

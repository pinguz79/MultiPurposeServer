namespace Finance.Desktop.Models
{
    public sealed record CreateParametroConto(
        string Name,
        TipoParametroConto Type,
        IReadOnlyList<SaveParametroContoDefinition> Definitions);
}

namespace Finance.Desktop.Models
{
    public sealed record ParametroContoDefinition(
        Guid? Id,
        string DisplayName,
        decimal Value,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        int Index) : ITemporalDefinition;
}

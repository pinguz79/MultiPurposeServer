namespace Finance.Desktop.Models
{
    public sealed record SaveParametroContoDefinition(
        Guid? Id,
        string DisplayName,
        decimal Value,
        DateOnly? ValidFrom,
        DateOnly? ValidTo);
}

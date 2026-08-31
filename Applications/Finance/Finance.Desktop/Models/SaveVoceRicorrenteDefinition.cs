namespace Finance.Desktop.Models
{
    public sealed record SaveVoceRicorrenteDefinition(
        Guid? Id,
        string DisplayName,
        decimal Value,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        string? CategoryName);
}

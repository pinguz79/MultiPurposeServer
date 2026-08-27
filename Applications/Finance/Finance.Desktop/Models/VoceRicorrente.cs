namespace Finance.Desktop.Models
{
    public sealed record VoceRicorrente(
        string Name,
        string DisplayName,
        DateOnly? ValidFrom,
        DateOnly? ValidTo,
        decimal? CurrentValue,
        IReadOnlyList<VoceRicorrenteDefinition> Definitions);
}

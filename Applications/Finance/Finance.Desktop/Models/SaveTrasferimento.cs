namespace Finance.Desktop.Models
{
    public sealed record SaveTrasferimento(string OrigineName, string DestinazioneName, decimal Amount,
        DateOnly Date, string Description, bool IsConfirmed = false);
}

namespace Finance.Desktop.Models
{
    public sealed record SavePedaggio(string ContoName, DateOnly Date, Guid EntrataId, Guid UscitaId, string Description, string? CategoryName, bool IsConfirmed);
}

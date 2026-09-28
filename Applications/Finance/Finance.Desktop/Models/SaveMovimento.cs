namespace Finance.Desktop.Models
{
    public sealed record SaveMovimento(string ContoName, DateOnly Date, string Description, string Formula, bool IsConfirmed, string? CategoryName);
}

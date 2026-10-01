namespace Finance.Desktop.Models
{
    public sealed record Finanziamento(Guid Id, string Name, string DisplayName, string Lender, decimal InitialPrincipal, decimal Tan, decimal Installment,
        decimal Insurance, decimal Fees, DateOnly FirstDueDate, int DueDay, int InstallmentCount, bool IsClosed);
}

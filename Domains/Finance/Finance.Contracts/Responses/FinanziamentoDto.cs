using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class FinanziamentoDto(Finanziamento loan)
    {
        public Guid Id { get; set; } = loan.Id;
        public string Name { get; set; } = loan.Name;
        public string DisplayName { get; set; } = loan.DisplayName;
        public string Lender { get; set; } = loan.Lender;
        public decimal InitialPrincipal { get; set; } = loan.InitialPrincipal;
        public decimal Tan { get; set; } = loan.Tan;
        public decimal Installment { get; set; } = loan.Installment;
        public decimal Insurance { get; set; } = loan.Insurance;
        public decimal Fees { get; set; } = loan.Fees;
        public DateOnly FirstDueDate { get; set; } = loan.FirstDueDate;
        public int DueDay { get; set; } = loan.DueDay;
        public int InstallmentCount { get; set; } = loan.InstallmentCount;
        public bool IsClosed { get; set; } = loan.IsClosed;
    }
}

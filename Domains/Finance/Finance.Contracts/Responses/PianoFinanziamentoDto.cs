using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class PianoFinanziamentoDto(Finanziamento loan, PianoFinanziamento plan)
    {
        public FinanziamentoDto Finanziamento { get; set; } = new(loan);
        public DateOnly ReferenceDate { get; set; } = plan.ReferenceDate;
        public decimal RemainingPrincipal { get; set; } = plan.RemainingPrincipal;
        public int RemainingInstallments { get; set; } = plan.RemainingInstallments;
        public decimal RemainingTotal { get; set; } = plan.RemainingTotal;
        public DateOnly LastDueDate { get; set; } = plan.LastDueDate;
        public decimal FinalPrincipal { get; set; } = plan.FinalPrincipal;
        public decimal TotalDiscrepancy { get; set; } = plan.TotalDiscrepancy;
        public RataFinanziamentoDto? LastAlignment { get; set; } = plan.LastAlignment is null ? null : new(plan.LastAlignment);
        public IReadOnlyList<RataFinanziamentoDto> Installments { get; set; } = [.. plan.Installments.Select(row => new RataFinanziamentoDto(row))];
    }
}

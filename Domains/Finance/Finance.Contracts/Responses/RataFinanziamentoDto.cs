using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class RataFinanziamentoDto(RataFinanziamento row)
    {
        public int Number { get; set; } = row.Number;
        public DateOnly DueDate { get; set; } = row.DueDate;
        public bool IsPast { get; set; } = row.IsPast;
        public decimal Principal { get; set; } = row.Principal;
        public decimal Interest { get; set; } = row.Interest;
        public decimal Insurance { get; set; } = row.Insurance;
        public decimal Fees { get; set; } = row.Fees;
        public decimal Total { get; set; } = row.Total;
        public decimal RemainingPrincipal { get; set; } = row.RemainingPrincipal;
        public decimal Discrepancy { get; set; } = row.Discrepancy;
        public decimal? VerifiedPrincipal { get; set; } = row.VerifiedPrincipal;
    }
}

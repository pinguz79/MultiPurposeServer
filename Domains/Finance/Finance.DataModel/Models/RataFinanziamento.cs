namespace Finance.DataModel.Models
{
    public sealed record RataFinanziamento(int Number, DateOnly DueDate, bool IsPast, decimal Principal, decimal Interest, decimal Insurance, decimal Fees,
        decimal Total, decimal RemainingPrincipal, decimal Discrepancy, decimal? VerifiedPrincipal);
}

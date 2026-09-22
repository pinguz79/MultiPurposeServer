namespace Finance.Desktop.Models
{
    public sealed record ConfigureCartaRevolving(decimal Plafond, decimal PercentualeScoperto, decimal QuotaRata, decimal RataMinima, decimal Tan, decimal Bollo, decimal SogliaBollo,
        int ChiusuraCiclo, int Addebito, string ContoAddebitoName, DateOnly ValidFrom, DateOnly ValidTo, decimal? Rata = null);
}

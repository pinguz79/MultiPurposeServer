namespace Finance.Desktop.Models
{
    public sealed record ConfigureCartaASaldo(
        decimal Plafond,
        decimal PercentualeScoperto,
        int ChiusuraCiclo,
        int Addebito,
        int RipristinoPlafond,
        string ContoAddebitoName,
        DateOnly ValidFrom,
        DateOnly ValidTo);
}

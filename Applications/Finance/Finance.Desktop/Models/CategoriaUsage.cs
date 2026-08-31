namespace Finance.Desktop.Models
{
    public sealed record CategoriaUsage(int VociRicorrenti, int Pianificazioni, int Movimenti)
    {
        public int Total => VociRicorrenti + Pianificazioni + Movimenti;
    }
}

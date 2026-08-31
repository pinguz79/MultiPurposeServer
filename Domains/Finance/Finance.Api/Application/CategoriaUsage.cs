namespace Finance.Api.Application
{
    public sealed record CategoriaUsage(int VociRicorrenti, int Pianificazioni, int Movimenti)
    {
        public int Total => VociRicorrenti + Pianificazioni + Movimenti;
    }
}

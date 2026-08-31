namespace Finance.Contracts.Responses
{
    public sealed record CategoriaUsageDto(int VociRicorrenti, int Pianificazioni, int Movimenti)
    {
        public int Total => VociRicorrenti + Pianificazioni + Movimenti;
    }
}

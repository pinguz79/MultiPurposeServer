using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface ICycleIndicatorsService
    {
        Task<CycleIndicators?> Get(Conto conto, decimal balance, DateOnly date);
    }
}

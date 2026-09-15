using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface IRevolvingIndicatorsService
    {
        Task<RevolvingIndicators?> Get(Conto conto, decimal balance, DateOnly date);
    }
}

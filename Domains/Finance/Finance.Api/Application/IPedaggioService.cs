using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface IPedaggioService
    {
        Task<Pedaggio?> Get(Guid movimentoId);
        Task<Pedaggio> Create(SavePedaggioRequest request);
        Task<Pedaggio> Update(Guid movimentoId, SavePedaggioRequest request);
    }
}

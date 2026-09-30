using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IPedaggioRepository
    {
        Task<Pedaggio?> GetByMovimento(Guid movimentoId);
        Task<IReadOnlyList<Pedaggio>> GetByMovimenti(IReadOnlyList<Guid> movimentoIds);
        Task Save(Pedaggio pedaggio);
    }
}

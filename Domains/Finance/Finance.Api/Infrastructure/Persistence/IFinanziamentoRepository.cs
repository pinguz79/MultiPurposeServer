using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IFinanziamentoRepository
    {
        Task<IReadOnlyList<Finanziamento>> GetAll(bool includeClosed);
        Task<Finanziamento?> GetByName(string name);
        Task<Finanziamento> Create(Finanziamento loan);
        Task Save();
        Task AddAlignment(RiallineamentoFinanziamento alignment);
        Task RemoveAlignment(RiallineamentoFinanziamento alignment);
    }
}

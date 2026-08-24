using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IContoRepository
    {
        Task<Conto> CreateConto(string name, string displayName, decimal initialBalance);
        Task<Conto?> GetById(Guid id);
        Task<IReadOnlyList<Conto>> GetConti();
        Task<bool> NameExists(string name);
        Task<Conto> UpdateConto(Guid id, string? displayName, decimal? initialBalance);
    }
}

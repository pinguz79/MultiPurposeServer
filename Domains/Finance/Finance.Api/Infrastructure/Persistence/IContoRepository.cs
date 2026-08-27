using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IContoRepository
    {
        Task<Conto> CreateConto(string name, string displayName, decimal initialBalance);
        Task<Conto?> GetById(Guid id);
        Task<Conto?> GetByName(string name);
        Task<IReadOnlyList<Conto>> GetConti();
        Task<bool> NameExists(string name);
        Task<bool> NameExists(string name, Guid excludedId);
        Task<Conto> UpdateConto(Guid id, string? name, string? displayName, decimal? initialBalance);
    }
}

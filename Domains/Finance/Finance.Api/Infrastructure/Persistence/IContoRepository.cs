using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IContoRepository
    {
        Task<Conto> Create(string name, string displayName, decimal initialBalance);
        Task<Conto?> Get(Guid id);
        Task<IReadOnlyList<Conto>> GetAll();
        Task<bool> NameExists(string name);
        Task<Conto> Update(Guid id, string? displayName, decimal? initialBalance);
    }
}

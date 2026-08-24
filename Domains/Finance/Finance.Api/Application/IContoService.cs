using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface IContoService
    {
        Task<Conto> Create(string name, string displayName, decimal initialBalance);
        Task<Conto?> Get(Guid id);
        Task<IReadOnlyList<Conto>> GetAll();
        Task<Conto> Update(Guid id, string? displayName, decimal? initialBalance);
    }
}

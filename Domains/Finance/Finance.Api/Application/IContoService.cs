using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface IContoService
    {
        Task<Conto> CreateConto(string name, string displayName, decimal initialBalance);
        Task<Conto?> GetById(Guid id);
        Task<decimal> GetBalance(Conto conto);
        Task<ContoStatus> GetStatus(Conto conto);
        Task<IReadOnlyList<Conto>> GetConti();
        Task<Conto> UpdateConto(Guid id, string? name, string? displayName, decimal? initialBalance);
    }
}

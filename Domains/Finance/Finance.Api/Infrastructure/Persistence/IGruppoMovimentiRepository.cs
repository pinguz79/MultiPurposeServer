using Finance.DataModel.Models;

namespace Finance.Api.Infrastructure.Persistence
{
    public interface IGruppoMovimentiRepository
    {
        Task<GruppoMovimenti?> GetById(Guid id);
        Task<GruppoMovimenti> Create(IReadOnlyList<Movimento> movements);
        Task Remove(Guid id);
    }
}

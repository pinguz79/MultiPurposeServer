using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.DataModel.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : class, IEntity
    {
        Task<IPersistenceTransaction> BeginTransaction();
        Task<TEntity?> GetById(Guid id);
        Task<List<TEntity>> GetAll();
        Task<List<TEntity>> GetByIds(IEnumerable<Guid> ids);
        Task<int> SaveIfRequired();
    }
}

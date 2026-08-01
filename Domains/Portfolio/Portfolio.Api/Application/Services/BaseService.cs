using Portfolio.Api.Application.Operations;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Data.Models;

namespace Portfolio.Api.Application.Services
{
    public class BaseService<TEntity>(IRepository<TEntity> repository) : IService<TEntity>
        where TEntity : class, IEntity
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await repository.BeginTransaction());
        public Task<TEntity?> GetById(Guid id) => repository.GetById(id);
    }
}

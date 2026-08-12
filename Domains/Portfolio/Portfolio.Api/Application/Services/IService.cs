using Portfolio.Api.Application.Operations;
using Portfolio.Data.Models;

namespace Portfolio.Api.Application.Services
{
    public interface IService<TEntity>
        where TEntity : class, IEntity
    {
        Task<IApplicationOperation> BeginOperation();
        Task<TEntity?> GetById(Guid id);
    }
}

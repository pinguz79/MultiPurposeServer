using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.Transactions;

using Portfolio.DataModel;
using Portfolio.DataModel.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository<TEntity>(PortfolioContext context, IPersistenceCoordinator persistence) : IRepository<TEntity>
        where TEntity : class, IEntity
    {
        private readonly DbSet<TEntity> _set = context.Set<TEntity>();

        protected virtual string EntityName => typeof(TEntity).Name;

        public Task<IPersistenceTransaction> BeginTransaction() => persistence.BeginTransaction();

        #region Get

        public async Task<TEntity?> GetById(Guid id) => await _set.FirstOrDefaultAsync(entity => entity.Id == id);

        public async Task<List<TEntity>> GetAll() => await _set.ToListAsync();

        public async Task<List<TEntity>> GetByIds(IEnumerable<Guid> ids) => await _set.Where(entity => ids.Contains(entity.Id)).ToListAsync();

        #endregion

        #region Persistenza

        public async Task<int> SaveIfRequired() => persistence.IsTransactionActive ? 0 : await context.SaveChangesAsync();

        protected async Task<TEntity> Add(TEntity entity)
        {
            _set.Add(entity);
            await SaveIfRequired();

            return entity;
        }

        protected async Task<TEntity> Update(Guid id, Action<TEntity> update)
        {
            var entity = await GetRequiredById(id);

            update(entity);
            await SaveIfRequired();

            return entity;
        }

        protected async Task Remove(Guid id)
        {
            var entity = await GetRequiredById(id);
            _set.Remove(entity);
            await SaveIfRequired();
        }

        #endregion

        protected IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate) => _set.Where(predicate);


        #region Validazione

        protected static string NormalizeRequiredString(string? value, string parameterName, string fieldName)
        {
            var normalizedValue = value?.Trim() ?? string.Empty;

            return normalizedValue.Length == 0 ? throw new ArgumentNullException(parameterName, $"{fieldName} cannot be empty.")
                : normalizedValue;
        }

        private async Task<TEntity> GetRequiredById(Guid id) => await GetById(id) ?? throw new KeyNotFoundException($"{EntityName} '{id}' was not found.");
        #endregion

    }
}

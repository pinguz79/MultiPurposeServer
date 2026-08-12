using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Data;
using Portfolio.Data.Models;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository<TEntity>(PortfolioContext context) : IRepository<TEntity>, ITransactionalRepository
        where TEntity : class, IEntity
    {
        private readonly DbSet<TEntity> _set = context.Set<TEntity>();
        private IDbContextTransaction? _transaction;

        protected virtual string EntityName => typeof(TEntity).Name;

        public async Task<IPersistenceTransaction> BeginTransaction()
        {
            if (_transaction is not null)
            {
                throw new InvalidOperationException("A repository transaction is already active.");
            }

            _transaction = await context.Database.BeginTransactionAsync();

            return new PersistenceTransaction(this);
        }

        public async Task CommitTransaction()
        {
            if (_transaction is null)
            {
                throw new InvalidOperationException("No repository transaction is active.");
            }

            try
            {
                await context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransaction()
        {
            if (_transaction is null)
            {
                throw new InvalidOperationException("No repository transaction is active.");
            }

            try
            {
                await _transaction.RollbackAsync();
                context.ChangeTracker.Clear();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<TEntity?> GetById(Guid id) => await _set.FirstOrDefaultAsync(entity => entity.Id == id);

        public async Task<List<TEntity>> GetAll() => await _set.ToListAsync();

        public async Task<List<TEntity>> GetByIds(IEnumerable<Guid> ids) => await _set.Where(entity => ids.Contains(entity.Id)).ToListAsync();

        public async Task<int> SaveIfRequired()
        {
            if (_transaction is not null)
            {
                return 0;
            }

            return await context.SaveChangesAsync();
        }

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

        protected IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate) => _set.Where(predicate);

        protected static string NormalizeRequiredString(string? value, string parameterName, string fieldName)
        {
            var normalizedValue = value?.Trim() ?? string.Empty;

            if (normalizedValue.Length == 0)
            {
                throw new ArgumentNullException(parameterName, $"{fieldName} cannot be empty.");
            }

            return normalizedValue;
        }

        private async Task<TEntity> GetRequiredById(Guid id) => await GetById(id) ?? throw new KeyNotFoundException($"{EntityName} '{id}' was not found.");
    }
}

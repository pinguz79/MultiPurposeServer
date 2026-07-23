using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Data;
using Portfolio.Data.Models;
using System.Linq.Expressions;

namespace Portfolio.Api.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository<TEntity>(PortfolioContext context) : ITransactionalRepository
        where TEntity : class, IEntity
    {
        private IDbContextTransaction? _transaction;
        protected abstract DbSet<TEntity> Set { get; }

        protected static string NormalizeRequiredString(string? value, string parameterName, string fieldName)
        {
            string normalizedValue = value?.Trim() ?? string.Empty;

            if (normalizedValue.Length == 0)
            {
                throw new ArgumentNullException($"{fieldName} cannot be empty.", parameterName);
            }

            return normalizedValue;
        }
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

        public async Task<TEntity?> GetById(Guid id) => await Set.FirstOrDefaultAsync(entity => entity.Id == id);
        public async Task<TEntity> GetRequiredById(Guid id, object entityName) => await GetById(id) ?? throw new KeyNotFoundException($"{entityName} '{id}' was not found.");

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

        public async Task<int> Save()
        {
            if (_transaction is not null)
            {
                return 0;
            }

            return await context.SaveChangesAsync();
        }
        protected async Task SaveIfRequired()
        {
            if (_transaction is null)
            {
                await context.SaveChangesAsync();
            }
        }

        protected async Task<TEntity> Update(Guid id, Action<TEntity> update, string entityName)
        {
            var album = await GetRequiredById(id, entityName);

            update(album);
            await SaveIfRequired();

            return album;
        }

        public async Task<List<TEntity>> GetByIds(IEnumerable<Guid> ids) => await Set.Where(entity => ids.Contains(entity.Id)).ToListAsync();
        public async Task<List<TEntity>> GetAll() => await Set.ToListAsync();

        public async Task<TEntity> Add(TEntity entity)
        {
            Set.Add(entity);
            await SaveIfRequired();
            return entity;
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate) => Set.Where(predicate);
        public async Task<PagedResult<TEntity>> GetPagedResult(IQueryable<TEntity> query, int page, int pageSize)
        {
            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<TEntity>(items, totalItems);
        }
    }
}

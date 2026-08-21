using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using MultiPurposeServer.Shared.Persistence.Transactions;

namespace MultiPurposeServer.Shared.Persistence.EntityFramework
{
    public sealed class EntityFrameworkPersistenceCoordinator<TContext>(TContext context) : IPersistenceCoordinator
        where TContext : DbContext
    {
        private IDbContextTransaction? _transaction;

        public bool IsTransactionActive => _transaction is not null;

        #region Transazioni

        public async Task<IPersistenceTransaction> BeginTransaction()
        {
            if (IsTransactionActive)
            {
                throw new InvalidOperationException("A persistence transaction is already active.");
            }

            _transaction = await context.Database.BeginTransactionAsync();

            return new PersistenceTransaction(this);
        }

        public async Task CommitTransaction()
        {
            EnsureTransactionIsActive();

            try
            {
                await context.SaveChangesAsync();
                await _transaction!.CommitAsync();
            }
            finally
            {
                await _transaction!.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransaction()
        {
            EnsureTransactionIsActive();

            try
            {
                await _transaction!.RollbackAsync();
                context.ChangeTracker.Clear();
            }
            finally
            {
                await _transaction!.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

        #region Checkpoint

        public async Task CreateCheckpoint(string name)
        {
            EnsureTransactionIsActive();
            await _transaction!.CreateSavepointAsync(name);
        }

        public async Task CompleteCheckpoint(string name)
        {
            EnsureTransactionIsActive();
            await context.SaveChangesAsync();
            await _transaction!.ReleaseSavepointAsync(name);
            context.ChangeTracker.Clear();
        }

        public async Task RollbackCheckpoint(string name)
        {
            EnsureTransactionIsActive();

            try
            {
                await _transaction!.RollbackToSavepointAsync(name);
                await _transaction.ReleaseSavepointAsync(name);
            }
            finally
            {
                context.ChangeTracker.Clear();
            }
        }

        #endregion

        private void EnsureTransactionIsActive()
        {
            if (_transaction is null)
            {
                throw new InvalidOperationException("No persistence transaction is active.");
            }
        }
    }
}

using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Contracts.Responses;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

using Portfolio.Api.Application.Operations;

namespace Portfolio.Api.Application.Bulk
{
    public static class BulkOperationExecutor
    {
        public static async Task<BulkResponse<TKey, TValue>> Execute<TItem, TKey, TValue>(
            IReadOnlyCollection<TItem> items,
            BulkOptions options,
            Func<TItem, TKey> keySelector,
            Func<TItem, Task<TValue>> processItem,
            Func<Task<IApplicationOperation>> beginOperation,
            Func<Exception, BulkError?> mapPersistenceError)
            where TItem : IRequest
        {
            List<BulkItemResult<TKey, TValue>> results = options.PersistenceStrategy == BulkPersistenceStrategy.AllOrNothing
                ? await ExecuteAllOrNothing(items, options, keySelector, processItem, beginOperation, mapPersistenceError)
                : await ExecutePartialSuccess(items, options, keySelector, processItem, beginOperation, mapPersistenceError);

            return new BulkResponse<TKey, TValue>(options, GetOutcome(options.PersistenceStrategy, results), results);
        }

        #region Strategie di persistenza

        private static async Task<List<BulkItemResult<TKey, TValue>>> ExecuteAllOrNothing<TItem, TKey, TValue>(
            IReadOnlyCollection<TItem> items,
            BulkOptions options,
            Func<TItem, TKey> keySelector,
            Func<TItem, Task<TValue>> processItem,
            Func<Task<IApplicationOperation>> beginOperation,
            Func<Exception, BulkError?> mapPersistenceError)
            where TItem : IRequest
        {
            var results = new List<BulkItemResult<TKey, TValue>>(items.Count);
            await using var operation = await beginOperation();

            await EvaluateItems(items, options, keySelector, item => ProcessWithCheckpoint(item, operation, processItem), mapPersistenceError, results);

            if (results.All(result => result.Outcome == BulkItemOutcome.Succeeded))
            {
                await operation.Complete();
                MarkSuccessfulItemsAsPersisted(results);
            }

            return results;
        }

        private static async Task<List<BulkItemResult<TKey, TValue>>> ExecutePartialSuccess<TItem, TKey, TValue>(
            IReadOnlyCollection<TItem> items,
            BulkOptions options,
            Func<TItem, TKey> keySelector,
            Func<TItem, Task<TValue>> processItem,
            Func<Task<IApplicationOperation>> beginOperation,
            Func<Exception, BulkError?> mapPersistenceError)
            where TItem : IRequest
        {
            var results = new List<BulkItemResult<TKey, TValue>>(items.Count);

            await EvaluateItems(items, options, keySelector, ProcessInIndependentOperation, mapPersistenceError, results);
            MarkSuccessfulItemsAsPersisted(results);

            return results;

            async Task<TValue> ProcessInIndependentOperation(TItem item)
            {
                await using var operation = await beginOperation();
                TValue value = await processItem(item);
                await operation.Complete();

                return value;
            }
        }

        #endregion

        #region Valutazione

        private static async Task EvaluateItems<TItem, TKey, TValue>(
            IReadOnlyCollection<TItem> items,
            BulkOptions options,
            Func<TItem, TKey> keySelector,
            Func<TItem, Task<TValue>> processItem,
            Func<Exception, BulkError?> mapPersistenceError,
            List<BulkItemResult<TKey, TValue>> results)
            where TItem : IRequest
        {
            var index = 0;
            foreach (TItem item in items)
            {
                BulkItemResult<TKey, TValue> result = await EvaluateItem(item, index, keySelector, processItem, mapPersistenceError);
                results.Add(result);
                index++;

                if (result.Outcome == BulkItemOutcome.Failed && options.EvaluationStrategy == BulkEvaluationStrategy.StopOnFirstFailure)
                {
                    AddNotProcessedItems(items.Skip(index), index, keySelector, results);
                    break;
                }
            }
        }

        private static async Task<BulkItemResult<TKey, TValue>> EvaluateItem<TItem, TKey, TValue>(
            TItem item,
            int index,
            Func<TItem, TKey> keySelector,
            Func<TItem, Task<TValue>> processItem,
            Func<Exception, BulkError?> mapPersistenceError)
            where TItem : IRequest
        {
            TKey key = keySelector(item);

            try
            {
                item.Validate();
            }
            catch (ValidationException exception)
            {
                return Failed<TKey, TValue>(index, key, GetValidationErrors(exception));
            }

            try
            {
                TValue value = await processItem(item);

                return new BulkItemResult<TKey, TValue>(index, key, BulkItemOutcome.Succeeded, false, value, []);
            }
            catch (Exception exception)
            {
                BulkError? error = mapPersistenceError(exception);

                if (error is null)
                {
                    throw;
                }

                return Failed<TKey, TValue>(index, key, [error]);
            }
        }

        private static async Task<TValue> ProcessWithCheckpoint<TItem, TValue>(
            TItem item,
            IApplicationOperation operation,
            Func<TItem, Task<TValue>> processItem)
        {
            await using IApplicationOperationCheckpoint checkpoint = await operation.BeginCheckpoint();
            TValue value = await processItem(item);
            await checkpoint.Complete();

            return value;
        }

        #endregion

        #region Risultati

        private static void AddNotProcessedItems<TItem, TKey, TValue>(
            IEnumerable<TItem> items,
            int firstIndex,
            Func<TItem, TKey> keySelector,
            List<BulkItemResult<TKey, TValue>> results)
        {
            var index = firstIndex;
            foreach (TItem item in items)
            {
                results.Add(new BulkItemResult<TKey, TValue>(index, keySelector(item), BulkItemOutcome.NotProcessed, false, default, []));
                index++;
            }
        }

        private static BulkItemResult<TKey, TValue> Failed<TKey, TValue>(int index, TKey key, IReadOnlyCollection<BulkError> errors)
            => new(index, key, BulkItemOutcome.Failed, false, default, errors);

        private static IReadOnlyCollection<BulkError> GetValidationErrors(ValidationException exception) =>
            [.. exception.Errors.SelectMany(error => error.Value.Select(message => new BulkError(BulkErrorKind.Validation, error.Key, message)))];

        private static BulkOutcome GetOutcome<TKey, TValue>(
            BulkPersistenceStrategy persistenceStrategy,
            IReadOnlyCollection<BulkItemResult<TKey, TValue>> results) => results.All(result => result.Persisted)
                ? BulkOutcome.Succeeded
                : persistenceStrategy == BulkPersistenceStrategy.PartialSuccess && results.Any(result => result.Persisted) ? BulkOutcome.PartiallySucceeded : BulkOutcome.Failed;

        private static void MarkSuccessfulItemsAsPersisted<TKey, TValue>(IList<BulkItemResult<TKey, TValue>> results)
        {
            for (var index = 0; index < results.Count; index++)
            {
                BulkItemResult<TKey, TValue> result = results[index];
                results[index] = result.Outcome == BulkItemOutcome.Succeeded ? result with { Persisted = true } : result;
            }
        }

        #endregion
    }
}

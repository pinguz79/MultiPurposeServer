using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Contracts.Responses;
using MultiPurposeServer.Shared.Persistence.Operations;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace Finance.Api.Application.Bulk
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

            BulkOutcome outcome = results.All(result => result.Persisted) ? BulkOutcome.Succeeded
                : options.PersistenceStrategy == BulkPersistenceStrategy.PartialSuccess && results.Any(result => result.Persisted)
                    ? BulkOutcome.PartiallySucceeded : BulkOutcome.Failed;

            return new BulkResponse<TKey, TValue>(options, outcome, results);
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

            await Evaluate(items, options, keySelector, Process, mapPersistenceError, results);

            if (results.All(result => result.Outcome == BulkItemOutcome.Succeeded))
            {
                await operation.Complete();
                MarkPersisted(results);
            }

            return results;

            async Task<TValue> Process(TItem item)
            {
                await using IApplicationOperationCheckpoint checkpoint = await operation.BeginCheckpoint();
                TValue value = await processItem(item);
                await checkpoint.Complete();

                return value;
            }
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
            await Evaluate(items, options, keySelector, Process, mapPersistenceError, results);
            MarkPersisted(results);

            return results;

            async Task<TValue> Process(TItem item)
            {
                await using var operation = await beginOperation();
                TValue value = await processItem(item);
                await operation.Complete();

                return value;
            }
        }

        #endregion

        #region Valutazione

        private static async Task Evaluate<TItem, TKey, TValue>(
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
                    foreach (TItem remainingItem in items.Skip(index))
                    {
                        results.Add(new BulkItemResult<TKey, TValue>(index++, keySelector(remainingItem), BulkItemOutcome.NotProcessed, false, default, []));
                    }

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
                BulkError[] errors = [.. exception.Errors.SelectMany(error => error.Value.Select(message => new BulkError(BulkErrorKind.Validation, error.Key, message)))];

                return new BulkItemResult<TKey, TValue>(index, key, BulkItemOutcome.Failed, false, default, errors);
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

                return new BulkItemResult<TKey, TValue>(index, key, BulkItemOutcome.Failed, false, default, [error]);
            }
        }

        #endregion

        #region Risultati

        private static void MarkPersisted<TKey, TValue>(IList<BulkItemResult<TKey, TValue>> results)
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

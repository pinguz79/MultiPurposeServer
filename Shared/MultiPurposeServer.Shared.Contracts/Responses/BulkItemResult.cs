using MultiPurposeServer.Shared.Contracts.Enums;

namespace MultiPurposeServer.Shared.Contracts.Responses
{
    public sealed record BulkItemResult<TKey, TValue>(
        int Index,
        TKey Key,
        BulkItemOutcome Outcome,
        bool Persisted,
        TValue? Value,
        IReadOnlyCollection<BulkError> Errors);
}

using MultiPurposeServer.Shared.Contracts.Enums;

namespace MultiPurposeServer.Shared.Contracts.Responses
{
    public sealed record BulkResponse<TKey, TValue>(
        BulkOptions Options,
        BulkOutcome Outcome,
        IReadOnlyCollection<BulkItemResult<TKey, TValue>> Items);
}

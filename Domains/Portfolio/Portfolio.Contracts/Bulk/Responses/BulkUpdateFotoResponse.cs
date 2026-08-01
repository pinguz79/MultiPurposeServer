using Portfolio.Contracts.Responses;

namespace Portfolio.Contracts.Bulk.Responses
{
    public sealed class BulkUpdateFotoResponse
    {
        public required IReadOnlyCollection<PhotoDto> UpdatedItems { get; init; }
        public required IReadOnlyCollection<BulkUpdateFotoWarning> Warnings { get; init; }
    }
}

using Portfolio.Contracts.Bulk.Enums;

namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateAlbumOptions(BulkErrorStrategy ErrorStrategy = BulkErrorStrategy.WarningAndContinue);
}
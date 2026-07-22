using Portfolio.Contracts.Responses;

namespace Portfolio.Contracts.Bulk.Responses
{
    public sealed class BulkUpdateAlbumResponse
    {
        public required IReadOnlyCollection<AlbumDto> UpdatedItems { get; init; }
        public required IReadOnlyCollection<BulkUpdateAlbumWarning> Warnings { get; init; }
    }
}

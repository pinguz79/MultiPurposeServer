using Portfolio.Api.Application.Options;

namespace Portfolio.Api.Application.Diagnostics
{
    public enum AlbumSyncStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }

    public sealed class AlbumSyncReport
    {
        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }
        public MissingPhotoStrategy Strategy { get; init; }
        public AlbumSyncStatus Status { get; set; } = AlbumSyncStatus.Healthy;
        public int AlbumsCreated { get; set; }
        public int FoldersCreated { get; set; }
        public int PhotosCreated { get; set; }
        public int MissingPhotos { get; set; }
        public int PhotosDeleted { get; set; }
        public List<AlbumSyncFinding> Findings { get; init; } = [];
    }

    public sealed record AlbumSyncFinding(
        string Type,
        string Severity,
        Guid AlbumId,
        Guid? PhotoId,
        string ExpectedPath,
        string Message,
        string Action);
}

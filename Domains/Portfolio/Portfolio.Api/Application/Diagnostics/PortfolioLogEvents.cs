using MultiPurposeServer.Shared.Logging.Models;

namespace Portfolio.Api.Application.Diagnostics
{
    public static class PortfolioLogEvents
    {
        public static LogEventId AlbumCreated { get; } = new("Portfolio.Album.Created");

        public static LogEventId AlbumCreationPathResolved { get; } = new("Portfolio.Album.CreationPathResolved");

        public static LogEventId AlbumHierarchyIncomplete { get; } = new("Portfolio.Album.HierarchyIncomplete");

        public static LogEventId AlbumSynchronizationCompleted { get; } = new("Portfolio.Album.SynchronizationCompleted");

        public static LogEventId FaceDetectionFallbackActivated { get; } = new("Portfolio.Media.FaceDetectionFallbackActivated");

        public static LogEventId FaceDetectionFailed { get; } = new("Portfolio.Media.FaceDetectionFailed");
    }
}

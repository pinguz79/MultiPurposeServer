namespace Portfolio.Api.Application.Diagnostics
{
    public interface IAlbumSyncReportStore
    {
        Task<AlbumSyncReport?> Read();
        Task Write(AlbumSyncReport report);
    }
}

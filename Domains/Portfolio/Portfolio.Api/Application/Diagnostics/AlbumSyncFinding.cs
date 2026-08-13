namespace Portfolio.Api.Application.Diagnostics
{
    public sealed record AlbumSyncFinding(
        string Type,
        string Severity,
        Guid AlbumId,
        Guid? PhotoId,
        string ExpectedPath,
        string Message,
        string Action);
}

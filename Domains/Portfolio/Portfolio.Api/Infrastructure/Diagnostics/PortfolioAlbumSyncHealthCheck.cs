using Microsoft.Extensions.Diagnostics.HealthChecks;

using Portfolio.Api.Application.Diagnostics;

namespace Portfolio.Api.Infrastructure.Diagnostics
{
    public sealed class PortfolioAlbumSyncHealthCheck(IAlbumSyncReportStore reportStore) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var report = await reportStore.Read();

            if (report is null)
            {
                return HealthCheckResult.Degraded("No album synchronization report is available yet.");
            }

            var description = $"Album synchronization completed with {report.MissingPhotos} missing photos, {report.PhotosDeleted} deletions and {report.Findings.Count} findings.";

            return report.Status switch
            {
                AlbumSyncStatus.Healthy => HealthCheckResult.Healthy(description),
                AlbumSyncStatus.Degraded => HealthCheckResult.Degraded(description),
                _ => HealthCheckResult.Unhealthy(description)
            };
        }
    }
}

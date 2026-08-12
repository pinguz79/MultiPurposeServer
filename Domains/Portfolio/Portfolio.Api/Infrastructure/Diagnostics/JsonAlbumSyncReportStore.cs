using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Portfolio.Api.Application.Diagnostics;
using Portfolio.Api.Application.Options;

namespace Portfolio.Api.Infrastructure.Diagnostics
{
    public sealed class JsonAlbumSyncReportStore : IAlbumSyncReportStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _reportPath;

        public JsonAlbumSyncReportStore(
            IOptions<PortfolioAlbumOptions> options,
            IHostEnvironment environment)
        {
            _reportPath = Path.GetFullPath(options.Value.SyncReportPath, environment.ContentRootPath);
        }

        public async Task<AlbumSyncReport?> Read()
        {
            if (!File.Exists(_reportPath))
            {
                return null;
            }

            await using var stream = File.OpenRead(_reportPath);
            return await JsonSerializer.DeserializeAsync<AlbumSyncReport>(stream, SerializerOptions);
        }

        public async Task Write(AlbumSyncReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_reportPath)!);
            var temporaryPath = $"{_reportPath}.{Guid.NewGuid():N}.tmp";

            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, report, SerializerOptions);
            }

            File.Move(temporaryPath, _reportPath, true);
        }
    }
}

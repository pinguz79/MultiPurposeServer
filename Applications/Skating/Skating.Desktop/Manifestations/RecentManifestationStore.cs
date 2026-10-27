using System.Text.Json;

namespace Skating.Desktop.Manifestations
{
    internal sealed class RecentManifestationStore(string filePath)
    {
        private const int MaximumEntries = 4;

        internal IReadOnlyList<RecentManifestation> Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return [];
                }

                var entries = JsonSerializer.Deserialize<List<RecentManifestation?>>(File.ReadAllText(filePath)) ?? [];
                return [.. entries.OfType<RecentManifestation>().Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && !string.IsNullOrWhiteSpace(entry.Path) && Path.IsPathFullyQualified(entry.Path))
                    .Select(entry => new RecentManifestation(entry.Name, Path.GetFullPath(entry.Path)))
                    .DistinctBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase).Take(MaximumEntries)];
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException or NotSupportedException)
            {
                // Le preferenze locali non devono impedire di aprire un archivio della manifestazione.
                return [];
            }
        }

        internal void Remember(string name, string archivePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var path = Path.GetFullPath(archivePath);
            var entries = Load().Where(entry => !string.Equals(entry.Path, path, StringComparison.OrdinalIgnoreCase)).Prepend(new RecentManifestation(name, path)).Take(MaximumEntries);
            var destination = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(destination) ?? throw new InvalidOperationException("Cartella delle preferenze non valida.");
            Directory.CreateDirectory(directory);
            var temporary = Path.Combine(directory, $"recent-{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(entries));
                File.Move(temporary, destination, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }
    }
}

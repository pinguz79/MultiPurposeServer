using System.Text.Json;

using Skating.Core;
using Skating.Desktop.Configuration;
using Skating.Desktop.Manifestations;
using Skating.Desktop.Shell;
using Skating.Persistence;

namespace Skating.Desktop
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            AwardAggregation? aggregation;
            try
            {
                aggregation = AwardOptions.Load(Path.Combine(AppContext.BaseDirectory, "skating-settings.json")).IntermediateAwardAggregation;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                MessageBox.Show($"Configurazione premi non disponibile: {exception.Message}\r\nLe altre funzionalità restano disponibili.", "Skating System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                aggregation = null;
            }
            var recentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkatingSystem", "recent-manifestations.json");
            Application.Run(new MainForm(path => new SqliteManifestationStore(path), path => new SqliteRegistryStore(path), path => new SqliteChoreographyStore(path), path => new SqliteRaceStore(path),
                new RecentManifestationStore(recentPath), path => new SqlitePerformanceStore(path, aggregation), new ScorePreferencesStore(Path.ChangeExtension(recentPath, "scores.json")),
                path => new SqliteIntermediateAwardStore(path), aggregation, path => new SqliteAbsoluteAwardStore(path)));
        }
    }
}

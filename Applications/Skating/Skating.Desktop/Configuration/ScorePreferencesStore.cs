using System.Text.Json;

namespace Skating.Desktop.Configuration
{
    internal sealed class ScorePreferencesStore(string path)
    {
        public ScorePreferences Load()
        {
            try
            {
                return JsonSerializer.Deserialize<ScorePreferences>(File.ReadAllText(path)) ?? new ScorePreferences();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                return new ScorePreferences();
            }
        }

        public void Save(ScorePreferences preferences)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? throw new ArgumentException("Percorso preferenze non valido.", nameof(path)));
            var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(preferences));
                File.Move(temporary, path, true);
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

using System.Text.Json;

namespace Finance.Desktop.Configuration
{
    public sealed class AppConfiguration
    {
        public ApiConfiguration Api { get; set; } = new();

        public static AppConfiguration Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var configuration = JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText(path), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            return configuration ?? throw new InvalidOperationException("Finance.Desktop configuration is invalid.");
        }
    }
}

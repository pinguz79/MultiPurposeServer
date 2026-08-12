using Microsoft.Extensions.Logging;

namespace SampleApp.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // In sviluppo le credenziali Google possono essere fornite come MauiAsset nella cartella Secrets.
            try
            {
                var secretsPath = Path.Combine(AppContext.BaseDirectory, "Secrets");
                if (Directory.Exists(secretsPath))
                {
                    var files = Directory.GetFiles(secretsPath, "client_secret*.json");
                    if (files.Length > 0)
                    {
                        var json = File.ReadAllText(files[0]);
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        var cfg = root.TryGetProperty("installed", out var installed) ? installed : (root.TryGetProperty("web", out var web) ? web : default);
                        if (cfg.ValueKind != System.Text.Json.JsonValueKind.Undefined && cfg.TryGetProperty("client_id", out var cid))
                        {
                            builder.Configuration["Google:ClientId"] = cid.GetString();
                        }
                    }
                }
            }
            catch
            {
                // Le credenziali Google sono opzionali durante l'avvio dell'applicazione di esempio.
            }

            return builder.Build();
        }
    }
}

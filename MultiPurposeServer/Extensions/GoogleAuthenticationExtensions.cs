using Serilog;
using System.Text.Json;

namespace MultiPurposeServer.Extensions
{
    public static class GoogleAuthenticationExtensions
    {
        public static void AddGoogleClientSecrets(this WebApplicationBuilder builder)
        {
            // Expect files named like: client_secret_{AppName}.json
            try
            {
                var secretsDir = Path.Combine(builder.Environment.ContentRootPath, "Secrets");
                if (Directory.Exists(secretsDir))
                {
                    var files = Directory.GetFiles(secretsDir, "client_secret_*.json");
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file); // client_secret_{AppName}
                            var parts = fileName.Split('_', 3);
                            var appName = parts.Length >= 3 ? parts[2] : parts.Length == 2 ? parts[1] : fileName;
                            Log.Information($"Found Google secret file: {fileName}");

                            var json = File.ReadAllText(file);
                            using var doc = JsonDocument.Parse(json);
                            var root = doc.RootElement;
                            var cfg = root.TryGetProperty("installed", out var installed) ? installed : root.TryGetProperty("web", out var web) ? web : default;
                            if (cfg.ValueKind != JsonValueKind.Undefined)
                            {
                                if (cfg.TryGetProperty("client_id", out var cid))
                                    builder.Configuration[$"Authentication:Google:{appName}:ClientId"] = cid.GetString();
                                if (cfg.TryGetProperty("client_secret", out var cs))
                                    builder.Configuration[$"Authentication:Google:{appName}:ClientSecret"] = cs.GetString();
                            }
                        }
                        catch (Exception inner)
                        {
                            Log.Warning(inner, $"Failed to parse Google client file {file}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not load Google client JSON files from Secrets folder");
            }
        }
    }
}

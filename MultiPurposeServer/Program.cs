using MultiPurposeServer.Extensions;
using Portfolio.Api.Extensions;
using Serilog;
using System.Text.Json;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog for file logging early
        var logsPath = Path.Combine(builder.Environment.ContentRootPath ?? AppContext.BaseDirectory, "Logs", "mps-.log");
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
            .CreateLogger();

        builder.Host.UseSerilog();

        // Early logger for startup diagnostics (use Serilog static API)

        // Load Google client JSON files from MultiPurposeServer/Secrets
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
                        Log.Information("Found Google secret file: {FileName}", fileName);

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
                        Log.Warning(inner, "Failed to parse Google client file {File}", file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not load Google client JSON files from Secrets folder");
        }

        // Log startup info via logger
        try
        {
            Log.Information("Startup: SampleAppMobile_ClientId={ClientId}", builder.Configuration["Authentication:Google:SampleApp.Mobile:ClientId"] ?? "(missing)");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error while logging startup info");
        }

        // Add services to the container.
        builder.Services.AddControllers()
               .AddApplicationPart(typeof(Portfolio.Api.AssemblyReference).Assembly)
               .AddControllersAsServices(); 
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.AddPortfolioSecurity();
        });

        builder.Services.AddDatabase();
        builder.Services.AddPortfolioApi(builder.Configuration.GetSection("Portfolio"));
        builder.Services.AddPortfolioAuthentication(builder.Configuration.GetSection("Portfolio"));

        // CORS - adjust origins for production
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // If the app is hosted under a virtual application path, set PathBase from configuration
        var pathBase = builder.Configuration["PathBase"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            if (!pathBase.StartsWith("/")) pathBase = $"/{pathBase}";
            if (pathBase.EndsWith("/")) pathBase = pathBase.TrimEnd('/');
            app.UsePathBase(pathBase);
        }

        // Configure the HTTP request pipeline.
        var enableSwagger = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger");
        Log.Information("EnableSwagger={EnableSwagger}, Environment={Environment}, PathBase={PathBase}", enableSwagger, app.Environment.EnvironmentName, pathBase ?? "");

        if (enableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // Serve the UI at /swagger. If app is hosted under a PathBase, the endpoint will include it.
                var endpoint = string.IsNullOrEmpty(pathBase) ? "/swagger/v1/swagger.json" : $"{pathBase}/swagger/v1/swagger.json";
                options.SwaggerEndpoint(endpoint, "MPS API V1");
                options.RoutePrefix = "swagger"; // serve at /swagger
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.UsePortfolioAsync(); // no-op to force file save after adding using

        app.Run();
    }
}
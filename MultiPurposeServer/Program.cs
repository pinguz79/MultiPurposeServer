using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using MultiPurposeServer.Diagnostics;
using MultiPurposeServer.Extensions;
using MultiPurposeServer.Middleware;
using MultiPurposeServer.Shared.Logging.Extensions;
using MultiPurposeServer.Shared.Logging.Models;

using Portfolio.Api.Extensions;

namespace MultiPurposeServer
{
    public partial class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddMultiPurposeLogging();
            var diagnosticOptions = builder.Configuration.GetSection("Logging:Diagnostics").Get<DiagnosticOptions>() ?? new DiagnosticOptions();
            builder.Services.AddSharedLogging(diagnosticOptions);
            builder.AddGoogleClientSecrets();

            builder.Services.AddMultiPurposeControllers();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddMultiPurposeOpenApi();
            builder.Services.AddPortfolio(builder.Configuration.GetSection("Portfolio"), builder.Environment);
            builder.Services.AddMultiPurposeCors();

            var app = builder.Build();
            var pathBase = UseConfiguredPathBase(app);

            app.UseMultiPurposeOpenApi(builder.Configuration.GetValue<bool>("EnableOpenApi"), pathBase);
            app.UseMiddleware<LoggingContextMiddleware>();
            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseMultiPurposeCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseHealthChecks("/health/portfolio/albums", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("portfolio")
            });

            await app.UsePortfolioAsync();

            app.Run();
        }

        private static string? UseConfiguredPathBase(WebApplication app)
        {
            var pathBase = app.Configuration["PathBase"];

            if (string.IsNullOrWhiteSpace(pathBase))
            {
                return null;
            }

            pathBase = $"/{pathBase.Trim().Trim('/')}";

            if (pathBase == "/")
            {
                return null;
            }

            app.UsePathBase(pathBase);

            return pathBase;
        }
    }
}

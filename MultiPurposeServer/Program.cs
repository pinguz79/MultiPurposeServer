using MultiPurposeServer.Extensions;
using Portfolio.Api.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddMultiPurposeLogging();
        builder.AddGoogleClientSecrets();

        builder.Services.AddMultiPurposeControllers();
        builder.Services.AddMultiPurposeOpenApi();
        builder.Services.AddPortfolio(builder.Configuration.GetSection("Portfolio"), builder.Environment);
        builder.Services.AddMultiPurposeCors();

        var app = builder.Build();
        var pathBase = UseConfiguredPathBase(app);

        app.UseMultiPurposeOpenApi(builder.Configuration.GetValue<bool>("EnableOpenApi"), pathBase);
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

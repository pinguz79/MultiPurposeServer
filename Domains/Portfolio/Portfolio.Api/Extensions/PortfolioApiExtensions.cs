using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Api.Repositories;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Options;
using Portfolio.Data;

namespace Portfolio.Api.Extensions;

public static class PortfolioApiExtensions
{
    private static IConfigurationSection _configuration;

    public static void AddPortfolioApi(this IServiceCollection services, IConfigurationSection configuration)
    {
        _configuration = configuration;
        services.AddDbContext<PortfolioContext>(options =>
            options.UseLazyLoadingProxies()
            .UseSqlite(_configuration.GetConnectionString("PortfolioDatabase")));

        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IFotoRepository, FotoRepository>();

        services.AddScoped<IAlbumService, AlbumService>();
        services.AddScoped<IFotoService, FotoService>();
        services.AddScoped<IMediaService, MediaService>();

        services.Configure<PortfolioMediaOptions>(_configuration.GetSection(PortfolioMediaOptions.SectionName));
        services.Configure<PortfolioCacheOptions>(_configuration.GetSection(PortfolioCacheOptions.SectionName));

        services.AddHttpClient<ICacheService, CacheService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PortfolioCacheOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("PortfolioCache:BaseUrl is required.");
            }

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    public static async Task UsePortfolioAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
            var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();

            // Use synchronous Migrate to avoid missing async migration extension in some EF packages
            dbContext.Database.Migrate();
            await albumService.AmendDirectoryTree();
        }
    }
}

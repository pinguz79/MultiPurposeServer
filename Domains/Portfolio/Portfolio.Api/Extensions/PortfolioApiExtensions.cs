using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Portfolio.Data;
using Portfolio.Api.Repositories;
using Portfolio.Api.Services;

namespace Portfolio.Api.Extensions;

public static class PortfolioApiExtensions
{
    private static IConfigurationSection _configuration;

    public static void AddPortfolioApi(this IServiceCollection services, IConfigurationSection configuration)
    {
        _configuration = configuration;
        services.AddDbContext<PortfolioContext>(options =>
        options.UseLazyLoadingProxies().UseSqlite(_configuration.GetConnectionString("PortfolioDatabase")));

        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IFotoRepository, FotoRepository>();
        services.AddScoped<IAlbumService, AlbumService>();
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

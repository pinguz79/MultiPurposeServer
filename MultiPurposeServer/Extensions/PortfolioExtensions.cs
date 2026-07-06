using Microsoft.EntityFrameworkCore;
using MultiPurposeServer.DBContexts.Portfolio;
using MultiPurposeServer.Microservices.Portfolio;
using MultiPurposeServer.Repositories.Portfolio;

namespace MultiPurposeServer.Extensions
{
    public static class PortfolioExtensions
    {
        private static IConfigurationSection _configuration;
        public static void AddPortfolio(this IServiceCollection services, IConfigurationSection configuration)
        {
            _configuration = configuration;
            services.AddDbContext<PortfolioContext>(options =>
                options.UseLazyLoadingProxies()
                .UseSqlite(_configuration.GetConnectionString("PortfolioDatabase")));
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddTransient<IAlbumRepository, AlbumRepository>();
            services.AddTransient<IFotoRepository, FotoRepository>();
        }

        public static async Task UsePortfolioAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();

                await dbContext.Database.MigrateAsync();
                await albumService.AmendDirectoryTree();
            }
        }
    }
}

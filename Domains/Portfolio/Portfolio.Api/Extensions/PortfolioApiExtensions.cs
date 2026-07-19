using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Api.Authentication;
using Portfolio.Api.Repositories;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Options;
using Portfolio.Data;

namespace Portfolio.Api.Extensions
{
    public static class PortfolioApiExtensions
    {
        public static void AddPortfolio(this IServiceCollection services, IConfigurationSection configuration)
        {
            AddAuthentication(services, configuration);
            AddApiServices(services, configuration);
        }

        private static void AddAuthentication(IServiceCollection services, IConfigurationSection configuration)
        {
            services.Configure<PortfolioAuthenticationOptions>(configuration.GetSection(PortfolioAuthenticationOptions.SectionName));

            services
                .AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, PortfolioApiKeyAuthenticationHandler>(
                    PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme,
                    _ => { });

            services.AddAuthorizationBuilder()
                .AddPolicy(PortfolioPolicies.FrontEnd, policy =>
                {
                    policy.AddAuthenticationSchemes(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        PortfolioApiKeyAuthenticationHandler.AccessClaimType,
                        PortfolioApiKeyAuthenticationHandler.FrontEndAccess,
                        PortfolioApiKeyAuthenticationHandler.BackEndAccess);
                })
                .AddPolicy(PortfolioPolicies.BackEnd, policy =>
                {
                    policy.AddAuthenticationSchemes(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        PortfolioApiKeyAuthenticationHandler.AccessClaimType,
                        PortfolioApiKeyAuthenticationHandler.BackEndAccess);
                });
        }

        private static void AddApiServices(IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddDbContext<PortfolioContext>(options => options.UseLazyLoadingProxies().UseSqlite(configuration.GetConnectionString("PortfolioDatabase")));

            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IFotoRepository, FotoRepository>();

            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IFotoService, FotoService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IImageResizer, ImageMagickResizer>();

            services.Configure<PortfolioMediaOptions>(configuration.GetSection(PortfolioMediaOptions.SectionName));
            services.Configure<PortfolioCacheOptions>(configuration.GetSection(PortfolioCacheOptions.SectionName));
            services.Configure<PortfolioAlbumOptions>(configuration.GetSection(PortfolioAlbumOptions.SectionName));

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
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Authentication;
using Portfolio.Api.Filters;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Api.Services;
using Portfolio.Data;

namespace Portfolio.Api.Extensions
{
    public static class PortfolioApiExtensions
    {
        public static void AddPortfolio(this IServiceCollection services, IConfigurationSection configuration, IHostEnvironment environment)
        {
            AddAuthentication(services, configuration, environment);
            AddApiServices(services, configuration);
        }

        private static void AddAuthentication(IServiceCollection services, IConfigurationSection configuration, IHostEnvironment environment)
        {
            services.Configure<PortfolioAuthenticationOptions>(configuration.GetSection(PortfolioAuthenticationOptions.SectionName));

            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, PortfolioApiKeyAuthenticationHandler>(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme, _ => { });

            services.AddAuthorizationBuilder()
                .AddPolicy(PortfolioPolicies.FrontEnd, policy => ConfigurePolicy(policy, environment, PortfolioApiKeyAuthenticationHandler.FrontEndAccess, PortfolioApiKeyAuthenticationHandler.BackEndAccess))
                .AddPolicy(PortfolioPolicies.BackEnd, policy => ConfigurePolicy(policy, environment, PortfolioApiKeyAuthenticationHandler.BackEndAccess));
        }

        private static void ConfigurePolicy(AuthorizationPolicyBuilder policy, IHostEnvironment environment, params string[] accesses)
        {
            if (environment.IsDevelopment())
            {
                policy.RequireAssertion(_ => true);
                return;
            }

            policy.AddAuthenticationSchemes(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(PortfolioApiKeyAuthenticationHandler.AccessClaimType, accesses);
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

            services.AddScoped<RequestNormalizationValidationFilter>();
            services.AddScoped<ValidationExceptionFilter>();
            services.AddControllers(options =>
            {
                options.Filters.AddService<RequestNormalizationValidationFilter>();
                options.Filters.AddService<ValidationExceptionFilter>();
            });

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
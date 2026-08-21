using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Extensions;
using MultiPurposeServer.Shared.Logging.Models;
using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Transactions;

using Portfolio.Api.Application.Diagnostics;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Authentication;
using Portfolio.Api.Filters;
using Portfolio.Api.Infrastructure.Clients;
using Portfolio.Api.Infrastructure.Diagnostics;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.DataModel;

namespace Portfolio.Api.Extensions
{
    public static class PortfolioApiExtensions
    {

        public static void AddPortfolio(this IServiceCollection services, IConfigurationSection configuration, IHostEnvironment environment)
        {
            services.AddSharedLogging();
            AddAuthentication(services, configuration, environment);
            AddDbContext(services, configuration);
            AddRepositories(services);
            AddServices(services, configuration);
            AddPipelineFilters(services);
            ConfigureOptions(services, configuration, environment);
            AddHttpClient(services);
        }


        #region Autenticazione e policy

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

        #endregion

        #region Registrazione servizi

        private static void AddServices(IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IFotoService, FotoService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IImageResizer, ImageMagickResizer>();
            services.AddSingleton<ICropFocusDetector, OnnxFaceCropFocusDetector>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddSingleton<IAlbumSyncReportStore, JsonAlbumSyncReportStore>();
            services.AddHealthChecks().AddCheck<PortfolioAlbumSyncHealthCheck>("portfolio-album-sync", tags: ["portfolio"]);
        }

        private static void AddHttpClient(IServiceCollection services)
        {
            services.AddHttpClient<IPortfolioWebCacheClient, PortfolioWebCacheHttpClient>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PortfolioCacheOptions>>().Value;

                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
        }

        private static void ConfigureOptions(IServiceCollection services, IConfigurationSection configuration, IHostEnvironment environment)
        {
            services.AddOptions<PortfolioAuthenticationOptions>()
                .Bind(configuration.GetSection(PortfolioAuthenticationOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.HeaderName), "PortfolioAuthentication:HeaderName is required.")
                .Validate(options => environment.IsDevelopment() || !string.IsNullOrWhiteSpace(options.FrontEndKey),
                    "PortfolioAuthentication:FrontEndKey is required outside Development.")
                .Validate(options => environment.IsDevelopment() || !string.IsNullOrWhiteSpace(options.BackEndKey),
                    "PortfolioAuthentication:BackEndKey is required outside Development.")
                .ValidateOnStart();

            services.AddOptions<PortfolioMediaOptions>()
                .Bind(configuration.GetSection(PortfolioMediaOptions.SectionName))
                .PostConfigure(options => options.RootPath = environment.ContentRootPath)
                .Validate(options => !string.IsNullOrWhiteSpace(options.RootPath), "PortfolioMedia:RootPath is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.OriginalsRoot), "PortfolioMedia:OriginalsRoot is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.CacheRoot), "PortfolioMedia:CacheRoot is required.")
                .Validate(options => options.CoverWidth > 0 && options.CoverHeight > 0, "PortfolioMedia cover dimensions must be greater than zero.")
                .Validate(options => options.EditorialCoverWidth > 0 && options.EditorialCoverHeight > 0, "PortfolioMedia editorial cover dimensions must be greater than zero.")
                .Validate(options => options.ThumbnailWidth > 0 && options.ThumbnailHeight > 0, "PortfolioMedia thumbnail dimensions must be greater than zero.")
                .Validate(options => options.ImageWidth > 0 && options.ImageHeight > 0, "PortfolioMedia image dimensions must be greater than zero.")
                .Validate(options => !options.SmartCropEnabled || !string.IsNullOrWhiteSpace(options.FaceDetectionModelPath), "PortfolioMedia:FaceDetectionModelPath is required when smart crop is enabled.")
                .Validate(options => options.FaceDetectionConfidence is > 0 and <= 1, "PortfolioMedia:FaceDetectionConfidence must be between zero and one.")
                .ValidateOnStart();

            services.AddOptions<PortfolioCacheOptions>()
                .Bind(configuration.GetSection(PortfolioCacheOptions.SectionName))
                .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                    "PortfolioCache:BaseUrl must be an absolute HTTP or HTTPS URI.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ClearEndpoint), "PortfolioCache:ClearEndpoint is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.SharedSecret), "PortfolioCache:SharedSecret is required.")
                .ValidateOnStart();

            services.AddOptions<PortfolioAlbumOptions>()
                .Bind(configuration.GetSection(PortfolioAlbumOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.RootPath), "Albums:RootPath cannot be empty.")
                .Validate(options => options.MaxMissingPhotoDeletions >= 0, "Albums:MaxMissingPhotoDeletions cannot be negative.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.SyncReportPath), "Albums:SyncReportPath cannot be empty.")
                .ValidateOnStart();
        }

        private static void AddPipelineFilters(IServiceCollection services)
        {
            services.AddScoped<KeyNotFoundExceptionFilter>();
            services.AddScoped<RequestNormalizationValidationFilter>();
            services.AddScoped<ValidationExceptionFilter>();
            services.AddControllers(options =>
            {
                options.Filters.AddService<KeyNotFoundExceptionFilter>();
                options.Filters.AddService<RequestNormalizationValidationFilter>();
                options.Filters.AddService<ValidationExceptionFilter>();
            });
        }

        #endregion

        #region Configurazione persistenza

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IPersistenceCoordinator, EntityFrameworkPersistenceCoordinator<PortfolioContext>>();
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IFotoRepository, FotoRepository>();
        }

        private static void AddDbContext(IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddDbContext<PortfolioContext>(options => options.UseLazyLoadingProxies().UseSqlite(configuration.GetConnectionString("PortfolioDatabase")));
        }

        #endregion

        public static async Task UsePortfolioAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var loggingContext = new LoggingContext("Portfolio");
                var loggingScope = new Dictionary<string, object?>
                {
                    ["Domain"] = loggingContext.Domain,
                    ["CorrelationId"] = loggingContext.CorrelationId,
                    ["RequestId"] = loggingContext.RequestId,
                    ["Origin"] = loggingContext.Origin,
                };
                var contextAccessor = scope.ServiceProvider.GetRequiredService<ILoggingContextAccessor>();
                var frameworkLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Portfolio.Bootstrap");

                using var context = contextAccessor.Push(loggingContext);
                using var loggerScope = frameworkLogger.BeginScope(loggingScope);
                var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                var albumService = scope.ServiceProvider.GetRequiredService<IAlbumService>();

                // Alcuni pacchetti EF in uso non espongono l'estensione asincrona per le migrazioni.
                dbContext.Database.Migrate();
                await albumService.AmendDirectoryTree();
            }
        }

    }
}

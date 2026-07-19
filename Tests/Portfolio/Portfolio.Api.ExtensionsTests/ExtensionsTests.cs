using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Api.Authentication;
using Portfolio.Api.Extensions;
using Portfolio.Api.Repositories;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Options;
using Portfolio.Data;

namespace Portfolio.Api.ExtensionsTests
{
    public class ExtensionsTests
    {
        [Fact]
        public async Task AddPortfolio_WhenCalled_RegistersAuthenticationScheme()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            // Act
            services.AddPortfolio(configuration);
            await using var serviceProvider = services.BuildServiceProvider();

            var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);

            // Assert
            scheme.Should().NotBeNull();
            scheme!.Name.Should().Be(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
            scheme.HandlerType.Should().Be<PortfolioApiKeyAuthenticationHandler>();
        }

        [Fact]
        public void AddPortfolio_WhenCalled_BindsAuthenticationOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(
            [
                new($"{PortfolioAuthenticationOptions.SectionName}:HeaderName", "X-Custom-Portfolio-Key"),
                new($"{PortfolioAuthenticationOptions.SectionName}:FrontEndKey", "front-end-key"),
                new($"{PortfolioAuthenticationOptions.SectionName}:BackEndKey", "back-end-key")
            ]);

            // Act
            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<PortfolioAuthenticationOptions>>().Value;

            // Assert
            options.HeaderName.Should().Be("X-Custom-Portfolio-Key");
            options.FrontEndKey.Should().Be("front-end-key");
            options.BackEndKey.Should().Be("back-end-key");
        }

        [Fact]
        public async Task AddPortfolio_WhenCalled_ConfiguresFrontEndPolicy()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();

            var policyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();

            // Act
            var policy = await policyProvider.GetPolicyAsync(PortfolioPolicies.FrontEnd);

            // Assert
            policy.Should().NotBeNull();
            policy!.AuthenticationSchemes.Should().ContainSingle().Which.Should().Be(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
            policy.Requirements.Should().ContainSingle(requirement => requirement is DenyAnonymousAuthorizationRequirement);

            var claimRequirement = policy.Requirements.OfType<ClaimsAuthorizationRequirement>().Should().ContainSingle().Subject;

            claimRequirement.ClaimType.Should().Be(PortfolioApiKeyAuthenticationHandler.AccessClaimType);
            claimRequirement.AllowedValues.Should().BeEquivalentTo(
            [
                PortfolioApiKeyAuthenticationHandler.FrontEndAccess,
                PortfolioApiKeyAuthenticationHandler.BackEndAccess
            ]);
        }

        [Fact]
        public async Task AddPortfolio_WhenCalled_ConfiguresBackEndPolicy()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();

            var policyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();

            // Act
            var policy = await policyProvider.GetPolicyAsync(PortfolioPolicies.BackEnd);

            // Assert
            policy.Should().NotBeNull();
            policy!.AuthenticationSchemes.Should().ContainSingle().Which.Should().Be(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
            policy.Requirements.Should().ContainSingle(requirement => requirement is DenyAnonymousAuthorizationRequirement);

            var claimRequirement = policy.Requirements.OfType<ClaimsAuthorizationRequirement>().Should().ContainSingle().Subject;

            claimRequirement.ClaimType.Should().Be(PortfolioApiKeyAuthenticationHandler.AccessClaimType);
            claimRequirement.AllowedValues.Should().ContainSingle().Which.Should().Be(PortfolioApiKeyAuthenticationHandler.BackEndAccess);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_RegistersRepositories()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            // Act
            services.AddPortfolio(configuration);

            // Assert
            AssertScopedRegistration<IAlbumRepository, AlbumRepository>(services);
            AssertScopedRegistration<IFotoRepository, FotoRepository>(services);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            // Act
            services.AddPortfolio(configuration);

            // Assert
            AssertScopedRegistration<IAlbumService, AlbumService>(services);
            AssertScopedRegistration<IFotoService, FotoService>(services);
            AssertScopedRegistration<IMediaService, MediaService>(services);
            AssertScopedRegistration<IImageResizer, ImageMagickResizer>(services);

            services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(ICacheService) && descriptor.Lifetime == ServiceLifetime.Transient);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_BindsCacheOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(
            [
                new($"{PortfolioCacheOptions.SectionName}:BaseUrl", "https://portfolio.example/"),
                new($"{PortfolioCacheOptions.SectionName}:SharedSecret", "shared-secret"),
                new($"{PortfolioCacheOptions.SectionName}:ClearEndpoint", "internal/cache/clear")
            ]);

            // Act
            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<PortfolioCacheOptions>>().Value;

            // Assert
            options.BaseUrl.Should().Be("https://portfolio.example/");
            options.SharedSecret.Should().Be("shared-secret");
            options.ClearEndpoint.Should().Be("internal/cache/clear");
        }

        [Fact]
        public void AddPortfolio_WhenCalled_BindsMediaOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(
            [
                new($"{PortfolioMediaOptions.SectionName}:OriginalsRoot", "media/originals"),
                new($"{PortfolioMediaOptions.SectionName}:CacheRoot", "media/cache"),
                new($"{PortfolioMediaOptions.SectionName}:ImageWidth", "1600"),
                new($"{PortfolioMediaOptions.SectionName}:ImageHeight", "1200")
            ]);

            // Act
            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<PortfolioMediaOptions>>().Value;

            // Assert
            options.OriginalsRoot.Should().Be("media/originals");
            options.CacheRoot.Should().Be("media/cache");
            options.ImageWidth.Should().Be(1600);
            options.ImageHeight.Should().Be(1200);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_ConfiguresPortfolioDatabaseConnection()
        {
            // Arrange
            const string connectionString = "Data Source=portfolio-tests.db";
            var services = new ServiceCollection();
            var configuration = CreateConfiguration([new("ConnectionStrings:PortfolioDatabase", connectionString)]);

            // Act
            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();

            // Assert
            context.Database.GetConnectionString().Should().Be(connectionString);
        }

        [Fact]
        public void AddPortfolio_WhenCalledWithDifferentConfigurations_DoesNotShareConfigurationState()
        {
            // Arrange
            const string firstConnectionString = "Data Source=first.db";
            const string secondConnectionString = "Data Source=second.db";

            var firstServices = new ServiceCollection();
            var secondServices = new ServiceCollection();

            var firstConfiguration = CreateConfiguration([new("ConnectionStrings:PortfolioDatabase", firstConnectionString)]);
            var secondConfiguration = CreateConfiguration([new("ConnectionStrings:PortfolioDatabase", secondConnectionString)]);

            // Act
            firstServices.AddPortfolio(firstConfiguration);
            secondServices.AddPortfolio(secondConfiguration);

            using var firstProvider = firstServices.BuildServiceProvider();
            using var secondProvider = secondServices.BuildServiceProvider();
            using var firstScope = firstProvider.CreateScope();
            using var secondScope = secondProvider.CreateScope();

            var firstContext = firstScope.ServiceProvider.GetRequiredService<PortfolioContext>();
            var secondContext = secondScope.ServiceProvider.GetRequiredService<PortfolioContext>();

            // Assert
            firstContext.Database.GetConnectionString().Should().Be(firstConnectionString);
            secondContext.Database.GetConnectionString().Should().Be(secondConnectionString);
        }

        [Fact]
        public void AddPortfolio_WhenCacheBaseUrlIsMissing_ThrowsWhenCacheServiceIsResolved()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(
            [
                new($"{PortfolioCacheOptions.SectionName}:BaseUrl", string.Empty),
                new($"{PortfolioCacheOptions.SectionName}:SharedSecret", "shared-secret"),
                new($"{PortfolioCacheOptions.SectionName}:ClearEndpoint", "internal/cache/clear")
            ]);

            services.AddPortfolio(configuration);
            using var serviceProvider = services.BuildServiceProvider();

            // Act
            var action = () => serviceProvider.GetRequiredService<ICacheService>();

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("PortfolioCache:BaseUrl is required.");
        }

        private static IConfigurationSection CreateConfiguration(IEnumerable<KeyValuePair<string, string?>>? values = null)
        {
            var configurationValues = new Dictionary<string, string?>
            {
                ["Portfolio:ConnectionStrings:PortfolioDatabase"] = "Data Source=:memory:",
                [$"Portfolio:{PortfolioCacheOptions.SectionName}:BaseUrl"] = "https://localhost/",
                [$"Portfolio:{PortfolioCacheOptions.SectionName}:SharedSecret"] = "shared-secret",
                [$"Portfolio:{PortfolioCacheOptions.SectionName}:ClearEndpoint"] = "internal/cache/clear"
            };

            if (values is not null)
            {
                foreach (var value in values)
                {
                    configurationValues[$"Portfolio:{value.Key}"] = value.Value;
                }
            }

            return new ConfigurationBuilder().AddInMemoryCollection(configurationValues).Build().GetSection("Portfolio");
        }

        private static void AssertScopedRegistration<TService, TImplementation>(IServiceCollection services)
        {
            var descriptor = services.Should().ContainSingle(item => item.ServiceType == typeof(TService)).Subject;

            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
            descriptor.ImplementationType.Should().Be(typeof(TImplementation));
        }
    }
}
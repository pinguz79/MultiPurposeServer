using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Moq;

using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Authentication;
using Portfolio.Api.Extensions;
using Portfolio.Api.Filters;
using Portfolio.Api.Infrastructure.Clients;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.DataModel;

namespace Portfolio.Api.Tests.Extensions
{
    public class ExtensionsTests
    {
        #region Authentication

        [Fact]
        public async Task AddPortfolio_WhenCalled_RegistersAuthenticationScheme()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
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
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<PortfolioAuthenticationOptions>>().Value;

            // Assert
            options.HeaderName.Should().Be("X-Custom-Portfolio-Key");
            options.FrontEndKey.Should().Be("front-end-key");
            options.BackEndKey.Should().Be("back-end-key");
        }

        [Fact]
        public async Task AddPortfolio_WhenEnvironmentIsNotDevelopment_ConfiguresFrontEndPolicy()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            services.AddPortfolio(configuration, environment);
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
        public async Task AddPortfolio_WhenEnvironmentIsNotDevelopment_ConfiguresBackEndPolicy()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            services.AddPortfolio(configuration, environment);
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

        [Theory]
        [InlineData(PortfolioPolicies.FrontEnd)]
        [InlineData(PortfolioPolicies.BackEnd)]
        public async Task AddPortfolio_WhenEnvironmentIsDevelopment_ConfiguresPolicyWithoutAuthentication(string policyName)
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment(Environments.Development);

            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var policyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();

            // Act
            var policy = await policyProvider.GetPolicyAsync(policyName);

            // Assert
            policy.Should().NotBeNull();
            policy!.AuthenticationSchemes.Should().BeEmpty();
            policy.Requirements.OfType<DenyAnonymousAuthorizationRequirement>().Should().BeEmpty();
            policy.Requirements.OfType<ClaimsAuthorizationRequirement>().Should().BeEmpty();
            policy.Requirements.OfType<AssertionRequirement>().Should().ContainSingle();
        }

        #endregion

        #region Registrations

        [Fact]
        public void AddPortfolio_WhenCalled_RegistersRepositories()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);

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
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);

            // Assert
            AssertScopedRegistration<IAlbumService, AlbumService>(services);
            AssertScopedRegistration<IFotoService, FotoService>(services);
            AssertScopedRegistration<IMediaService, MediaService>(services);
            AssertScopedRegistration<IImageResizer, ImageMagickResizer>(services);
            AssertScopedRegistration<ICacheService, CacheService>(services);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_RegistersPortfolioWebCacheClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);

            // Assert
            var descriptor = services.Should().ContainSingle(item => item.ServiceType == typeof(IPortfolioWebCacheClient)).Subject;
            descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_RegistersPipelineFilters()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value;

            // Assert
            AssertScopedRegistration<KeyNotFoundExceptionFilter, KeyNotFoundExceptionFilter>(services);
            AssertScopedRegistration<RequestNormalizationValidationFilter, RequestNormalizationValidationFilter>(services);
            AssertScopedRegistration<ValidationExceptionFilter, ValidationExceptionFilter>(services);
            mvcOptions.Filters.OfType<ServiceFilterAttribute>().Should().ContainSingle(filter => filter.ServiceType == typeof(KeyNotFoundExceptionFilter));
            mvcOptions.Filters.OfType<ServiceFilterAttribute>().Should().ContainSingle(filter => filter.ServiceType == typeof(RequestNormalizationValidationFilter));
            mvcOptions.Filters.OfType<ServiceFilterAttribute>().Should().ContainSingle(filter => filter.ServiceType == typeof(ValidationExceptionFilter));
        }

        #endregion

        #region Options

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
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
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
                new($"{PortfolioMediaOptions.SectionName}:CoverWidth", "360"),
                new($"{PortfolioMediaOptions.SectionName}:CoverHeight", "240"),
                new($"{PortfolioMediaOptions.SectionName}:EditorialCoverWidth", "1050"),
                new($"{PortfolioMediaOptions.SectionName}:EditorialCoverHeight", "700"),
                new($"{PortfolioMediaOptions.SectionName}:ThumbnailWidth", "320"),
                new($"{PortfolioMediaOptions.SectionName}:ThumbnailHeight", "200"),
                new($"{PortfolioMediaOptions.SectionName}:ImageWidth", "1600"),
                new($"{PortfolioMediaOptions.SectionName}:ImageHeight", "1200")
            ]);
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<PortfolioMediaOptions>>().Value;

            // Assert
            options.OriginalsRoot.Should().Be("media/originals");
            options.CacheRoot.Should().Be("media/cache");
            options.CoverWidth.Should().Be(360);
            options.CoverHeight.Should().Be(240);
            options.EditorialCoverWidth.Should().Be(1050);
            options.EditorialCoverHeight.Should().Be(700);
            options.ThumbnailWidth.Should().Be(320);
            options.ThumbnailHeight.Should().Be(200);
            options.ImageWidth.Should().Be(1600);
            options.ImageHeight.Should().Be(1200);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_BindsAlbumOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration([new($"{PortfolioAlbumOptions.SectionName}:RootPath", "PortfolioRoot")]);
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<PortfolioAlbumOptions>>().Value;

            // Assert
            options.RootPath.Should().Be("PortfolioRoot");
        }

        [Fact]
        public void AddPortfolio_WhenCalled_ConfiguresMediaRootPathFromEnvironment()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(
            [
                new($"{PortfolioMediaOptions.SectionName}:OriginalsRoot", "media/originals"),
                new($"{PortfolioMediaOptions.SectionName}:CacheRoot", "media/cache")
            ]);
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<PortfolioMediaOptions>>().Value;

            // Assert
            options.RootPath.Should().Be(environment.ContentRootPath);
        }

        #endregion

        #region Database

        [Fact]
        public void AddPortfolio_WhenCalled_ConfiguresPortfolioDatabaseConnection()
        {
            // Arrange
            const string connectionString = "Data Source=portfolio-tests.db";
            var services = new ServiceCollection();
            var configuration = CreateConfiguration([new("ConnectionStrings:PortfolioDatabase", connectionString)]);
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();

            // Assert
            context.Database.GetConnectionString().Should().Be(connectionString);
        }

        [Fact]
        public void AddPortfolio_WhenCalled_ConfiguresLazyLoadingProxies()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();

            // Assert
            context.ChangeTracker.LazyLoadingEnabled.Should().BeTrue();
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
            var firstEnvironment = CreateHostEnvironment();
            var secondEnvironment = CreateHostEnvironment();

            // Act
            firstServices.AddPortfolio(firstConfiguration, firstEnvironment);
            secondServices.AddPortfolio(secondConfiguration, secondEnvironment);
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

        #endregion

        #region HttpClient

        [Fact]
        public void AddPortfolio_WhenCalled_ResolvesPortfolioWebCacheClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<IPortfolioWebCacheClient>();

            // Assert
            client.Should().BeOfType<PortfolioWebCacheHttpClient>();
        }

        [Fact]
        public void AddPortfolio_WhenCacheBaseUrlIsMissing_ThrowsWhenPortfolioWebCacheClientIsResolved()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(
            [
                new($"{PortfolioCacheOptions.SectionName}:BaseUrl", string.Empty),
                new($"{PortfolioCacheOptions.SectionName}:SharedSecret", "shared-secret"),
                new($"{PortfolioCacheOptions.SectionName}:ClearEndpoint", "internal/cache/clear")
            ]);
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var action = () => serviceProvider.GetRequiredService<IPortfolioWebCacheClient>();

            // Assert
            action.Should().Throw<OptionsValidationException>().WithMessage("*PortfolioCache:BaseUrl must be an absolute HTTP or HTTPS URI.*");
        }

        [Fact]
        public void AddPortfolio_WhenCacheBaseUrlIsInvalid_ThrowsWhenPortfolioWebCacheClientIsResolved()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateConfiguration([new($"{PortfolioCacheOptions.SectionName}:BaseUrl", "not a valid uri")]);
            var environment = CreateHostEnvironment();

            // Act
            services.AddPortfolio(configuration, environment);
            using var serviceProvider = services.BuildServiceProvider();
            var action = () => serviceProvider.GetRequiredService<IPortfolioWebCacheClient>();

            // Assert
            action.Should().Throw<OptionsValidationException>().WithMessage("*PortfolioCache:BaseUrl must be an absolute HTTP or HTTPS URI.*");
        }

        #endregion

        #region Helpers

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

        private static IHostEnvironment CreateHostEnvironment(string? environmentName = null)
        {
            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.SetupGet(environment => environment.EnvironmentName).Returns(environmentName ?? Environments.Production);
            environmentMock.SetupGet(environment => environment.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            return environmentMock.Object;
        }

        private static void AssertScopedRegistration<TService, TImplementation>(IServiceCollection services) where TImplementation : TService
        {
            var descriptor = services.Should().ContainSingle(item => item.ServiceType == typeof(TService)).Subject;
            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
            descriptor.ImplementationType.Should().Be(typeof(TImplementation));
        }

        #endregion
    }
}

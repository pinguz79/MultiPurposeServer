using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using Portfolio.Api.Application.Services;
using Portfolio.Api.Authentication;
using Portfolio.Api.Filters;

namespace Portfolio.Api.IntegrationTests.Infrastructure
{
    public sealed class PortfolioApiTestHost : IAsyncDisposable
    {
        private readonly IHost _host;

        public Mock<IAlbumService> AlbumService { get; } = new(MockBehavior.Strict);
        public Mock<ICacheService> CacheService { get; } = new(MockBehavior.Strict);
        public HttpClient Client { get; }
        public Mock<IFotoService> FotoService { get; } = new(MockBehavior.Strict);

        public PortfolioApiTestHost()
        {
            _host = new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddAuthorizationBuilder()
                            .AddPolicy(PortfolioPolicies.BackEnd, policy => policy.RequireAssertion(_ => true));
                        services.AddScoped<KeyNotFoundExceptionFilter>();
                        services.AddScoped<RequestNormalizationValidationFilter>();
                        services.AddScoped<ValidationExceptionFilter>();
                        services.AddSingleton(AlbumService.Object);
                        services.AddSingleton(CacheService.Object);
                        services.AddSingleton(FotoService.Object);
                        services.AddControllers(options =>
                            {
                                options.Filters.AddService<KeyNotFoundExceptionFilter>();
                                options.Filters.AddService<RequestNormalizationValidationFilter>();
                                options.Filters.AddService<ValidationExceptionFilter>();
                            })
                            .AddApplicationPart(typeof(AssemblyReference).Assembly)
                            .AddControllersAsServices();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints => endpoints.MapControllers());
                    }))
                .Start();

            Client = _host.GetTestClient();
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}

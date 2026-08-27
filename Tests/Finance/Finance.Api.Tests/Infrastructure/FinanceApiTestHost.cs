using Finance.Api.Application;
using Finance.Api.Authentication;
using Finance.Api.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

namespace Finance.Api.Tests.Infrastructure
{
    public sealed class FinanceApiTestHost : IAsyncDisposable
    {
        public const string ApiKey = "FinanceTestApiKey";

        private readonly IHost _host;

        public HttpClient Client { get; }
        public Mock<IContoService> ContoService { get; } = new(MockBehavior.Strict);
        public Mock<IMovimentoService> MovimentoService { get; } = new(MockBehavior.Strict);
        public Mock<IContoRepository> ContoRepository { get; } = new(MockBehavior.Strict);

        public FinanceApiTestHost()
        {
            _host = new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddOptions<FinanceAuthenticationOptions>().Configure(options =>
                        {
                            options.HeaderName = FinanceAuthenticationOptions.DefaultHeaderName;
                            options.DesktopKey = ApiKey;
                        });
                        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, FinanceApiKeyAuthenticationHandler>(
                            FinanceApiKeyAuthenticationDefaults.AuthenticationScheme,
                            _ => { });
                        services.AddAuthorizationBuilder().AddPolicy(FinancePolicies.Desktop, policy =>
                        {
                            policy.AddAuthenticationSchemes(FinanceApiKeyAuthenticationDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                        });
                        services.AddSingleton(ContoService.Object);
                        services.AddSingleton(MovimentoService.Object);
                        services.AddSingleton(ContoRepository.Object);
                        services.AddControllers()
                            .AddApplicationPart(typeof(AssemblyReference).Assembly)
                            .AddControllersAsServices();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints => endpoints.MapControllers());
                    }))
                .Start();

            Client = _host.GetTestClient();
        }

        public void Authenticate() => Client.DefaultRequestHeaders.Add(FinanceAuthenticationOptions.DefaultHeaderName, ApiKey);

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}

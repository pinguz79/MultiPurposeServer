using Finance.Api.Application;
using Finance.Api.Authentication;
using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Extensions
{
    public static class FinanceApiExtensions
    {
        public static void AddFinance(this IServiceCollection services, IConfigurationSection configuration, IHostEnvironment environment)
        {
            AddAuthentication(services, configuration, environment);
            services.AddDbContext<FinanceContext>(options => options.UseLazyLoadingProxies().UseSqlite(configuration.GetConnectionString("Database")));
            services.AddScoped<EntityFrameworkPersistenceCoordinator<FinanceContext>>();
            services.AddScoped<FormulaEvaluationCache>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<ICartaASaldoRepository, CartaASaldoRepository>();
            services.AddScoped<IContoRepository, ContoRepository>();
            services.AddScoped<IMovimentoRepository, MovimentoRepository>();
            services.AddScoped<IParametroContoRepository, ParametroContoRepository>();
            services.AddScoped<IVoceRicorrenteRepository, VoceRicorrenteRepository>();
            services.AddScoped<IPianificazioneRepository, PianificazioneRepository>();
            services.AddScoped<IFormulaResolver, FormulaResolver>();
            services.AddScoped<IFormulaEvaluator, FormulaEvaluator>();
            services.AddScoped<ICycleIndicatorsService, CycleIndicatorsService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<ICartaASaldoService, CartaASaldoService>();
            services.AddScoped<IContoService, ContoService>();
            services.AddScoped<IMovimentoService, MovimentoService>();
            services.AddScoped<IParametroContoService, ParametroContoService>();
            services.AddScoped<IVoceRicorrenteService, VoceRicorrenteService>();
            services.AddScoped<IPianificazioneService, PianificazioneService>();
        }

        private static void AddAuthentication(IServiceCollection services, IConfigurationSection configuration, IHostEnvironment environment)
        {
            services.AddOptions<FinanceAuthenticationOptions>()
                .Bind(configuration.GetSection(FinanceAuthenticationOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.HeaderName), "Finance:Authentication:HeaderName is required.")
                .Validate(options => environment.IsDevelopment() || !string.IsNullOrWhiteSpace(options.DesktopKey),
                    "Finance:Authentication:DesktopKey is required outside Development.")
                .ValidateOnStart();

            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, FinanceApiKeyAuthenticationHandler>(FinanceApiKeyAuthenticationDefaults.AuthenticationScheme, _ => { });
            services.AddAuthorizationBuilder().AddPolicy(FinancePolicies.Desktop, policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.RequireAssertion(_ => true);
                    return;
                }

                policy.AddAuthenticationSchemes(FinanceApiKeyAuthenticationDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        }

        public static void UseFinance(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FinanceContext>();

            context.Database.Migrate();
        }
    }
}

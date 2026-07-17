using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Portfolio.Api.Authentication;
using Portfolio.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portfolio.Api.Extensions
{
    public static class PortfolioSwaggerExtensions
    {
        public static void AddPortfolioSecurity(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition(
                PortfolioApiKeyAuthenticationDefaults.FrontEndSwaggerScheme,
                new OpenApiSecurityScheme
                {
                    Name = PortfolioAuthenticationOptions.DefaultHeaderName,
                    Description =
                        "Chiave Portfolio FrontEnd. Consente l'accesso esclusivamente agli endpoint /Portfolio/FrontEnd.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

            options.AddSecurityDefinition(
                PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme,
                new OpenApiSecurityScheme
                {
                    Name = PortfolioAuthenticationOptions.DefaultHeaderName,
                    Description =
                        "Chiave Portfolio BackEnd. Consente l'accesso agli endpoint FrontEnd e BackEnd.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

            options.OperationFilter<PortfolioSecurityOperationFilter>();
        }
    }
}
using Finance.Api.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

using Portfolio.Api.Authentication;

using Scalar.AspNetCore;

using Serilog;

namespace MultiPurposeServer.Extensions
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddMultiPurposeOpenApi(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info.Title = "MPS API";
                    document.Info.Version = "v1";
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes[PortfolioApiKeyAuthenticationDefaults.FrontEndOpenApiScheme] = CreateApiKeyScheme(
                        PortfolioAuthenticationOptions.DefaultHeaderName,
                        "Chiave Portfolio FrontEnd. Consente l'accesso esclusivamente agli endpoint /Portfolio/FrontEnd.");
                    document.Components.SecuritySchemes[PortfolioApiKeyAuthenticationDefaults.BackEndOpenApiScheme] = CreateApiKeyScheme(
                        PortfolioAuthenticationOptions.DefaultHeaderName,
                        "Chiave Portfolio BackEnd. Consente l'accesso agli endpoint FrontEnd e BackEnd.");
                    document.Components.SecuritySchemes[FinanceApiKeyAuthenticationDefaults.OpenApiScheme] = CreateApiKeyScheme(
                        FinanceAuthenticationOptions.DefaultHeaderName,
                        "Chiave Finance Desktop. Consente l'accesso agli endpoint /Finance/FrontEnd e /Finance/BackEnd.");

                    return Task.CompletedTask;
                });

                options.AddOperationTransformer((operation, context, _) =>
                {
                    var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                    if (metadata.OfType<AllowAnonymousAttribute>().Any())
                    {
                        operation.Security = [];
                        return Task.CompletedTask;
                    }

                    var policies = metadata
                        .OfType<AuthorizeAttribute>()
                        .Select(attribute => attribute.Policy)
                        .Where(policy => !string.IsNullOrWhiteSpace(policy))
                        .ToHashSet(StringComparer.Ordinal);

                    if (policies.Count == 0)
                    {
                        return Task.CompletedTask;
                    }

                    operation.Responses ??= [];
                    operation.Responses.TryAdd("401", new OpenApiResponse { Description = "API key missing or invalid." });
                    operation.Responses.TryAdd("403", new OpenApiResponse { Description = "The supplied API key does not grant access to this endpoint." });

                    if (policies.Contains(PortfolioPolicies.BackEnd))
                    {
                        operation.Security = [CreateRequirement(PortfolioApiKeyAuthenticationDefaults.BackEndOpenApiScheme, context.Document!)];
                    }
                    else if (policies.Contains(PortfolioPolicies.FrontEnd))
                    {
                        operation.Security =
                        [
                            CreateRequirement(PortfolioApiKeyAuthenticationDefaults.FrontEndOpenApiScheme, context.Document!),
                            CreateRequirement(PortfolioApiKeyAuthenticationDefaults.BackEndOpenApiScheme, context.Document!)
                        ];
                    }
                    else if (policies.Contains(FinancePolicies.Desktop))
                    {
                        operation.Security = [CreateRequirement(FinanceApiKeyAuthenticationDefaults.OpenApiScheme, context.Document!)];
                    }

                    return Task.CompletedTask;
                });
            });

            return services;
        }

        public static void UseMultiPurposeOpenApi(this WebApplication app, bool enabled, string? pathBase)
        {
            enabled |= app.Environment.IsDevelopment();
            Log.Information("EnableOpenApi={EnableDocumentation}, Environment={Environment}, PathBase={PathBase}", enabled, app.Environment.EnvironmentName, pathBase ?? string.Empty);

            if (!enabled)
            {
                return;
            }

            app.MapOpenApi("/openapi/{documentName}.json");
            app.MapScalarApiReference("/scalar", options => options
                .WithTitle("MPS API V1")
                .WithOpenApiRoutePattern($"{pathBase}/openapi/{{documentName}}.json"));
        }

        private static OpenApiSecurityScheme CreateApiKeyScheme(string headerName, string description) => new()
        {
            Name = headerName,
            Description = description,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        };

        private static OpenApiSecurityRequirement CreateRequirement(string schemeName, OpenApiDocument document) => new()
        {
            [new OpenApiSecuritySchemeReference(schemeName, document)] = []
        };
    }
}

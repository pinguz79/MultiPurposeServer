using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Portfolio.Api.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portfolio.Api.Swagger
{
    public sealed class PortfolioSecurityOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>() ?? [])
                .ToList();

            if (authorizeAttributes.Count == 0)
            {
                return;
            }

            var policies = authorizeAttributes
                .Select(attribute => attribute.Policy)
                .Where(policy => !string.IsNullOrWhiteSpace(policy))
                .ToHashSet(StringComparer.Ordinal);

            operation.Responses ??= [];

            operation.Responses.TryAdd("401", new OpenApiResponse
                {
                    Description = "API key missing or invalid."
                });

            operation.Responses.TryAdd("403", new OpenApiResponse
                {
                    Description = "The supplied API key does not grant access to this endpoint."
                });

            if (policies.Contains(PortfolioPolicies.BackEnd))
            {
                operation.Security =
                [
                    CreateRequirement(PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme, context.Document)
                ];

                return;
            }

            if (policies.Contains(PortfolioPolicies.FrontEnd))
            {
                // BackEnd users are allowed to access FrontEnd endpoints as well.
                operation.Security =
                [
                    CreateRequirement(PortfolioApiKeyAuthenticationDefaults.FrontEndSwaggerScheme, context.Document),
                    CreateRequirement(PortfolioApiKeyAuthenticationDefaults.BackEndSwaggerScheme, context.Document)
                ];
            }
        }

        private static OpenApiSecurityRequirement CreateRequirement(string schemeName, OpenApiDocument document) => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemeName, document)] = []
        };
    }
}
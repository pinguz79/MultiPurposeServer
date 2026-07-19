using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Extensions;
using Serilog;
using System.Reflection;

namespace MultiPurposeServer.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddMultiPurposeSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                IncludeXmlComments(options, Assembly.GetExecutingAssembly());
                IncludeXmlComments(options, typeof(Portfolio.Api.AssemblyReference).Assembly);

                options.AddPortfolioSecurity();
            });

            return services;
        }

        private static void IncludeXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options, Assembly assembly)
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        }

        public static void UseMultiPurposeSwagger(this WebApplication app, bool enableSwagger, string? pathBase)
        {
            enableSwagger |= app.Environment.IsDevelopment();
            Log.Information($"EnableSwagger={enableSwagger}, Environment={app.Environment.EnvironmentName}, PathBase={pathBase ?? ""}");

            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    // Serve the UI at /swagger. If app is hosted under a PathBase, the endpoint will include it.
                    var endpoint = string.IsNullOrEmpty(pathBase) ? "/swagger/v1/swagger.json" : $"{pathBase}/swagger/v1/swagger.json";
                    options.SwaggerEndpoint(endpoint, "MPS API V1");
                    options.RoutePrefix = "swagger"; // serve at /swagger
                });
            }
        }
    }
}
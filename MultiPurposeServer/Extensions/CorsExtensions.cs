namespace MultiPurposeServer.Extensions
{
    public static class CorsExtensions
    {
        private const string PolicyName = "DevelopmentAllowAll";

        public static IServiceCollection AddMultiPurposeCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            return services;
        }

        public static IApplicationBuilder UseMultiPurposeCors(this IApplicationBuilder app)
        {
            return app.UseCors(PolicyName);
        }
    }
}

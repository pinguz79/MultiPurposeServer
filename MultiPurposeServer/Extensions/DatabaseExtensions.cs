using MultiPurposeServer.Microservices.Database;

namespace MultiPurposeServer.Extensions
{
    public static class DatabaseExtensions
    {
        public static void AddDatabase(this IServiceCollection services)
        {
            services.AddTransient<IDatabaseService, DatabaseService>();
        }
    }
}

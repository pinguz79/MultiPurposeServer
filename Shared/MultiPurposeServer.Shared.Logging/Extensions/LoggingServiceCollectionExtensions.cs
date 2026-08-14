using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Models;
using MultiPurposeServer.Shared.Logging.Services;

namespace MultiPurposeServer.Shared.Logging.Extensions
{
    public static class LoggingServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedLogging(this IServiceCollection services, DiagnosticOptions? options = null)
        {
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton(options ?? new DiagnosticOptions());
            services.TryAddSingleton<ILoggingContextAccessor, LoggingContextAccessor>();
            services.TryAddSingleton<IDiagnosticStateRegistry, DiagnosticStateRegistry>();
            services.TryAddTransient(typeof(ILoggerService<>), typeof(LoggerService<>));

            return services;
        }
    }
}

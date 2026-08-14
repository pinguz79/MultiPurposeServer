using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace MultiPurposeServer.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddMultiPurposeLogging(this WebApplicationBuilder builder)
        {
            var logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs");
            var retainedFileCountLimit = builder.Configuration.GetValue<int?>("Logging:RetainedFileCountLimit") ?? 14;
            var fileSizeLimitBytes = builder.Configuration.GetValue<long?>("Logging:FileSizeLimitBytes") ?? 20 * 1024 * 1024;

            if (retainedFileCountLimit < 1)
            {
                throw new InvalidOperationException("Logging:RetainedFileCountLimit must be greater than zero.");
            }

            if (fileSizeLimitBytes < 1)
            {
                throw new InvalidOperationException("Logging:FileSizeLimitBytes must be greater than zero.");
            }

            var formatter = new JsonFormatter(renderMessage: true);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}/{OriginalLevel}] [{Domain}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.Logger(configuration => configuration
                    .Filter.ByIncludingOnly(logEvent => !HasDomain(logEvent, "Portfolio") && !HasDomain(logEvent, "SampleApp"))
                    .WriteTo.File(
                        formatter,
                        Path.Combine(logsPath, "host", "mps-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: retainedFileCountLimit,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        rollOnFileSizeLimit: true))
                .WriteTo.Logger(configuration => configuration
                    .Filter.ByIncludingOnly(logEvent => HasDomain(logEvent, "Portfolio"))
                    .WriteTo.File(
                        formatter,
                        Path.Combine(logsPath, "portfolio", "portfolio-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: retainedFileCountLimit,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        rollOnFileSizeLimit: true))
                .WriteTo.Logger(configuration => configuration
                    .Filter.ByIncludingOnly(logEvent => HasDomain(logEvent, "SampleApp"))
                    .WriteTo.File(
                        formatter,
                        Path.Combine(logsPath, "sample-app", "sample-app-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: retainedFileCountLimit,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        rollOnFileSizeLimit: true))
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }

        private static bool HasDomain(LogEvent logEvent, string domain) =>
            logEvent.Properties.TryGetValue("Domain", out var value)
            && value is ScalarValue { Value: string currentDomain }
            && currentDomain.Equals(domain, StringComparison.OrdinalIgnoreCase);
    }
}

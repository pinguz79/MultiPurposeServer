using Serilog;

namespace MultiPurposeServer.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddMultiPurposeLogging(this WebApplicationBuilder builder)
        {
            var logsPath = Path.Combine(builder.Environment.ContentRootPath, "Logs", "mps-.log");
            var retainedFileCountLimit = builder.Configuration.GetValue<int?>("Logging:RetainedFileCountLimit") ?? 14;

            if (retainedFileCountLimit < 1)
            {
                throw new InvalidOperationException("Logging:RetainedFileCountLimit must be greater than zero.");
            }

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: retainedFileCountLimit)
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}

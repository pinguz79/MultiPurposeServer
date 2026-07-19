using Serilog;

namespace MultiPurposeServer.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddMultiPurposeLogging(this WebApplicationBuilder builder)
        {
            var logsPath = Path.Combine(builder.Environment.ContentRootPath, "Logs", "mps-.log");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}
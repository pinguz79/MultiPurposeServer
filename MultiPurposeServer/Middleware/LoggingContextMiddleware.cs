using System.Text.RegularExpressions;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Middleware
{
    public sealed partial class LoggingContextMiddleware(RequestDelegate next)
    {
        public const string CorrelationHeaderName = "X-Correlation-ID";

        public async Task InvokeAsync(
            HttpContext httpContext,
            ILoggingContextAccessor contextAccessor,
            ILogger<LoggingContextMiddleware> logger)
        {
            var correlationId = ResolveCorrelationId(httpContext.Request.Headers[CorrelationHeaderName]);
            var domain = ResolveDomain(httpContext.Request.Path);
            var context = new LoggingContext(domain, correlationId, httpContext.TraceIdentifier);

            httpContext.Response.Headers[CorrelationHeaderName] = correlationId;

            var scope = new Dictionary<string, object?>
            {
                ["Domain"] = domain,
                ["CorrelationId"] = correlationId,
                ["RequestId"] = httpContext.TraceIdentifier,
                ["Origin"] = "Server",
            };

            using (contextAccessor.Push(context))
            using (logger.BeginScope(scope))
            {
                await next(httpContext);
            }
        }

        private static string ResolveCorrelationId(string? value) =>
            !string.IsNullOrWhiteSpace(value) && CorrelationIdPattern().IsMatch(value) ? value : Guid.NewGuid().ToString("N");

        private static string ResolveDomain(PathString path)
        {
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
            var domainIndex = segments.FirstOrDefault()?.Equals("api", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0;

            return segments.Length > domainIndex && segments[domainIndex].Equals("Portfolio", StringComparison.OrdinalIgnoreCase)
                ? "Portfolio"
                : "Host";
        }

        [GeneratedRegex("^[A-Za-z0-9._:-]{1,128}$", RegexOptions.CultureInvariant)]
        private static partial Regex CorrelationIdPattern();
    }
}

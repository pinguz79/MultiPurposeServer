using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Logging.Abstractions;

namespace MultiPurposeServer.Diagnostics
{
    public sealed class GlobalExceptionHandler(
        ILoggerService<GlobalExceptionHandler> logger,
        ILoggingContextAccessor contextAccessor) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var context = contextAccessor.Current;

            logger.Error(
                HostLogEvents.UnhandledHttpException,
                exception,
                "Eccezione non gestita durante {RequestMethod} {RequestPath}.",
                httpContext.Request.Method,
                httpContext.Request.Path.Value);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Errore interno del server",
                    Extensions = { ["correlationId"] = context.CorrelationId },
                },
                cancellationToken);

            return true;
        }
    }
}

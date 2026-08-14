using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Abstractions
{
    public interface ILoggingContextAccessor
    {
        LoggingContext Current { get; }

        IDisposable Push(LoggingContext context);
    }
}

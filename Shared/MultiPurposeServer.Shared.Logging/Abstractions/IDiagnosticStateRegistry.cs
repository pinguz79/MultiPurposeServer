using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Abstractions
{
    public interface IDiagnosticStateRegistry
    {
        DiagnosticState Get(string domain);

        DiagnosticState Enable(string domain, DiagnosticMode mode, TimeSpan duration);

        DiagnosticState Disable(string domain);
    }
}

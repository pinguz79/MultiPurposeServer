using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Diagnostics;
using MultiPurposeServer.Shared.Logging.Models;

namespace MultiPurposeServer.Shared.Logging.Services
{
    public sealed class DiagnosticStateRegistry(DiagnosticOptions options, TimeProvider timeProvider, ILogger<DiagnosticStateRegistry> logger) : IDiagnosticStateRegistry
    {
        private readonly ConcurrentDictionary<string, DiagnosticState> _states = new(StringComparer.OrdinalIgnoreCase);

        public DiagnosticState Get(string domain)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(domain);

            if (!_states.TryGetValue(domain, out var state))
            {
                return DiagnosticState.Off(domain);
            }

            if (state.ExpiresAt > timeProvider.GetUtcNow())
            {
                return state;
            }

            _states.TryRemove(domain, out _);
            logger.LogInformation(
                new EventId(0, SharedLoggingLogEvents.DiagnosticsExpired.Value),
                "Diagnostica scaduta per il dominio {Domain}.",
                domain);

            return DiagnosticState.Off(domain);
        }

        public DiagnosticState Enable(string domain, DiagnosticMode mode, TimeSpan duration)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(domain);
            if (mode == DiagnosticMode.Off)
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "La modalità Off deve essere impostata tramite Disable.");
            }

            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "La durata deve essere maggiore di zero.");
            }

            var maximumDuration = mode == DiagnosticMode.Verbose ? options.MaximumVerboseDuration : options.MaximumDiagnosticDuration;
            if (duration > maximumDuration)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "La durata supera il limite configurato per la modalità.");
            }

            var state = new DiagnosticState(domain, mode, timeProvider.GetUtcNow().Add(duration));
            _states[domain] = state;
            return state;
        }

        public DiagnosticState Disable(string domain)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(domain);
            _states.TryRemove(domain, out _);
            return DiagnosticState.Off(domain);
        }
    }
}

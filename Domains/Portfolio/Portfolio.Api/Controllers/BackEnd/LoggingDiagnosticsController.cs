using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Logging.Abstractions;
using MultiPurposeServer.Shared.Logging.Models;

using Portfolio.Api.Application.Diagnostics;
using Portfolio.Contracts.Enums;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Route("Portfolio/BackEnd/Diagnostics/Logging")]
    [ApiController]
    public sealed class LoggingDiagnosticsController(
        IDiagnosticStateRegistry registry,
        DiagnosticOptions options,
        ILoggerService<LoggingDiagnosticsController> logger) : PortfolioBackEndControllerBase
    {
        private const string Domain = "Portfolio";

        [HttpGet]
        public ActionResult<LoggingDiagnosticStateDto> Get() => Ok(ToDto(registry.Get(Domain)));

        [HttpPut]
        public ActionResult<LoggingDiagnosticStateDto> Enable(EnableLoggingDiagnosticRequest request)
        {
            var mode = ToSharedMode(request.Mode);
            var duration = TimeSpan.FromMinutes(request.DurationMinutes);
            var maximumDuration = mode == DiagnosticMode.Verbose ? options.MaximumVerboseDuration : options.MaximumDiagnosticDuration;

            if (duration <= TimeSpan.Zero || duration > maximumDuration)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Durata diagnostica non valida.",
                    Detail = $"La durata deve essere compresa tra 1 e {maximumDuration.TotalMinutes:0} minuti.",
                    Status = StatusCodes.Status400BadRequest,
                });
            }

            var state = registry.Enable(Domain, mode, duration);
            logger.Information(
                PortfolioLogEvents.LoggingDiagnosticsEnabled,
                "Diagnostica {DiagnosticMode} abilitata per il dominio {Domain} fino a {ExpiresAt}.",
                state.Mode,
                state.Domain,
                state.ExpiresAt);

            return Ok(ToDto(state));
        }

        [HttpDelete]
        public ActionResult<LoggingDiagnosticStateDto> Disable()
        {
            var state = registry.Disable(Domain);
            logger.Information(
                PortfolioLogEvents.LoggingDiagnosticsDisabled,
                "Diagnostica disabilitata per il dominio {Domain}.",
                state.Domain);

            return Ok(ToDto(state));
        }

        private static DiagnosticMode ToSharedMode(LoggingDiagnosticMode mode) => mode switch
        {
            LoggingDiagnosticMode.Diagnostic => DiagnosticMode.Diagnostic,
            LoggingDiagnosticMode.Verbose => DiagnosticMode.Verbose,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };

        private static LoggingDiagnosticStateDto ToDto(DiagnosticState state) => new(state.Domain, state.Mode.ToString(), state.ExpiresAt);
    }
}

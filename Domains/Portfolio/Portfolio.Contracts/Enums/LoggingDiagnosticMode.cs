using System.Text.Json.Serialization;

namespace Portfolio.Contracts.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<LoggingDiagnosticMode>))]
    public enum LoggingDiagnosticMode
    {
        Diagnostic,
        Verbose,
    }
}

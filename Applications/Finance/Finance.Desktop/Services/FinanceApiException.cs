using System.Net;

namespace Finance.Desktop.Services
{
    public sealed class FinanceApiException(HttpStatusCode statusCode, string message, string? field = null) : Exception(message)
    {
        public HttpStatusCode StatusCode { get; } = statusCode;
        public string? Field { get; } = field;
    }
}

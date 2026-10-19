using System.Net;

namespace MultiPurposeServer.Client
{
    /// <summary>Errore HTTP o risposta non interpretabile ricevuta dal server.</summary>
    public sealed class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
    {
        /// <summary>Stato della risposta originale, anche in caso di payload non valido.</summary>
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}

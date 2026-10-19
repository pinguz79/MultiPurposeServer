namespace Finance.Client.Tests
{
    internal sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? ApiKey { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues("X-Finance-Api-Key", out var values) ? values.Single() : null;
            return send(request, cancellationToken);
        }
    }
}

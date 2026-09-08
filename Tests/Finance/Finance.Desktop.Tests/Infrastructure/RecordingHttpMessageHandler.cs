using System.Net;

namespace Finance.Desktop.Tests.Infrastructure
{
    public sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public List<HttpRequestMessage> Requests { get; } = [];
        public Queue<(HttpStatusCode Status, string Content)> Responses { get; } = [];
        public string? RequestContent { get; private set; }
        public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.Created;
        public string ResponseContent { get; set; } = """
            {
              "id": "fd54970c-41b9-4060-88c3-37c0133ecbbe",
              "name": "HelloBank",
              "displayName": "Hello Bank",
              "initialBalance": 3081.69,
              "balance": 3081.69
            }
            """;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Requests.Add(request);
            RequestContent = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            (HttpStatusCode status, string content) = Responses.Count > 0 ? Responses.Dequeue() : (ResponseStatusCode, ResponseContent);

            return new HttpResponseMessage(status)
            {
                Content = new StringContent(content),
            };
        }
    }
}

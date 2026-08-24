using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;

namespace Finance.Desktop.Services
{
    public sealed class FinanceApiClient
    {
        private readonly HttpClient _client;

        public FinanceApiClient(ApiConfiguration configuration)
        {
            _client = new HttpClient { BaseAddress = new Uri(configuration.BaseUrl) };
            _client.DefaultRequestHeaders.Add(configuration.HeaderName, configuration.ApiKey);
        }

        public async Task<IReadOnlyList<Conto>> GetConti()
        {
            return await _client.GetFromJsonAsync<List<Conto>>("Finance/FrontEnd/Conto/Conti") ?? [];
        }

        public async Task<Conto> CreateConto(CreateConto request)
        {
            using var response = await _client.PostAsJsonAsync("Finance/BackEnd/Conto/Create", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Conto>() ?? throw new InvalidOperationException("The server returned an empty response.");
            }

            throw await CreateException(response);
        }

        private static async Task<FinanceApiException> CreateException(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var message = response.ReasonPhrase ?? "Finance request failed.";
            string? field = null;

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    using var problem = JsonDocument.Parse(content);
                    var root = problem.RootElement;
                    message = root.TryGetProperty("detail", out var detail) ? detail.GetString() ?? message : message;
                    field = root.TryGetProperty("field", out var fieldElement) ? fieldElement.GetString() : null;
                }
                catch (JsonException)
                {
                    message = content;
                }
            }

            return new FinanceApiException(response.StatusCode, message, response.StatusCode == HttpStatusCode.Conflict ? field : null);
        }
    }
}

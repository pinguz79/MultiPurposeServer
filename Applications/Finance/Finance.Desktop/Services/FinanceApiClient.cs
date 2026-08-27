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

        public FinanceApiClient(ApiConfiguration configuration) : this(new HttpClient(), configuration)
        {
        }

        public FinanceApiClient(HttpClient client, ApiConfiguration configuration)
        {
            _client = client;
            _client.BaseAddress ??= new Uri(configuration.BaseUrl);
            _client.DefaultRequestHeaders.Add(configuration.HeaderName, configuration.ApiKey);
        }

        public async Task<IReadOnlyList<Conto>> GetConti()
        {
            return await _client.GetFromJsonAsync<List<Conto>>("Finance/FrontEnd/Conto/List") ?? [];
        }

        public async Task<ContoMovimenti> GetMovimenti(string contoName, int month, int year)
        {
            string route = $"Finance/FrontEnd/Conto/{Uri.EscapeDataString(contoName)}/Movimento/List?month={month}&year={year}";

            return await _client.GetFromJsonAsync<ContoMovimenti>(route) ?? throw new InvalidOperationException("The server returned an empty response.");
        }

        public async Task<Conto> CreateConto(CreateConto request)
        {
            using var response = await _client.PostAsJsonAsync("Finance/BackEnd/Conto", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Conto>() ?? throw new InvalidOperationException("The server returned an empty response.");
            }

            throw await CreateException(response);
        }

        public async Task<IReadOnlyList<VoceRicorrente>> GetVociRicorrenti()
            => await _client.GetFromJsonAsync<List<VoceRicorrente>>("Finance/BackEnd/VoceRicorrente/List") ?? [];

        public async Task<VoceRicorrente> CreateVoceRicorrente(SaveVoceRicorrente request)
        {
            using HttpResponseMessage response = await _client.PostAsJsonAsync("Finance/BackEnd/VoceRicorrente", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<VoceRicorrente>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<VoceRicorrente> UpdateVoceRicorrente(string currentName, SaveVoceRicorrente request)
        {
            using HttpResponseMessage response = await _client.PatchAsJsonAsync(
                $"Finance/BackEnd/VoceRicorrente/{Uri.EscapeDataString(currentName)}",
                request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<VoceRicorrente>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task DeleteVoceRicorrente(string name)
        {
            using HttpResponseMessage response = await _client.DeleteAsync($"Finance/BackEnd/VoceRicorrente/{Uri.EscapeDataString(name)}");

            if (!response.IsSuccessStatusCode)
            {
                throw await CreateException(response);
            }
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

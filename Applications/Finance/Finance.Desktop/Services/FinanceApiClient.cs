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

        public async Task<IReadOnlyList<Conto>> Initialize()
        {
            using HttpResponseMessage response = await _client.PostAsync("Finance/BackEnd/Movimento/Consolida", null);
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateException(response);
            }

            return await GetConti();
        }

        public async Task<ContoMovimenti> GetMovimenti(string contoName, int month, int year)
        {
            string route = $"Finance/FrontEnd/Conto/{Uri.EscapeDataString(contoName)}/Movimento/List?month={month}&year={year}";

            return await _client.GetFromJsonAsync<ContoMovimenti>(route) ?? throw new InvalidOperationException("The server returned an empty response.");
        }

        public async Task<ContoCicli> GetCicli(string contoName, int month, int year)
        {
            string route = $"Finance/FrontEnd/Conto/{Uri.EscapeDataString(contoName)}/Ciclo/List?month={month}&year={year}";

            return await _client.GetFromJsonAsync<ContoCicli>(route) ?? throw new InvalidOperationException("The server returned an empty response.");
        }

        public async Task<Conto> CreateConto(CreateConto request)
        {
            using var response = await _client.PostAsJsonAsync("Finance/BackEnd/Conto", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Conto>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<IReadOnlyList<Categoria>> GetCategorie()
            => await _client.GetFromJsonAsync<List<Categoria>>("Finance/BackEnd/Categoria/List") ?? [];

        public async Task<IReadOnlyList<ParametroConto>> GetParametriConto(string contoName)
            => await _client.GetFromJsonAsync<List<ParametroConto>>(
                $"Finance/BackEnd/Conto/{Uri.EscapeDataString(contoName)}/Parametro/List") ?? [];

        public async Task<ParametroConto> CreateParametroConto(string contoName, CreateParametroConto request)
        {
            using HttpResponseMessage response = await _client.PostAsJsonAsync(
                $"Finance/BackEnd/Conto/{Uri.EscapeDataString(contoName)}/Parametro",
                request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ParametroConto>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<ParametroConto> UpdateParametroConto(string contoName, string name, UpdateParametroConto request)
        {
            using HttpResponseMessage response = await _client.PatchAsJsonAsync(
                $"Finance/BackEnd/Conto/{Uri.EscapeDataString(contoName)}/Parametro/{Uri.EscapeDataString(name)}",
                request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<ParametroConto>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task DeleteParametroConto(string contoName, string name)
        {
            using HttpResponseMessage response = await _client.DeleteAsync(
                $"Finance/BackEnd/Conto/{Uri.EscapeDataString(contoName)}/Parametro/{Uri.EscapeDataString(name)}");

            if (!response.IsSuccessStatusCode)
            {
                throw await CreateException(response);
            }
        }

        public async Task<CartaASaldo> ConfigureCartaASaldo(string contoName, ConfigureCartaASaldo request)
        {
            using HttpResponseMessage response = await _client.PostAsJsonAsync(
                $"Finance/BackEnd/Conto/{Uri.EscapeDataString(contoName)}/Configurazione/CartaASaldo",
                request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CartaASaldo>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<Categoria> CreateCategoria(SaveCategoria request)
        {
            using HttpResponseMessage response = await _client.PostAsJsonAsync("Finance/BackEnd/Categoria", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Categoria>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<Categoria> UpdateCategoria(string name, UpdateCategoria request)
        {
            using HttpResponseMessage response = await _client.PatchAsJsonAsync($"Finance/BackEnd/Categoria/{Uri.EscapeDataString(name)}", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Categoria>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<CategoriaUsage?> DeleteCategoria(string name, bool confirmReferences)
        {
            string route = $"Finance/BackEnd/Categoria/{Uri.EscapeDataString(name)}?confirmReferences={confirmReferences.ToString().ToLowerInvariant()}";
            using HttpResponseMessage response = await _client.DeleteAsync(route);

            return response.IsSuccessStatusCode
                ? null
                : response.StatusCode == HttpStatusCode.Conflict
                ? await response.Content.ReadFromJsonAsync<CategoriaUsage>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
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

        public async Task<PianificazionePreview> PreviewPianificazione(CreatePianificazione request)
        {
            using HttpResponseMessage response = await _client.PostAsJsonAsync("Finance/FrontEnd/Pianificazione/Preview", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PianificazionePreview>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
        }

        public async Task<Pianificazione> CreatePianificazione(CreatePianificazione request)
        {
            using HttpResponseMessage response = await _client.PostAsJsonAsync("Finance/FrontEnd/Pianificazione", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Pianificazione>() ?? throw new InvalidOperationException("The server returned an empty response.")
                : throw await CreateException(response);
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
                    if (root.TryGetProperty("errorCode", out var errorCode) && errorCode.GetString() == "FormulaEvaluationFailed")
                    {
                        string description = root.GetProperty("description").GetString() ?? string.Empty;
                        DateOnly date = root.GetProperty("date").Deserialize<DateOnly>();
                        string movementId = root.GetProperty("movimentoId").GetString() ?? string.Empty;
                        string formula = root.GetProperty("formula").GetString() ?? string.Empty;
                        string error = root.GetProperty("errorMessage").GetString() ?? message;
                        message = $"Movimento {description} del {date:dd/MM/yyyy} (ID {movementId}), formula {formula}: {error}";
                    }
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

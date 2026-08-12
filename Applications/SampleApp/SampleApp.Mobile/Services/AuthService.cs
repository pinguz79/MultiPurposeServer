using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SampleApp.Mobile.Services
{
    public class AuthService
    {
        private readonly string _clientId;
        private readonly string _redirectUri;
        private readonly string _apiBase;

        public AuthService(string clientId, string redirectUri, string apiBase)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;
            _apiBase = apiBase?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(apiBase));
        }

        private static string GenerateCodeVerifier()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string ComputeCodeChallenge(string codeVerifier)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(bytes);
        }

        public async Task<string?> SignInWithGoogleAsync()
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = ComputeCodeChallenge(codeVerifier);

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(_clientId)}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&response_type=code&scope={Uri.EscapeDataString("openid email profile")}&code_challenge={codeChallenge}&code_challenge_method=S256&access_type=offline";

            var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(authUrl), new Uri(_redirectUri));
            if (result?.Properties == null || !result.Properties.TryGetValue("code", out var code))
            {
                return null;
            }

            using var http = new HttpClient();
            var payload = new { code, redirectUri = _redirectUri, codeVerifier };
            var resp = await http.PostAsync($"{_apiBase}/Auth/SampleApp/External/Google/Code", new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("token", out var tokenEl))
            {
                return null;
            }

            var token = tokenEl.GetString();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            await SecureStorage.Default.SetAsync("mps_token", token);
            return token;
        }
    }
}

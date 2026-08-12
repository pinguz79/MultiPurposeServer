using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SampleApp.Mobile.Models;


namespace SampleApp.Mobile.Pages
{
    public partial class LoginPage : ContentPage
    {
        private const string ApiBase = "https://www.modelbook.cloud";
        private static string ApiUrl => $"{ApiBase}/Auth/SampleApp/login";

        public LoginPage()
        {
            InitializeComponent();
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

        private async void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            // La stessa URI deve essere registrata anche nella Google Console.
            const string redirectUri = "com.mps.sampleappmobile:/oauth2redirect";

            // Il client ID viene distribuito con l'applicazione di esempio come MauiAsset.
            string clientId = "YOUR_GOOGLE_CLIENT_ID";
            var assetName = "Secrets/client_secret_SampleApp.Mobile.json";
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(assetName);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                JsonElement cfg = default;
                if (root.TryGetProperty("installed", out var installed))
                {
                    cfg = installed;
                }
                else if (root.TryGetProperty("web", out var web))
                {
                    cfg = web;
                }

                if (cfg.ValueKind != JsonValueKind.Undefined && cfg.TryGetProperty("client_id", out var cid))
                {
                    clientId = cid.GetString() ?? clientId;
                }
            }
            catch
            {
                ShowMessage("Configurazione Google mancante: client_secret_SampleApp.Mobile.json non trovato nel pacchetto.");
                return;
            }

            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = ComputeCodeChallenge(codeVerifier);

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString("openid email profile")}&code_challenge={codeChallenge}&code_challenge_method=S256&access_type=offline";

            try
            {
                var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(authUrl), new Uri(redirectUri));
                if (result?.Properties == null || !result.Properties.TryGetValue("code", out var code))
                {
                    ShowMessage("Authentication cancelled or no code returned.");
                    return;
                }

                using var http = new HttpClient();
                var payload = new { code, redirectUri, codeVerifier };
                var extResp = await http.PostAsync($"{ApiBase}/Auth/SampleApp/External/Google/Code", new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
                var extJson = await extResp.Content.ReadAsStringAsync();
                if (!extResp.IsSuccessStatusCode)
                {
                    ShowMessage($"External login failed: {extResp.StatusCode} {extJson}");
                    return;
                }

                using var extDoc = JsonDocument.Parse(extJson);
                if (!extDoc.RootElement.TryGetProperty("token", out var localTokenEl))
                {
                    ShowMessage("Server did not return local token.");
                    return;
                }

                var localToken = localTokenEl.GetString();
                if (string.IsNullOrEmpty(localToken))
                {
                    ShowMessage("Empty local token returned.");
                    return;
                }

                await SecureStorage.Default.SetAsync("mps_token", localToken);

                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                ShowMessage($"Google auth error: {ex.Message}");
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            MessageLabel.IsVisible = false;
            BusyIndicator.IsVisible = true;
            BusyIndicator.IsRunning = true;
            LoginButton.IsEnabled = false;

            try
            {
                var request = new LoginRequest
                {
                    Username = UsernameEntry.Text?.Trim(),
                    Password = PasswordEntry.Text
                };

                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    ShowMessage("Please enter username and password.");
                    return;
                }

                // Il redirect automatico nasconderebbe la risposta originale necessaria alla diagnostica.
                var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var http = new HttpClient(handler);
                var json = JsonSerializer.Serialize(request);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(ApiUrl, content);

                var respText = await resp.Content.ReadAsStringAsync();
                // Alcuni proxy trasformano in GET una POST rediretta, provocando una risposta 405.
                if ((int)resp.StatusCode is 301 or 302 or 303)
                {
                    var loc = resp.Headers.Location?.ToString() ?? "(none)";
                    ShowMessage($"Server redirected: {(int)resp.StatusCode} -> {loc}. Redirect may change POST to GET and cause 405.");
                    return;
                }
                if (resp.IsSuccessStatusCode)
                {
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    var body = string.Empty;
                    try { body = respText; } catch { }
                    ShowMessage($"Login failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}");
            }
            finally
            {
                BusyIndicator.IsRunning = false;
                BusyIndicator.IsVisible = false;
                LoginButton.IsEnabled = true;
            }
        }

        void ShowMessage(string msg)
        {
            MessageLabel.Text = msg;
            MessageLabel.IsVisible = true;
        }
    }
}

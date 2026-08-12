using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Google.Apis.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using MultiPurposeServer.Models.Auth;

using Serilog;

using SysFile = System.IO.File;

namespace MultiPurposeServer.Controllers.Auth.SampleApp
{
    [ApiController]
    [Route("Auth/SampleApp")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(ILogger<AuthController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Login endpoint for SampleApp clients.
        /// </summary>
        /// <param name="request">Username/password payload.</param>
        [HttpPost("login")]
        [HttpPost("~/SampleApp/Auth")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger?.LogInformation($"Login attempt for user {request?.Username}. Path={Request.Path} Method={Request.Method}");

            if (request is null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "username and password are required" });
            }

            // TODO: sostituire questo controllo dimostrativo con uno user store e password opportunamente cifrate.
            if (request.Username == "sample" && request.Password == "password")
            {
                var response = new LoginResponse
                {
                    Token = Guid.NewGuid().ToString(),
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                return Ok(response);
            }

            return Unauthorized(new { error = "invalid credentials" });
        }

        /// <summary>
        /// Accepts a Google id_token, validates it and returns a local JWT for the client to use with the API.
        /// POST /Auth/SampleApp/External/Google
        /// Body: { "idToken": "..." }
        /// </summary>
        [HttpPost("External/Google")]
        public async Task<IActionResult> ExternalGoogle([FromBody] ExternalLoginRequest request)
        {
            if (request is null || string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest(new { error = "idToken is required" });
            }

            var googleClientId = _config["Authentication:Google:ClientId"];
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                // TODO: applicare al payload validato le regole di autorizzazione specifiche dell'applicazione.
                var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
                var issuer = _config["Jwt:Issuer"] ?? "MPS";
                var audience = _config["Jwt:Audience"] ?? "MPSClients";
                var expireHours = int.TryParse(_config["Jwt:ExpireHours"], out var h) ? h : 1;

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, payload.Subject),
                    new Claim(JwtRegisteredClaimNames.Email, payload.Email ?? string.Empty),
                    new Claim("name", payload.Name ?? string.Empty),
                    new Claim("provider", "google"),
                    new Claim("app", "SampleApp")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwt = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, expires: DateTime.UtcNow.AddHours(expireHours), signingCredentials: creds);
                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Ok(new { token, expires = jwt.ValidTo });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Invalid Google id_token");
                return Unauthorized(new { error = "invalid_external_token" });
            }
        }

        /// <summary>
        /// Accepts an authorization code from Google (PKCE flow), exchanges it server-side for tokens,
        /// validates the returned id_token and returns a local JWT for the client to use with the API.
        /// POST /Auth/SampleApp/External/Google/Code
        /// Body: { "code": "...", "redirectUri": "...", "codeVerifier": "..." }
        /// </summary>
        [HttpPost("External/Google/Code")]
        public async Task<IActionResult> ExternalGoogleCode([FromBody] ExternalGoogleCodeRequest request)
        {
            if (request is null || string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.RedirectUri) || string.IsNullOrEmpty(request.CodeVerifier))
            {
                return BadRequest(new { error = "code, redirectUri and codeVerifier are required" });
            }

            // Ogni applicazione usa credenziali Google autonome.
            var clientId = _config["Authentication:Google:SampleApp.Mobile:ClientId"];
            var clientSecret = _config["Authentication:Google:SampleApp.Mobile:ClientSecret"];
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Log.Warning("Google client configuration missing for SampleApp.Mobile. Expected keys: Authentication:Google:SampleApp.Mobile:ClientId and :ClientSecret");
                return StatusCode(500, new { error = "google client configuration missing on server" });
            }

            try
            {
                using var http = new HttpClient();
                var form = new Dictionary<string, string>
                {
                    ["code"] = request.Code,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["redirect_uri"] = request.RedirectUri,
                    ["grant_type"] = "authorization_code",
                    ["code_verifier"] = request.CodeVerifier
                };

                var resp = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(form));
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    Log.Warning($"Google token exchange failed with status {resp.StatusCode}");

                    return StatusCode(502, new { error = "invalid_external_token" });
                }

                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("id_token", out var idTokenEl))
                {
                    Log.Warning("Google token response did not contain id_token");
                    return Unauthorized(new { error = "invalid_external_token" });
                }

                var idToken = idTokenEl.GetString();

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
                var issuer = _config["Jwt:Issuer"] ?? "MPS";
                var audience = _config["Jwt:Audience"] ?? "MPSClients";
                var expireHours = int.TryParse(_config["Jwt:ExpireHours"], out var h) ? h : 1;

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, payload.Subject),
                    new Claim(JwtRegisteredClaimNames.Email, payload.Email ?? string.Empty),
                    new Claim("name", payload.Name ?? string.Empty),
                    new Claim("provider", "google"),
                    new Claim("app", "SampleApp")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwt = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, expires: DateTime.UtcNow.AddHours(expireHours), signingCredentials: creds);
                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Ok(new { token, expires = jwt.ValidTo });
            }
            catch (Exception ex)
            {
                try
                {
                    var logsDir = Path.Combine(AppContext.BaseDirectory, "Logs");
                    Directory.CreateDirectory(logsDir);
                    var fileName = $"google_token_exception_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.log";
                    var filePath = Path.Combine(logsDir, fileName);
                    var details = new
                    {
                        Timestamp = DateTime.UtcNow,
                        Exception = ex.ToString(),
                        ClientId = _config["Authentication:Google:SampleApp.Mobile:ClientId"]
                    };
                    SysFile.WriteAllText(filePath, JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch { }

                Log.Warning(ex, "Error exchanging Google authorization code");
                return StatusCode(502, new { error = "invalid_external_token", detail = "server error, check Logs folder" });
            }
        }
    }
}

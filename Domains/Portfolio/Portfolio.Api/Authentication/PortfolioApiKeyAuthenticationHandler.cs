using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Portfolio.Api.Authentication
{
    public class PortfolioApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> authenticationOptions,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<PortfolioAuthenticationOptions> portfolioOptions)
        : AuthenticationHandler<AuthenticationSchemeOptions>(authenticationOptions, logger, encoder)
    {
        public const string AccessClaimType = "PortfolioAccess";
        public const string FrontEndAccess = "FrontEnd";
        public const string BackEndAccess = "BackEnd";

        private readonly PortfolioAuthenticationOptions _portfolioOptions = portfolioOptions.Value;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(_portfolioOptions.HeaderName, out var headerValues))
                return Task.FromResult(AuthenticateResult.NoResult());

            var apiKey = headerValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(apiKey))
                return Task.FromResult(AuthenticateResult.Fail("The Portfolio API key is missing."));

            var accessLevel = GetAccessLevel(apiKey);

            if (accessLevel is null)
                return Task.FromResult(AuthenticateResult.Fail("The Portfolio API key is invalid."));

            Claim[] claims =
            [
                new(ClaimTypes.NameIdentifier, accessLevel),
                new(ClaimTypes.Name, accessLevel),
                new(AccessClaimType, accessLevel)
            ];

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private string? GetAccessLevel(string apiKey)
        {
            if (KeysEqual(apiKey, _portfolioOptions.BackEndKey))
                return BackEndAccess;

            return KeysEqual(apiKey, _portfolioOptions.FrontEndKey) ? FrontEndAccess : null;
        }

        private static bool KeysEqual(string suppliedKey, string configuredKey)
        {
            if (string.IsNullOrWhiteSpace(configuredKey))
                return false;

            var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
            var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);

            return suppliedBytes.Length == configuredBytes.Length && CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes);
        }
    }
}
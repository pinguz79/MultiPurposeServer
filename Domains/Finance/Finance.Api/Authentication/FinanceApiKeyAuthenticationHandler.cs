using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finance.Api.Authentication
{
    public sealed class FinanceApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> authenticationOptions,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<FinanceAuthenticationOptions> financeOptions)
        : AuthenticationHandler<AuthenticationSchemeOptions>(authenticationOptions, logger, encoder)
    {
        private readonly FinanceAuthenticationOptions _financeOptions = financeOptions.Value;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(_financeOptions.HeaderName, out var headerValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var apiKey = headerValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(apiKey) || !KeysEqual(apiKey, _financeOptions.DesktopKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("The Finance API key is invalid."));
            }

            Claim[] claims =
            [
                new(ClaimTypes.NameIdentifier, "FinanceDesktop"),
                new(ClaimTypes.Name, "FinanceDesktop"),
            ];
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static bool KeysEqual(string suppliedKey, string configuredKey)
        {
            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                return false;
            }

            var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
            var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);

            return suppliedBytes.Length == configuredBytes.Length && CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes);
        }
    }
}

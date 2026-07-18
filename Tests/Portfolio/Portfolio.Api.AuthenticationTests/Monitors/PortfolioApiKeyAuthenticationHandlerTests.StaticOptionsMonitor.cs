using Microsoft.Extensions.Options;

namespace Portfolio.Api.AuthenticationTests
{
    public partial class PortfolioApiKeyAuthenticationHandlerTests
    {
        private sealed class StaticOptionsMonitor<TOptions>(TOptions currentValue) : IOptionsMonitor<TOptions>
        {
            public TOptions CurrentValue { get; } = currentValue;

            public TOptions Get(string? name) => CurrentValue;

            public IDisposable? OnChange(Action<TOptions, string?> listener) => null;
        }
    }
}

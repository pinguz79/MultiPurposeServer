using System.Security.Claims;
using System.Text.Encodings.Web;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Portfolio.Api.Authentication;
using Portfolio.Api.Tests.Authentication.Monitors;

namespace Portfolio.Api.Tests.Authentication
{
    public class PortfolioApiKeyAuthenticationHandlerTests
    {
        private const string FrontEndKey = "portfolio-web-front-end-key";
        private const string BackEndKey = "portfolio-web-back-end-key";

        [Fact]
        public async Task AuthenticateAsync_WhenHeaderIsMissing_ReturnsNoResult()
        {
            // Arrange
            var handler = await CreateHandler();

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.None.Should().BeTrue();
            result.Succeeded.Should().BeFalse();
            result.Failure.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task AuthenticateAsync_WhenApiKeyIsMissing_ReturnsFailure(string apiKey)
        {
            // Arrange
            var handler = await CreateHandler(apiKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.None.Should().BeFalse();
            result.Failure.Should().NotBeNull();
            result.Failure!.Message.Should().Be("The Portfolio API key is missing.");
        }

        [Theory]
        [InlineData("invalid-key")]
        [InlineData("PORTFOLIO-WEB-FRONT-END-KEY")]
        [InlineData("portfolio-web-front-end-key ")]
        public async Task AuthenticateAsync_WhenApiKeyIsInvalid_ReturnsFailure(string apiKey)
        {
            // Arrange
            var handler = await CreateHandler(apiKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.None.Should().BeFalse();
            result.Failure.Should().NotBeNull();
            result.Failure!.Message.Should().Be("The Portfolio API key is invalid.");
        }

        [Fact]
        public async Task AuthenticateAsync_WhenFrontEndKeyIsValid_AuthenticatesPortfolioWebWithFrontEndAccess()
        {
            // Arrange
            var handler = await CreateHandler(FrontEndKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            AssertSuccessfulAuthentication(result, PortfolioApiKeyAuthenticationHandler.FrontEndAccess);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenBackEndKeyIsValid_AuthenticatesPortfolioWebWithBackEndAccess()
        {
            // Arrange
            var handler = await CreateHandler(BackEndKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            AssertSuccessfulAuthentication(result, PortfolioApiKeyAuthenticationHandler.BackEndAccess);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenAuthenticationSucceeds_UsesPortfolioAuthenticationScheme()
        {
            // Arrange
            var handler = await CreateHandler(FrontEndKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Ticket.Should().NotBeNull();
            result.Ticket!.AuthenticationScheme.Should().Be(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
            result.Principal!.Identity!.AuthenticationType.Should().Be(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenCustomHeaderIsConfigured_UsesCustomHeader()
        {
            // Arrange
            const string customHeaderName = "X-Custom-Portfolio-Key";
            var handler = await CreateHandler(FrontEndKey, customHeaderName);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            AssertSuccessfulAuthentication(result, PortfolioApiKeyAuthenticationHandler.FrontEndAccess);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenCustomHeaderIsConfigured_IgnoresDefaultHeader()
        {
            // Arrange
            const string customHeaderName = "X-Custom-Portfolio-Key";
            var handler = await CreateHandler(null, customHeaderName, context => context.Request.Headers[PortfolioAuthenticationOptions.DefaultHeaderName] = FrontEndKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.None.Should().BeTrue();
            result.Succeeded.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateAsync_WhenFrontEndKeyIsNotConfigured_DoesNotAuthenticateFrontEndCaller()
        {
            // Arrange
            var handler = await CreateHandler(FrontEndKey, frontEndKey: string.Empty);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failure!.Message.Should().Be("The Portfolio API key is invalid.");
        }

        [Fact]
        public async Task AuthenticateAsync_WhenBackEndKeyIsNotConfigured_DoesNotAuthenticateBackEndCaller()
        {
            // Arrange
            var handler = await CreateHandler(BackEndKey, backEndKey: string.Empty);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failure!.Message.Should().Be("The Portfolio API key is invalid.");
        }

        [Fact]
        public async Task AuthenticateAsync_WhenFrontEndAndBackEndKeysAreEqual_AssignsBackEndAccess()
        {
            // Arrange
            const string sharedKey = "shared-key";
            var handler = await CreateHandler(sharedKey, frontEndKey: sharedKey, backEndKey: sharedKey);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            AssertSuccessfulAuthentication(result, PortfolioApiKeyAuthenticationHandler.BackEndAccess);
        }

        private static void AssertSuccessfulAuthentication(AuthenticateResult result, string expectedAccessLevel)
        {
            result.Succeeded.Should().BeTrue();
            result.None.Should().BeFalse();
            result.Failure.Should().BeNull();
            result.Principal.Should().NotBeNull();
            result.Principal!.Identity.Should().NotBeNull();
            result.Principal.Identity!.IsAuthenticated.Should().BeTrue();
            result.Principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(expectedAccessLevel);
            result.Principal.FindFirstValue(ClaimTypes.Name).Should().Be(expectedAccessLevel);
            result.Principal.FindFirstValue(PortfolioApiKeyAuthenticationHandler.AccessClaimType).Should().Be(expectedAccessLevel);
        }

        private static async Task<PortfolioApiKeyAuthenticationHandler> CreateHandler(string? apiKey = null, string headerName = PortfolioAuthenticationOptions.DefaultHeaderName, Action<DefaultHttpContext>? configureContext = null, string frontEndKey = FrontEndKey, string backEndKey = BackEndKey)
        {
            var authenticationOptions = new StaticOptionsMonitor<AuthenticationSchemeOptions>(new AuthenticationSchemeOptions());
            var portfolioOptions = Options.Create(new PortfolioAuthenticationOptions { HeaderName = headerName, FrontEndKey = frontEndKey, BackEndKey = backEndKey });
            var handler = new PortfolioApiKeyAuthenticationHandler(authenticationOptions, NullLoggerFactory.Instance, UrlEncoder.Default, portfolioOptions);
            var context = new DefaultHttpContext();

            if (apiKey is not null)
            {
                context.Request.Headers[headerName] = apiKey;
            }

            configureContext?.Invoke(context);

            var scheme = new AuthenticationScheme(PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme, PortfolioApiKeyAuthenticationDefaults.AuthenticationScheme, typeof(PortfolioApiKeyAuthenticationHandler));
            await handler.InitializeAsync(scheme, context);

            return handler;
        }
    }
}

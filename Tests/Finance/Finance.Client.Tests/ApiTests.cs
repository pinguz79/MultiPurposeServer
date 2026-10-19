using System.Net;

using FluentAssertions;

using MultiPurposeServer.Client;

namespace Finance.Client.Tests
{
    public sealed class ApiTests
    {
        [Theory]
        [InlineData("http://example.org")]
        [InlineData("https://user:pass@example.org")]
        [InlineData("https://example.org/?key=secret")]
        [InlineData("not an address")]
        public void ValidateServer_WhenAddressIsUnsafe_ThrowsArgumentException(string url)
        {
            // Arrange

            // Act
            Action action = () => ApiClient.ValidateServer(url);

            // Assert
            action.Should().Throw<ArgumentException>().WithParameterName("address");
        }

        [Fact]
        public async Task GetConti_WhenServerHasBasePath_SendsPathAndCredentialsOnce()
        {
            // Arrange
            using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }));
            using var http = new HttpClient(handler);
            var client = new FinanceApiClientFactory(http).Create(ApiClient.ValidateServer("https://example.org/mps"), "test");

            // Act
            await client.GetConti();

            // Assert
            handler.RequestUri.Should().Be(new Uri("https://example.org/mps/Finance/FrontEnd/Conto/List"));
            handler.ApiKey.Should().Be("test");
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task GetConti_WhenAccountsHaveOptionalIndicators_ReturnsBalances()
        {
            // Arrange
            const string response = """
                [{"id":"00000000-0000-0000-0000-000000000001","name":"Bank","displayName":"Bank","balance":12.34,"previousPeriodBalance":9.87},
                 {"id":"00000000-0000-0000-0000-000000000002","name":"Card","displayName":"Card","balance":100,
                  "revolvingIndicators":{"plafond":500,"overdraft":0,"remainingPlafond":400,"remainingIncludingOverdraft":400}}]
                """;
            using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) }));
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.org/") };
            var client = new FinanceApiClient(new ApiClient(http));

            // Act
            IReadOnlyList<Conto> accounts = await client.GetConti();

            // Assert
            accounts.Should().HaveCount(2);
            accounts[0].PreviousPeriodBalance.Should().Be(9.87m);
            accounts[1].RevolvingIndicators.Should().Be(new RevolvingIndicators(500m, 0m, 400m, 400m));
        }

        [Theory]
        [InlineData(503, "<html>Errore server</html>")]
        [InlineData(401, "")]
        [InlineData(503, "[]")]
        [InlineData(200, "null")]
        [InlineData(200, "not json")]
        public async Task GetConti_WhenResponseIsInvalid_ThrowsApiExceptionWithoutRetry(int status, string body)
        {
            // Arrange
            using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(body) }));
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.org/") };
            var client = new FinanceApiClient(new ApiClient(http));

            // Act
            Func<Task> action = () => client.GetConti();

            // Assert
            var assertion = await action.Should().ThrowAsync<ApiException>();
            assertion.Which.StatusCode.Should().Be((HttpStatusCode)status);
            assertion.Which.Message.Should().NotContain("<html>");
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task GetConti_WhenConnectionFails_ThrowsHttpRequestExceptionWithoutRetry()
        {
            // Arrange
            using var handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("Connessione assente."));
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.org/") };
            var client = new FinanceApiClient(new ApiClient(http));

            // Act
            Func<Task> action = () => client.GetConti();

            // Assert
            await action.Should().ThrowAsync<HttpRequestException>();
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task GetConti_WhenAccountHasCycle_ReturnsDatesAndThresholds()
        {
            // Arrange
            const string response = """
                [{"id":"00000000-0000-0000-0000-000000000003","name":"Card","displayName":"Card","balance":10,"cycleIndicators":
                {"from":"2026-09-22","to":"2026-10-21","currentCycleSpent":12.34,"plafond":5000,"overdraftPercentage":0.1,"overdraft":500,
                "remainingPlafond":4987.66,"remainingIncludingOverdraft":5487.66,"pendingDebit":77.88,
                "firstPlafondExceeded":{"date":"2026-10-20","balance":5100,"excess":100},"firstTotalLimitExceeded":null}}]
                """;
            using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) }));
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.org/") };
            var client = new FinanceApiClient(new ApiClient(http));

            // Act
            IReadOnlyList<Conto> accounts = await client.GetConti();

            // Assert
            Conto account = accounts.Should().ContainSingle().Subject;
            account.CycleIndicators.Should().NotBeNull();
            account.CycleIndicators?.To.Should().Be(new DateOnly(2026, 10, 21));
            account.CycleIndicators?.PendingDebit.Should().Be(77.88m);
            account.CycleIndicators?.FirstPlafondExceeded.Should().Be(new CycleThreshold(new DateOnly(2026, 10, 20), 5100m, 100m));
        }

        [Fact]
        public async Task GetConti_WhenRequestIsCancelled_PropagatesCancellationWithoutRetry()
        {
            // Arrange
            using var cancellation = new CancellationTokenSource();
            using var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
            {
                cancellation.Cancel();
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return new HttpResponseMessage();
            });
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.org/") };
            var client = new FinanceApiClient(new ApiClient(http));

            // Act
            Func<Task> action = () => client.GetConti(cancellation.Token);

            // Assert
            await action.Should().ThrowAsync<OperationCanceledException>();
            handler.Calls.Should().Be(1);
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Options;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Portfolio.Api.ServiceTests.Services
{
    public class CacheServiceTests
    {
        [Fact]
        public async Task Clear_WhenRequestSucceeds_SendsExpectedRequestAndReturnsResult()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            string? capturedBody = null;

            var handler = new StubHttpMessageHandler(async request =>
            {
                capturedRequest = request;
                capturedBody = await request.Content!.ReadAsStringAsync();

                return CreateJsonResponse(HttpStatusCode.OK, """
                {
                    "albumRoutingEntriesDeleted": 12,
                    "photoRoutingEntriesDeleted": 8,
                    "apiResponseEntriesDeleted": 25
                }
                """);
            });

            var service = CreateService(handler);

            // Act
            var result = await service.Clear(true, false, true);

            // Assert
            result.Should().BeEquivalentTo(new
            {
                AlbumRoutingEntriesDeleted = 12,
                PhotoRoutingEntriesDeleted = 8,
                ApiResponseEntriesDeleted = 25
            });

            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Post);
            capturedRequest.RequestUri.Should().Be(new Uri("https://portfolio.test/internal/cache/clear/"));
            capturedRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            capturedRequest.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
            capturedRequest.Headers.GetValues("X-Portfolio-Shared-Secret").Should().ContainSingle().Which.Should().Be("test-shared-secret");

            using var document = JsonDocument.Parse(capturedBody!);
            document.RootElement.GetProperty("clearAlbumRoutingCache").GetBoolean().Should().BeTrue();
            document.RootElement.GetProperty("clearPhotoRoutingCache").GetBoolean().Should().BeFalse();
            document.RootElement.GetProperty("clearApiResponseCache").GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task Clear_WhenAllFlagsAreFalse_SendsAllFlagsAsFalse()
        {
            // Arrange
            string? capturedBody = null;

            var handler = new StubHttpMessageHandler(async request =>
            {
                capturedBody = await request.Content!.ReadAsStringAsync();

                return CreateJsonResponse(HttpStatusCode.OK, """
                {
                    "albumRoutingEntriesDeleted": 0,
                    "photoRoutingEntriesDeleted": 0,
                    "apiResponseEntriesDeleted": 0
                }
                """);
            });

            var service = CreateService(handler);

            // Act
            await service.Clear(false, false, false);

            // Assert
            using var document = JsonDocument.Parse(capturedBody!);
            document.RootElement.GetProperty("clearAlbumRoutingCache").GetBoolean().Should().BeFalse();
            document.RootElement.GetProperty("clearPhotoRoutingCache").GetBoolean().Should().BeFalse();
            document.RootElement.GetProperty("clearApiResponseCache").GetBoolean().Should().BeFalse();
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task Clear_WhenResponseIsNotSuccessful_ThrowsHttpRequestException(HttpStatusCode statusCode)
        {
            // Arrange
            var responseBody = """{"error":"Cache clear failed."}""";
            var handler = new StubHttpMessageHandler(_ => Task.FromResult(CreateJsonResponse(statusCode, responseBody)));
            var service = CreateService(handler);

            // Act
            var action = async () => await service.Clear(true, false, false);

            // Assert
            var exception = await action.Should().ThrowAsync<HttpRequestException>();
            exception.Which.Message.Should().Contain(((int)statusCode).ToString());
            exception.Which.Message.Should().Contain(responseBody);
        }

        [Fact]
        public async Task Clear_WhenResponseContainsInvalidJson_ThrowsInvalidOperationException()
        {
            // Arrange
            const string responseBody = "<html>Invalid response</html>";
            var handler = new StubHttpMessageHandler(_ => Task.FromResult(CreateResponse(HttpStatusCode.OK, responseBody, "text/html")));
            var service = CreateService(handler);

            // Act
            var action = async () => await service.Clear(true, false, false);

            // Assert
            var exception = await action.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Message.Should().Contain("Portfolio.Web returned invalid JSON");
            exception.Which.Message.Should().Contain(responseBody);
            exception.Which.InnerException.Should().BeOfType<JsonException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("null")]
        public async Task Clear_WhenResponseIsEmpty_ThrowsInvalidOperationException(string responseBody)
        {
            // Arrange
            var handler = new StubHttpMessageHandler(_ => Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, responseBody)));
            var service = CreateService(handler);

            // Act
            var action = async () => await service.Clear(true, false, false);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Clear_WhenResponsePropertyNamesUseDifferentCase_DeserializesResult()
        {
            // Arrange
            var handler = new StubHttpMessageHandler(_ => Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, """
            {
                "ALBUMROUTINGENTRIESDELETED": 3,
                "PhotoRoutingEntriesDeleted": 4,
                "apiresponseentriesdeleted": 5
            }
            """)));

            var service = CreateService(handler);

            // Act
            var result = await service.Clear(true, true, true);

            // Assert
            result.Should().BeEquivalentTo(new
            {
                AlbumRoutingEntriesDeleted = 3,
                PhotoRoutingEntriesDeleted = 4,
                ApiResponseEntriesDeleted = 5
            });
        }

        [Fact]
        public async Task Clear_WhenCalledMultipleTimes_SendsOneRequestForEachCall()
        {
            // Arrange
            var requestCount = 0;

            var handler = new StubHttpMessageHandler(_ =>
            {
                requestCount++;

                return Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, """
                {
                    "albumRoutingEntriesDeleted": 0,
                    "photoRoutingEntriesDeleted": 0,
                    "apiResponseEntriesDeleted": 0
                }
                """));
            });

            var service = CreateService(handler);

            // Act
            await service.Clear(true, false, false);
            await service.Clear(false, true, false);

            // Assert
            requestCount.Should().Be(2);
        }

        private static CacheService CreateService(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://portfolio.test/")
            };

            var options = Options.Create(new PortfolioCacheOptions
            {
                BaseUrl = "https://portfolio.test/",
                ClearEndpoint = "internal/cache/clear/",
                SharedSecret = "test-shared-secret"
            });

            return new CacheService(httpClient, options);
        }

        private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string content)
        {
            return CreateResponse(statusCode, content, "application/json");
        }

        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content, string mediaType)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, mediaType)
            };
        }

        private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handler(request);
        }
    }
}
using FluentAssertions;
using Moq;
using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Infrastructure.Clients;

namespace Portfolio.Api.Tests.Application.Services
{
    public class CacheServiceTests
    {
        private readonly Mock<IPortfolioWebCacheClient> _client;
        private readonly CacheService _service;

        public CacheServiceTests()
        {
            _client = new Mock<IPortfolioWebCacheClient>();
            _service = new CacheService(_client.Object);
        }

        [Fact]
        public async Task Clear_WhenCalled_DelegatesToClientAndReturnsResult()
        {
            // Arrange
            var expected = new CacheClearOperationResult
            {
                AlbumRoutingEntriesDeleted = 12,
                PhotoRoutingEntriesDeleted = 8,
                ApiResponseEntriesDeleted = 25
            };

            _client.Setup(client => client.Clear(true, false, true)).ReturnsAsync(expected);

            // Act
            var result = await _service.Clear(true, false, true);

            // Assert
            result.Should().BeSameAs(expected);
            _client.Verify(client => client.Clear(true, false, true), Times.Once);
        }

        [Fact]
        public async Task Clear_WhenClientThrows_PropagatesException()
        {
            // Arrange
            var expected = new HttpRequestException("Portfolio.Web cache clear failed.");

            _client.Setup(client => client.Clear(true, false, false)).ThrowsAsync(expected);

            // Act
            var action = async () => await _service.Clear(true, false, false);

            // Assert
            var exception = await action.Should().ThrowAsync<HttpRequestException>();

            exception.Which.Should().BeSameAs(expected);
        }
    }
}
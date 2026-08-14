using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.BackEnd;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;

namespace Portfolio.Api.Tests.Controllers.BackEnd
{
    public class CacheControllerTests
    {
        private readonly Mock<ICacheService> _cacheService;
        private readonly CacheController _controller;

        public CacheControllerTests()
        {
            _cacheService = new Mock<ICacheService>();
            _controller = new CacheController(_cacheService.Object);
        }
        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public async Task ClearCache_WhenAtLeastOneCacheIsSelected_CallsServiceWithExpectedFlags(bool clearAlbums, bool clearPhotos, bool clearApiResponses)
        {
            // Arrange
            var request = new CacheClearRequest(clearAlbums, clearPhotos, clearApiResponses);
            var serviceResult = new CacheClearOperationResult
            {
                AlbumRoutingEntriesDeleted = 1,
                PhotoRoutingEntriesDeleted = 2,
                ApiResponseEntriesDeleted = 3
            };

            _cacheService.Setup(service => service.Clear(clearAlbums, clearPhotos, clearApiResponses)).ReturnsAsync(serviceResult);

            // Act
            await _controller.ClearCache(request);

            // Assert
            _cacheService.Verify(service => service.Clear(clearAlbums, clearPhotos, clearApiResponses), Times.Once);
        }

        [Fact]
        public async Task ClearCache_WhenServiceSucceeds_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new CacheClearRequest(true, true, true);
            var serviceResult = new CacheClearOperationResult
            {
                AlbumRoutingEntriesDeleted = 12,
                PhotoRoutingEntriesDeleted = 8,
                ApiResponseEntriesDeleted = 25
            };

            _cacheService.Setup(service => service.Clear(true, true, true)).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.ClearCache(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<CacheClearResult>().Subject;

            response.Should().BeEquivalentTo(new
            {
                AlbumRoutingEntriesDeleted = 12,
                PhotoRoutingEntriesDeleted = 8,
                ApiResponseEntriesDeleted = 25
            });
        }

        [Fact]
        public async Task ClearCache_WhenServiceReturnsZeros_ReturnsOkWithZeroCounts()
        {
            // Arrange
            var request = new CacheClearRequest(true, false, false);
            var serviceResult = new CacheClearOperationResult();

            _cacheService.Setup(service => service.Clear(true, false, false)).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.ClearCache(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<CacheClearResult>().Subject;

            response.Should().BeEquivalentTo(new
            {
                AlbumRoutingEntriesDeleted = 0,
                PhotoRoutingEntriesDeleted = 0,
                ApiResponseEntriesDeleted = 0
            });
        }

        [Fact]
        public async Task ClearCache_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new CacheClearRequest(true, false, false);
            var expectedException = new HttpRequestException("Cache clear failed.");

            _cacheService.Setup(service => service.Clear(true, false, false)).ThrowsAsync(expectedException);

            // Act
            var action = async () => await _controller.ClearCache(request);

            // Assert
            var exception = await action.Should().ThrowAsync<HttpRequestException>();
            exception.Which.Should().BeSameAs(expectedException);
        }
    }
}

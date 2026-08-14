using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.FrontEnd;

namespace Portfolio.Api.Tests.Controllers.FrontEnd
{
    public class MediaControllerTests
    {
        private const string CacheControlValue = "public, max-age=864000";

        private readonly Mock<IMediaService> _mediaService;
        private readonly MediaController _controller;

        public MediaControllerTests()
        {
            _mediaService = new Mock<IMediaService>();
            _controller = new MediaController(_mediaService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        #region Cover

        [Fact]
        public async Task GetCover_WhenMediaExists_ReturnsPhysicalFileAndSetsCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var mediaFile = CreateMediaFile("cover.jpg");

            _mediaService.Setup(service => service.GetCoverPhoto(photoId)).ReturnsAsync(mediaFile);

            // Act
            var result = await _controller.GetCover(photoId);

            // Assert
            AssertPhysicalFileResult(result, mediaFile);
            _controller.Response.Headers.CacheControl.ToString().Should().Be(CacheControlValue);
            _mediaService.Verify(service => service.GetCoverPhoto(photoId), Times.Once);
        }

        [Fact]
        public async Task GetCover_WhenMediaDoesNotExist_ReturnsNotFoundWithoutCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            _mediaService.Setup(service => service.GetCoverPhoto(photoId)).ReturnsAsync((MediaFile?)null);

            // Act
            var result = await _controller.GetCover(photoId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
            _mediaService.Verify(service => service.GetCoverPhoto(photoId), Times.Once);
        }

        [Fact]
        public async Task GetCover_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var exception = new InvalidOperationException("Cover generation failed.");

            _mediaService.Setup(service => service.GetCoverPhoto(photoId)).ThrowsAsync(exception);

            // Act
            Func<Task> act = () => _controller.GetCover(photoId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(exception.Message);
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task GetEditorialCover_WhenMediaExists_ReturnsPhysicalFileAndSetsCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var mediaFile = CreateMediaFile("editorial-cover.jpg");

            _mediaService.Setup(service => service.GetEditorialCoverPhoto(photoId)).ReturnsAsync(mediaFile);

            // Act
            var result = await _controller.GetEditorialCover(photoId);

            // Assert
            AssertPhysicalFileResult(result, mediaFile);
            _controller.Response.Headers.CacheControl.ToString().Should().Be(CacheControlValue);
            _mediaService.Verify(service => service.GetEditorialCoverPhoto(photoId), Times.Once);
        }

        #endregion

        #region Thumbnail

        [Fact]
        public async Task GetThumbnail_WhenMediaExists_ReturnsPhysicalFileAndSetsCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var mediaFile = CreateMediaFile("thumbnail.jpg");

            _mediaService.Setup(service => service.GetThumbnailPhoto(photoId)).ReturnsAsync(mediaFile);

            // Act
            var result = await _controller.GetThumbnail(photoId);

            // Assert
            AssertPhysicalFileResult(result, mediaFile);
            _controller.Response.Headers.CacheControl.ToString().Should().Be(CacheControlValue);
            _mediaService.Verify(service => service.GetThumbnailPhoto(photoId), Times.Once);
        }

        [Fact]
        public async Task GetThumbnail_WhenMediaDoesNotExist_ReturnsNotFoundWithoutCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            _mediaService.Setup(service => service.GetThumbnailPhoto(photoId)).ReturnsAsync((MediaFile?)null);

            // Act
            var result = await _controller.GetThumbnail(photoId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
            _mediaService.Verify(service => service.GetThumbnailPhoto(photoId), Times.Once);
        }

        [Fact]
        public async Task GetThumbnail_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var exception = new InvalidOperationException("Thumbnail generation failed.");

            _mediaService.Setup(service => service.GetThumbnailPhoto(photoId)).ThrowsAsync(exception);

            // Act
            Func<Task> act = () => _controller.GetThumbnail(photoId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(exception.Message);
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
        }

        #endregion

        #region Image

        [Fact]
        public async Task GetImage_WhenMediaExists_ReturnsPhysicalFileAndSetsCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var mediaFile = CreateMediaFile("image.jpg");

            _mediaService.Setup(service => service.GetImagePhoto(photoId)).ReturnsAsync(mediaFile);

            // Act
            var result = await _controller.GetImage(photoId);

            // Assert
            AssertPhysicalFileResult(result, mediaFile);
            _controller.Response.Headers.CacheControl.ToString().Should().Be(CacheControlValue);
            _mediaService.Verify(service => service.GetImagePhoto(photoId), Times.Once);
        }

        [Fact]
        public async Task GetImage_WhenMediaDoesNotExist_ReturnsNotFoundWithoutCacheHeader()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            _mediaService.Setup(service => service.GetImagePhoto(photoId)).ReturnsAsync((MediaFile?)null);

            // Act
            var result = await _controller.GetImage(photoId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
            _mediaService.Verify(service => service.GetImagePhoto(photoId), Times.Once);
        }

        [Fact]
        public async Task GetImage_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var exception = new InvalidOperationException("Image generation failed.");

            _mediaService.Setup(service => service.GetImagePhoto(photoId)).ThrowsAsync(exception);

            // Act
            Func<Task> act = () => _controller.GetImage(photoId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(exception.Message);
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
        }

        #endregion

        private static MediaFile CreateMediaFile(string fileName)
        {
            return new MediaFile
            {
                FilePath = Path.Combine(Path.GetTempPath(), fileName),
                ContentType = "image/jpeg"
            };
        }


        #region Assert e Verify

        private static void AssertPhysicalFileResult(IActionResult result, MediaFile expected)
        {
            var physicalFile = result.Should().BeOfType<PhysicalFileResult>().Subject;

            physicalFile.FileName.Should().Be(expected.FilePath);
            physicalFile.ContentType.Should().Be(expected.ContentType);
        }

        #endregion

    }
}

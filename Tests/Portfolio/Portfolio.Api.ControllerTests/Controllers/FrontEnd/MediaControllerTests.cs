using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.FrontEnd;
using Portfolio.Api.Application.Models;

namespace Portfolio.Api.ControllerTests.Controllers.FrontEnd
{
    public class MediaControllerTests
    {
        private const string CacheControlValue = "public, max-age=864000";

        private readonly Mock<IMediaService> _mediaService;
        private readonly Mock<ILogger<MediaController>> _logger;
        private readonly MediaController _controller;

        public MediaControllerTests()
        {
            _mediaService = new Mock<IMediaService>();
            _logger = new Mock<ILogger<MediaController>>();

            _controller = new MediaController(_mediaService.Object, _logger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

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
        public async Task GetCover_WhenServiceThrows_ReturnsProblemWithExpectedDetails()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var exception = new InvalidOperationException("Cover generation failed.");

            _mediaService.Setup(service => service.GetCoverPhoto(photoId)).ThrowsAsync(exception);

            // Act
            var result = await _controller.GetCover(photoId);

            // Assert
            AssertProblemResult(result, "Errore nella generazione della cover", exception.Message);
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
            VerifyErrorWasLogged(exception);
        }

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
        public async Task GetThumbnail_WhenServiceThrows_ReturnsProblemWithExpectedDetails()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var exception = new InvalidOperationException("Thumbnail generation failed.");

            _mediaService.Setup(service => service.GetThumbnailPhoto(photoId)).ThrowsAsync(exception);

            // Act
            var result = await _controller.GetThumbnail(photoId);

            // Assert
            AssertProblemResult(result, "Errore nella generazione della miniatura", exception.Message);
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
            VerifyErrorWasLogged(exception);
        }

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
        public async Task GetImage_WhenServiceThrows_ReturnsProblemWithExpectedDetails()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var exception = new InvalidOperationException("Image generation failed.");

            _mediaService.Setup(service => service.GetImagePhoto(photoId)).ThrowsAsync(exception);

            // Act
            var result = await _controller.GetImage(photoId);

            // Assert
            AssertProblemResult(result, "Errore nella generazione dell'immagine", exception.Message);
            _controller.Response.Headers.CacheControl.ToString().Should().BeEmpty();
            VerifyErrorWasLogged(exception);
        }

        private static MediaFile CreateMediaFile(string fileName)
        {
            return new MediaFile
            {
                FilePath = Path.Combine(Path.GetTempPath(), fileName),
                ContentType = "image/jpeg"
            };
        }

        private static void AssertPhysicalFileResult(IActionResult result, MediaFile expected)
        {
            var physicalFile = result.Should().BeOfType<PhysicalFileResult>().Subject;

            physicalFile.FileName.Should().Be(expected.FilePath);
            physicalFile.ContentType.Should().Be(expected.ContentType);
        }

        private static void AssertProblemResult(IActionResult result, string expectedTitle, string expectedDetail)
        {
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            problem.Title.Should().Be(expectedTitle);
            problem.Detail.Should().Be(expectedDetail);
            problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
        }

        private void VerifyErrorWasLogged(Exception exception)
        {
            _logger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((_, _) => true), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
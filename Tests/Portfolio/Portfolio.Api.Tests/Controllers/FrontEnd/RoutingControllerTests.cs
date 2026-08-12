using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.FrontEnd;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Controllers.FrontEnd
{
    public class RoutingControllerTests
    {
        private readonly Mock<IAlbumService> _albumService;
        private readonly RoutingController _controller;

        public RoutingControllerTests()
        {
            _albumService = new Mock<IAlbumService>();
            var logger = new Mock<ILogger<RoutingController>>();

            _controller = new RoutingController(_albumService.Object, logger.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public async Task ResolveAlbumPath_WhenPathIsMissing_ReturnsBadRequestWithoutCallingService(string? path)
        {
            // Arrange

            // Act
            var result = await _controller.ResolveAlbumPath(path!);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
            _albumService.Verify(service => service.ResolvePath(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResolveAlbumPath_WhenAlbumDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string path = "Fashion/Milano";

            _albumService.Setup(service => service.ResolvePath(path)).ReturnsAsync((Album?)null);

            // Act
            var result = await _controller.ResolveAlbumPath(path);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _albumService.Verify(service => service.ResolvePath(path), Times.Once);
        }

        [Fact]
        public async Task ResolveAlbumPath_WhenAlbumExists_ReturnsOkWithMappedDto()
        {
            // Arrange
            const string path = "Fashion/Milano";
            var album = new Album
            {
                Id = Guid.NewGuid(),
                Name = "Milano",
                Path = "Milano",
                ParentId = Guid.NewGuid()
            };

            _albumService.Setup(service => service.ResolvePath(path)).ReturnsAsync(album);

            // Act
            var result = await _controller.ResolveAlbumPath(path);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<AlbumDto>().Subject;

            dto.Should().BeEquivalentTo(new { album.Id, album.Name });
            _albumService.Verify(service => service.ResolvePath(path), Times.Once);
        }

        [Fact]
        public async Task ResolveAlbumPath_WhenPathContainsOuterSpaces_PassesOriginalPathToService()
        {
            // Arrange
            const string path = "  Fashion/Milano  ";
            var album = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano" };

            _albumService.Setup(service => service.ResolvePath(path)).ReturnsAsync(album);

            // Act
            await _controller.ResolveAlbumPath(path);

            // Assert
            _albumService.Verify(service => service.ResolvePath(path), Times.Once);
        }

        [Fact]
        public async Task ResolveAlbumPath_WhenPathContainsDifferentFormatting_PassesValueUnchangedToService()
        {
            // Arrange
            const string path = @"\Fashion\Milano\";
            var album = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano" };

            _albumService.Setup(service => service.ResolvePath(path)).ReturnsAsync(album);

            // Act
            await _controller.ResolveAlbumPath(path);

            // Assert
            _albumService.Verify(service => service.ResolvePath(path), Times.Once);
        }
    }
}

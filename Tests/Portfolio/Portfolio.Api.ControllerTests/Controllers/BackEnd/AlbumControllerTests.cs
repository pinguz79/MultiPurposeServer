using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Portfolio.Api.Controllers.BackEnd;
using Portfolio.Api.Services;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.ControllerTests.Controllers.BackEnd
{
    public class AlbumControllerTests
    {
        private readonly Mock<IAlbumService> _albumService;
        private readonly AlbumController _controller;

        public AlbumControllerTests()
        {
            _albumService = new Mock<IAlbumService>();
            var logger = new Mock<ILogger<AlbumController>>();

            _controller = new AlbumController(_albumService.Object, logger.Object);
        }

        [Fact]
        public async Task GetList_WhenRepositoryReturnsAlbums_ReturnsOkWithMappedDtos()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = parentId },
                new() { Id = Guid.NewGuid(), Name = "Glamour", Path = "Glamour", ParentId = parentId }
            };

            _albumService.Setup(service => service.GetAlbums(parentId)).ReturnsAsync(albums);

            // Act
            var result = await _controller.GetList(parentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Should().HaveCount(2);
            dtos.Select(dto => dto.Id).Should().BeEquivalentTo(albums.Select(album => album.Id));
            dtos.Select(dto => dto.Name).Should().BeEquivalentTo(["Fashion", "Glamour"]);

            _albumService.Verify(service => service.GetAlbums(parentId), Times.Once);
        }

        [Fact]
        public async Task GetList_WhenIdIsNull_ReturnsRootAlbums()
        {
            // Arrange
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null }
            };

            _albumService.Setup(service => service.GetAlbums(null)).ReturnsAsync(albums);

            // Act
            var result = await _controller.GetList();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Should().ContainSingle();
            dtos.Single().Id.Should().Be(albums.Single().Id);

            _albumService.Verify(service => service.GetAlbums(null), Times.Once);
        }

        [Fact]
        public async Task GetList_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _albumService.Setup(service => service.GetAlbums(null)).ReturnsAsync([]);

            // Act
            var result = await _controller.GetList();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Should().BeEmpty();
        }

        [Fact]
        public async Task Get_WhenAlbumExists_ReturnsOkWithMappedDto()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Fashion", Path = "Fashion" };

            _albumService.Setup(service => service.GetById(albumId)).ReturnsAsync(album);

            // Act
            var result = await _controller.Get(albumId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<AlbumDto>().Subject;

            dto.Should().BeEquivalentTo(new { Id = albumId, Name = "Fashion" });
            _albumService.Verify(service => service.GetById(albumId), Times.Once);
        }

        [Fact]
        public async Task Get_WhenAlbumDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            _albumService.Setup(service => service.GetById(albumId)).ReturnsAsync((Album?)null);

            // Act
            var result = await _controller.Get(albumId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _albumService.Verify(service => service.GetById(albumId), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Create_WhenNameIsMissing_ReturnsBadRequestWithoutCallingService(string? name)
        {
            // Arrange
            var request = new CreateAlbumRequest(name!);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Album name is required.");

            _albumService.Verify(service => service.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task Create_WhenRequestIsValid_CreatesAlbumAndReturnsCreatedAtAction()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = parentId };
            var request = new CreateAlbumRequest("Fashion", parentId);

            _albumService.Setup(service => service.CreateAlbum("Fashion", parentId)).ReturnsAsync(album);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(AlbumController.Get));
            createdResult.RouteValues.Should().ContainKey("albumId").WhoseValue.Should().Be(album.Id);

            var dto = createdResult.Value.Should().BeOfType<AlbumDto>().Subject;
            dto.Should().BeEquivalentTo(new { album.Id, Name = "Fashion" });

            _albumService.Verify(service => service.CreateAlbum("Fashion", parentId), Times.Once);
        }

        [Fact]
        public async Task Create_WhenNameContainsOuterSpaces_PassesOriginalNameToService()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion" };
            var request = new CreateAlbumRequest("  Fashion  ");

            _albumService.Setup(service => service.CreateAlbum("  Fashion  ", null)).ReturnsAsync(album);

            // Act
            await _controller.Create(request);

            // Assert
            _albumService.Verify(service => service.CreateAlbum("  Fashion  ", null), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Update_WhenNameIsMissing_ReturnsBadRequestWithoutCallingService(string? name)
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new UpdateAlbumRequest(name!, null);

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Album name is required.");

            _albumService.Verify(service => service.UpdateName(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Update_WhenAlbumExists_TrimsNameAndReturnsOkWithMappedDto()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Fashion Updated", Path = "Fashion" };
            var request = new UpdateAlbumRequest("  Fashion Updated  ", null);

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Updated")).ReturnsAsync(album);

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<AlbumDto>().Subject;

            dto.Should().BeEquivalentTo(new { Id = albumId, Name = "Fashion Updated" });
            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Updated"), Times.Once);
        }

        [Fact]
        public async Task Update_WhenAlbumDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new UpdateAlbumRequest("Fashion Updated", null);

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Updated")).ReturnsAsync((Album?)null);

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Updated"), Times.Once);
        }
    }
}
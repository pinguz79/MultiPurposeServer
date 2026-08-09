using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.BackEnd;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Controllers.BackEnd
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
        [Fact]
        public async Task Create_WhenRequestIsValid_CreatesAlbumAndReturnsCreatedAtAction()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Description = "Editorial fashion", Path = "Fashion", ParentId = parentId };
            var request = new CreateAlbumRequest("Fashion", parentId, "Editorial fashion");

            _albumService.Setup(service => service.CreateAlbum("Fashion", parentId, "Editorial fashion")).ReturnsAsync(album);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(AlbumController.Get));
            createdResult.RouteValues.Should().ContainKey("albumId").WhoseValue.Should().Be(album.Id);

            var dto = createdResult.Value.Should().BeOfType<AlbumDto>().Subject;
            dto.Should().BeEquivalentTo(new { album.Id, Name = "Fashion", Description = "Editorial fashion" });

            _albumService.Verify(service => service.CreateAlbum("Fashion", parentId, "Editorial fashion"), Times.Once);
        }
        [Fact]
        public async Task Update_WhenOnlyNameIsSpecified_UpdatesNameAndCompletesOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Fashion Updated", Path = "Fashion" };
            var request = new UpdateAlbumRequest("Fashion Updated", null);
            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Updated")).ReturnsAsync(album);

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<AlbumDto>().Subject;

            dto.Should().BeEquivalentTo(new { Id = albumId, Name = "Fashion Updated" });

            _albumService.Verify(service => service.BeginOperation(), Times.Once);
            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Updated"), Times.Once);
            _albumService.Verify(service => service.UpdateDescription(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            operation.Verify(value => value.Complete(), Times.Once);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenOnlyDescriptionIsSpecified_UpdatesDescriptionAndCompletesOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album
            {
                Id = albumId,
                Name = "Fashion",
                Description = "Fashion photography",
                Path = "Fashion"
            };

            var request = new UpdateAlbumRequest(null, "Fashion photography");
            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateDescription(albumId, "Fashion photography")).ReturnsAsync(album);

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<AlbumDto>().Subject;

            dto.Should().BeEquivalentTo(new
            {
                Id = albumId,
                Name = "Fashion",
                Description = "Fashion photography"
            });

            _albumService.Verify(service => service.BeginOperation(), Times.Once);
            _albumService.Verify(service => service.UpdateName(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _albumService.Verify(service => service.UpdateDescription(albumId, "Fashion photography"), Times.Once);
            operation.Verify(value => value.Complete(), Times.Once);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenNameAndDescriptionAreSpecified_UpdatesBothFieldsAndCompletesOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var nameUpdatedAlbum = new Album { Id = albumId, Name = "Fashion Updated", Path = "Fashion" };

            var fullyUpdatedAlbum = new Album
            {
                Id = albumId,
                Name = "Fashion Updated",
                Description = "Updated description",
                Path = "Fashion"
            };

            var request = new UpdateAlbumRequest("Fashion Updated", "Updated description");
            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Updated")).ReturnsAsync(nameUpdatedAlbum);
            _albumService.Setup(service => service.UpdateDescription(albumId, "Updated description")).ReturnsAsync(fullyUpdatedAlbum);

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<AlbumDto>().Subject;

            dto.Should().BeEquivalentTo(new
            {
                Id = albumId,
                Name = "Fashion Updated",
                Description = "Updated description"
            });

            _albumService.Verify(service => service.BeginOperation(), Times.Once);
            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Updated"), Times.Once);
            _albumService.Verify(service => service.UpdateDescription(albumId, "Updated description"), Times.Once);
            operation.Verify(value => value.Complete(), Times.Once);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenAlbumDoesNotExist_ReturnsNotFoundWithoutCompletingOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new UpdateAlbumRequest("Fashion Updated", null);
            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Updated"))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            result.Should().BeOfType<NotFoundResult>();

            _albumService.Verify(service => service.BeginOperation(), Times.Once);
            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Updated"), Times.Once);
            operation.Verify(value => value.Complete(), Times.Never);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenSecondUpdateDoesNotFindAlbum_ReturnsNotFoundWithoutCompletingOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Fashion Updated", Path = "Fashion" };
            var request = new UpdateAlbumRequest("Fashion Updated", "Updated description");
            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Updated")).ReturnsAsync(album);
            _albumService.Setup(service => service.UpdateDescription(albumId, "Updated description")).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Update(albumId, request);

            // Assert
            result.Should().BeOfType<NotFoundResult>();

            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Updated"), Times.Once);
            _albumService.Verify(service => service.UpdateDescription(albumId, "Updated description"), Times.Once);
            operation.Verify(value => value.Complete(), Times.Never);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenAlbumIsEmpty_DeletesAlbumAndReturnsNoContent()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            // Act
            var result = await _controller.Delete(albumId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _albumService.Verify(service => service.DeleteEmptyAlbum(albumId), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenAlbumDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            _albumService.Setup(service => service.DeleteEmptyAlbum(albumId)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Delete(albumId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_WhenAlbumIsNotEmpty_ReturnsConflictWithReason()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            _albumService.Setup(service => service.DeleteEmptyAlbum(albumId))
                .ThrowsAsync(new InvalidOperationException("Album contains photos."));

            // Act
            var result = await _controller.Delete(albumId);

            // Assert
            var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflict.Value.Should().Be("Album contains photos.");
        }

        private Mock<IApplicationOperation> SetupOperation()
        {
            var operation = new Mock<IApplicationOperation>();

            _albumService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);

            return operation;
        }
    }
}

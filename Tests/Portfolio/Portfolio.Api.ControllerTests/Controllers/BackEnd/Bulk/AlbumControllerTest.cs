using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Portfolio.Api.Controllers.BackEnd.Bulk;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Operations;
using Portfolio.Contracts.Bulk.Enums;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Bulk.Responses;
using Portfolio.Data.Models;

namespace Portfolio.Api.ControllerTests.Controllers.BackEnd.Bulk
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

        private Mock<IApplicationOperation> SetupOperation()
        {
            var operation = new Mock<IApplicationOperation>();
            _albumService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            return operation;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task MatchNames_WhenPatternIsMissing_ReturnsBadRequestWithoutCallingService(string? pattern)
        {
            // Arrange

            // Act
            var result = await _controller.MatchNames(pattern!);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Regex pattern is required.");
            _albumService.Verify(service => service.GetByNamePattern(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task MatchNames_WhenAlbumsMatch_ReturnsOkWithMappedDtos()
        {
            // Arrange
            const string pattern = "^fashion";
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "Fashion Milano", Path = "Fashion-Milano" },
                new() { Id = Guid.NewGuid(), Name = "Fashion Roma", Path = "Fashion-Roma" }
            };

            _albumService.Setup(service => service.GetByNamePattern(pattern)).ReturnsAsync(albums);

            // Act
            var result = await _controller.MatchNames(pattern);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumMatchDto>>().Subject;

            dtos.Should().BeEquivalentTo(
            [
                new { albums[0].Id, albums[0].Name },
                new { albums[1].Id, albums[1].Name }
            ]);

            _albumService.Verify(service => service.GetByNamePattern(pattern), Times.Once);
        }

        [Fact]
        public async Task MatchNames_WhenNoAlbumsMatch_ReturnsOkWithEmptyList()
        {
            // Arrange
            const string pattern = "^portrait";

            _albumService.Setup(service => service.GetByNamePattern(pattern)).ReturnsAsync([]);

            // Act
            var result = await _controller.MatchNames(pattern);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumMatchDto>>().Subject;
            dtos.Should().BeEmpty();
        }

        [Fact]
        public async Task MatchNames_WhenServiceRejectsPattern_ReturnsBadRequestWithExceptionMessage()
        {
            // Arrange
            const string pattern = "[";
            const string errorMessage = "Invalid regular expression.";

            _albumService.Setup(service => service.GetByNamePattern(pattern)).ThrowsAsync(new ArgumentException(errorMessage, nameof(pattern)));

            // Act
            var result = await _controller.MatchNames(pattern);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be($"Invalid regular expression. (Parameter '{nameof(pattern)}')");
            _albumService.Verify(service => service.GetByNamePattern(pattern), Times.Once);
        }

        [Fact]
        public async Task Update_WhenItemsAreEmpty_ReturnsBadRequestWithoutBeginningOperation()
        {
            // Arrange
            var request = new BulkUpdateAlbumRequest(new BulkUpdateAlbumOptions(), []);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("At least one album is required.");

            _albumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task Update_WhenRequestContainsDuplicateIds_ReturnsBadRequestWithoutBeginningOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [
                    new BulkUpdateAlbumItem(albumId, "Fashion Milano", null),
            new BulkUpdateAlbumItem(albumId, null, "New description")
                ]);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("The request contains duplicate album ids.");

            _albumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task Update_WhenErrorStrategyIsNotSupported_ReturnsBadRequestWithoutBeginningOperation()
        {
            // Arrange
            var options = new BulkUpdateAlbumOptions((BulkErrorStrategy)999);
            var request = new BulkUpdateAlbumRequest(
                options,
                [new BulkUpdateAlbumItem(Guid.NewGuid(), "Fashion", null)]);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("The requested error strategy is not supported.");

            _albumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", "   ")]
        public async Task Update_WhenItemHasNoFieldsToUpdate_AddsWarningWithoutBeginningOperation(string? name, string? description)
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [new BulkUpdateAlbumItem(albumId, name, description)]);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Should().BeEmpty();
            response.Warnings.Should().ContainSingle().Which.Should().Be(
                new BulkUpdateAlbumWarning(albumId, "At least one field must be specified."));

            _albumService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task Update_WhenOnlyNameIsSpecified_UpdatesNameAndCompletesOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Fashion Milano", Path = "Fashion" };
            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [new BulkUpdateAlbumItem(albumId, "  Fashion Milano  ", null)]);

            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Milano")).ReturnsAsync(album);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Should().ContainSingle().Which.Should().BeEquivalentTo(new { Id = albumId, Name = "Fashion Milano" });
            response.Warnings.Should().BeEmpty();

            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Milano"), Times.Once);
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

            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [new BulkUpdateAlbumItem(albumId, null, "  Fashion photography  ")]);

            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateDescription(albumId, "Fashion photography")).ReturnsAsync(album);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Id = albumId,
                Name = "Fashion",
                Description = "Fashion photography"
            });

            response.Warnings.Should().BeEmpty();

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
            var nameUpdatedAlbum = new Album { Id = albumId, Name = "Fashion Milano", Path = "Fashion" };
            var updatedAlbum = new Album
            {
                Id = albumId,
                Name = "Fashion Milano",
                Description = "New description",
                Path = "Fashion"
            };

            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [new BulkUpdateAlbumItem(albumId, "  Fashion Milano  ", "  New description  ")]);

            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Milano")).ReturnsAsync(nameUpdatedAlbum);
            _albumService.Setup(service => service.UpdateDescription(albumId, "New description")).ReturnsAsync(updatedAlbum);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Id = albumId,
                Name = "Fashion Milano",
                Description = "New description"
            });

            response.Warnings.Should().BeEmpty();

            _albumService.Verify(service => service.UpdateName(albumId, "Fashion Milano"), Times.Once);
            _albumService.Verify(service => service.UpdateDescription(albumId, "New description"), Times.Once);
            operation.Verify(value => value.Complete(), Times.Once);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenAlbumDoesNotExist_AddsWarningWithoutCompletingOperation()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [new BulkUpdateAlbumItem(albumId, "Fashion Milano", null)]);

            var operation = SetupOperation();

            _albumService.Setup(service => service.UpdateName(albumId, "Fashion Milano"))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Should().BeEmpty();
            response.Warnings.Should().ContainSingle().Which.Should().Be(
                new BulkUpdateAlbumWarning(albumId, "Album not found."));

            operation.Verify(value => value.Complete(), Times.Never);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenSomeItemsFail_ContinuesUpdatingRemainingItems()
        {
            // Arrange
            var firstId = Guid.NewGuid();
            var missingId = Guid.NewGuid();
            var thirdId = Guid.NewGuid();

            var request = new BulkUpdateAlbumRequest(
                new BulkUpdateAlbumOptions(),
                [
                    new BulkUpdateAlbumItem(firstId, "First updated", null),
            new BulkUpdateAlbumItem(missingId, "Missing", null),
            new BulkUpdateAlbumItem(thirdId, null, "Third description")
                ]);

            var firstOperation = new Mock<IApplicationOperation>();
            var missingOperation = new Mock<IApplicationOperation>();
            var thirdOperation = new Mock<IApplicationOperation>();

            _albumService.SetupSequence(service => service.BeginOperation())
                .ReturnsAsync(firstOperation.Object)
                .ReturnsAsync(missingOperation.Object)
                .ReturnsAsync(thirdOperation.Object);

            var firstAlbum = new Album { Id = firstId, Name = "First updated", Path = "First" };
            var thirdAlbum = new Album
            {
                Id = thirdId,
                Name = "Third",
                Description = "Third description",
                Path = "Third"
            };

            _albumService.Setup(service => service.UpdateName(firstId, "First updated")).ReturnsAsync(firstAlbum);
            _albumService.Setup(service => service.UpdateName(missingId, "Missing")).ThrowsAsync(new KeyNotFoundException());
            _albumService.Setup(service => service.UpdateDescription(thirdId, "Third description")).ReturnsAsync(thirdAlbum);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Select(item => item.Id).Should().BeEquivalentTo([firstId, thirdId]);
            response.Warnings.Should().ContainSingle().Which.Should().Be(
                new BulkUpdateAlbumWarning(missingId, "Album not found."));

            firstOperation.Verify(value => value.Complete(), Times.Once);
            missingOperation.Verify(value => value.Complete(), Times.Never);
            thirdOperation.Verify(value => value.Complete(), Times.Once);

            firstOperation.Verify(value => value.DisposeAsync(), Times.Once);
            missingOperation.Verify(value => value.DisposeAsync(), Times.Once);
            thirdOperation.Verify(value => value.DisposeAsync(), Times.Once);
        }
    }
}
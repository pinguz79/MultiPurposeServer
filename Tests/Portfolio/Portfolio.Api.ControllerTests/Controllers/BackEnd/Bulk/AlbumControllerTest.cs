using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Portfolio.Api.Controllers.BackEnd.Bulk;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Models;
using Portfolio.Contracts.Bulk.Requests;
using Portfolio.Contracts.Bulk.Responses;
using Portfolio.Contracts.Responses;
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
        public async Task UpdateNames_WhenItemsAreEmpty_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            var request = new BulkUpdateAlbumNameRequest([]);

            // Act
            var result = await _controller.UpdateNames(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("At least one album is required.");
            _albumService.Verify(service => service.BulkUpdateNames(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateNames_WhenAnItemHasInvalidName_ReturnsBadRequestWithoutCallingService(string? newName)
        {
            // Arrange
            var request = new BulkUpdateAlbumNameRequest([new BulkUpdateAlbumNameItem(Guid.NewGuid(), newName!)]);

            // Act
            var result = await _controller.UpdateNames(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Every album must have a valid new name.");
            _albumService.Verify(service => service.BulkUpdateNames(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNames_WhenRequestContainsDuplicateIds_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new BulkUpdateAlbumNameRequest(
            [
                new BulkUpdateAlbumNameItem(albumId, "Fashion Milano"),
                new BulkUpdateAlbumNameItem(albumId, "Fashion Roma")
            ]);

            // Act
            var result = await _controller.UpdateNames(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("The request contains duplicate album ids.");
            _albumService.Verify(service => service.BulkUpdateNames(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNames_WhenOneOrMoreAlbumsDoNotExist_ReturnsNotFound()
        {
            // Arrange
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var request = new BulkUpdateAlbumNameRequest(
            [
                new BulkUpdateAlbumNameItem(firstId, "Fashion Milano"),
                new BulkUpdateAlbumNameItem(secondId, "Fashion Roma")
            ]);

            _albumService.Setup(service => service.BulkUpdateNames(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>())).ReturnsAsync((List<Album>?)null);

            // Act
            var result = await _controller.UpdateNames(request);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().Be("One or more albums do not exist.");

            _albumService.Verify(service => service.BulkUpdateNames(It.Is<IReadOnlyCollection<BulkUpdateItem<string>>>(items =>
                items.Count == 2 &&
                items.Any(item => item.Id == firstId && item.Value == "Fashion Milano") &&
                items.Any(item => item.Id == secondId && item.Value == "Fashion Roma"))), Times.Once);
        }

        [Fact]
        public async Task UpdateNames_WhenRequestIsValid_MapsItemsAndReturnsUpdatedAlbums()
        {
            // Arrange
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var request = new BulkUpdateAlbumNameRequest(
            [
                new BulkUpdateAlbumNameItem(firstId, "Fashion Milano"),
                new BulkUpdateAlbumNameItem(secondId, "Glamour Studio")
            ]);

            var albums = new List<Album>
            {
                new() { Id = firstId, Name = "Fashion Milano", Path = "Fashion" },
                new() { Id = secondId, Name = "Glamour Studio", Path = "Glamour" }
            };

            _albumService.Setup(service => service.BulkUpdateNames(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>())).ReturnsAsync(albums);

            // Act
            var result = await _controller.UpdateNames(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Select(dto => new { dto.Id, dto.Name }).Should().BeEquivalentTo(
            [
                new { Id = firstId, Name = "Fashion Milano" },
                new { Id = secondId, Name = "Glamour Studio" }
            ]);

            _albumService.Verify(service => service.BulkUpdateNames(It.Is<IReadOnlyCollection<BulkUpdateItem<string>>>(items =>
                items.Count == 2 &&
                items.Any(item => item.Id == firstId && item.Value == "Fashion Milano") &&
                items.Any(item => item.Id == secondId && item.Value == "Glamour Studio"))), Times.Once);
        }

        [Fact]
        public async Task UpdateNames_WhenRequestIsValid_PreservesNamesPassedByController()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var request = new BulkUpdateAlbumNameRequest([new BulkUpdateAlbumNameItem(albumId, "  Fashion Milano  ")]);
            var albums = new List<Album> { new() { Id = albumId, Name = "Fashion Milano", Path = "Fashion" } };

            _albumService.Setup(service => service.BulkUpdateNames(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>())).ReturnsAsync(albums);

            // Act
            await _controller.UpdateNames(request);

            // Assert
            _albumService.Verify(service => service.BulkUpdateNames(It.Is<IReadOnlyCollection<BulkUpdateItem<string>>>(items =>
                items.Count == 1 && items.Single().Id == albumId && items.Single().Value == "  Fashion Milano  ")), Times.Once);
        }
    }
}
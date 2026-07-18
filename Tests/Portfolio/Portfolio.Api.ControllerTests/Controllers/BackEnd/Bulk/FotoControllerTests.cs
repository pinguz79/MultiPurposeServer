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
    public class FotoControllerTests
    {
        private readonly Mock<IFotoService> _fotoService;
        private readonly FotoController _controller;

        public FotoControllerTests()
        {
            _fotoService = new Mock<IFotoService>();
            var logger = new Mock<ILogger<FotoController>>();
            _controller = new FotoController(_fotoService.Object, logger.Object);
        }

        [Fact]
        public async Task MissingDescriptions_WhenPhotosExist_ReturnsOkWithMappedDtos()
        {
            // Arrange
            var photos = new List<Foto>
            {
                CreatePhoto("Fashion", "Portrait_001.jpg"),
                CreatePhoto("Glamour/Studio", "Portrait_002.jpg")
            };

            _fotoService.Setup(service => service.GetMissingDescriptions()).ReturnsAsync(photos);

            // Act
            var result = await _controller.MissingDescriptions();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<FotoMissingDescriptionsDto>>().Subject;

            dtos.Should().HaveCount(2);
            dtos.Select(dto => dto.Id).Should().BeEquivalentTo(photos.Select(photo => photo.Id));
            dtos.Select(dto => dto.FileName).Should().BeEquivalentTo(photos.Select(photo => photo.FileName));
            dtos.Select(dto => dto.AlbumName).Should().BeEquivalentTo(photos.Select(photo => photo.Album.FullName));
            dtos.Select(dto => dto.PhotoName).Should().BeEquivalentTo(photos.Select(photo => photo.PhotoName));

            _fotoService.Verify(service => service.GetMissingDescriptions(), Times.Once);
        }

        [Fact]
        public async Task MissingDescriptions_WhenNoPhotosExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            _fotoService.Setup(service => service.GetMissingDescriptions()).ReturnsAsync([]);

            // Act
            var result = await _controller.MissingDescriptions();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<FotoMissingDescriptionsDto>>().Subject;

            dtos.Should().BeEmpty();
            _fotoService.Verify(service => service.GetMissingDescriptions(), Times.Once);
        }

        [Fact]
        public async Task MissingDescriptions_WhenServiceThrowsArgumentException_ReturnsBadRequestWithMessage()
        {
            // Arrange
            const string errorMessage = "Invalid photo data.";

            _fotoService.Setup(service => service.GetMissingDescriptions()).ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _controller.MissingDescriptions();

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be(errorMessage);

            _fotoService.Verify(service => service.GetMissingDescriptions(), Times.Once);
        }

        [Fact]
        public async Task UpdateDescriptions_WhenItemsAreEmpty_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            var request = new BulkUpdateFotoDescriptionRequest([]);

            // Act
            var result = await _controller.UpdateDescriptions(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("At least one photo is required.");

            _fotoService.Verify(service => service.BulkUpdateDescriptions(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateDescriptions_WhenAnItemHasInvalidDescription_ReturnsBadRequestWithoutCallingService(string? description)
        {
            // Arrange
            var request = new BulkUpdateFotoDescriptionRequest([new BulkUpdateFotoDescriptionItem(Guid.NewGuid(), description!)]);

            // Act
            var result = await _controller.UpdateDescriptions(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Every photo must have a valid new description.");

            _fotoService.Verify(service => service.BulkUpdateDescriptions(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDescriptions_WhenRequestContainsDuplicateIds_ReturnsBadRequestWithoutCallingService()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var request = new BulkUpdateFotoDescriptionRequest(
            [
                new BulkUpdateFotoDescriptionItem(photoId, "First description"),
                new BulkUpdateFotoDescriptionItem(photoId, "Second description")
            ]);

            // Act
            var result = await _controller.UpdateDescriptions(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("The request contains duplicate photo ids.");

            _fotoService.Verify(service => service.BulkUpdateDescriptions(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDescriptions_WhenOneOrMorePhotosDoNotExist_ReturnsNotFound()
        {
            // Arrange
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var request = new BulkUpdateFotoDescriptionRequest(
            [
                new BulkUpdateFotoDescriptionItem(firstId, "First description"),
                new BulkUpdateFotoDescriptionItem(secondId, "Second description")
            ]);

            _fotoService.Setup(service => service.BulkUpdateDescriptions(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>())).ReturnsAsync((List<Foto>?)null);

            // Act
            var result = await _controller.UpdateDescriptions(request);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().Be("One or more photos do not exist.");

            _fotoService.Verify(service => service.BulkUpdateDescriptions(It.Is<IReadOnlyCollection<BulkUpdateItem<string>>>(items =>
                items.Count == 2 &&
                items.Any(item => item.Id == firstId && item.Value == "First description") &&
                items.Any(item => item.Id == secondId && item.Value == "Second description"))), Times.Once);
        }

        [Fact]
        public async Task UpdateDescriptions_WhenRequestIsValid_ReturnsOkWithMappedDtos()
        {
            // Arrange
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var request = new BulkUpdateFotoDescriptionRequest(
            [
                new BulkUpdateFotoDescriptionItem(firstId, "First description"),
                new BulkUpdateFotoDescriptionItem(secondId, "Second description")
            ]);

            var firstPhoto = CreatePhoto("Fashion", "Portrait_001.jpg", firstId);
            var secondPhoto = CreatePhoto("Glamour", "Portrait_002.jpg", secondId);
            firstPhoto.Description = "First description";
            secondPhoto.Description = "Second description";

            var photos = new List<Foto> { firstPhoto, secondPhoto };

            _fotoService.Setup(service => service.BulkUpdateDescriptions(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>())).ReturnsAsync(photos);

            // Act
            var result = await _controller.UpdateDescriptions(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<PhotoDto>>().Subject;

            dtos.Select(dto => dto.Id).Should().BeEquivalentTo([firstId, secondId]);
            dtos.Select(dto => dto.Name).Should().BeEquivalentTo(photos.Select(photo => photo.PhotoName));
            dtos.Select(dto => dto.Alt).Should().BeEquivalentTo(photos.Select(photo => photo.AltText));

            _fotoService.Verify(service => service.BulkUpdateDescriptions(It.Is<IReadOnlyCollection<BulkUpdateItem<string>>>(items =>
                items.Count == 2 &&
                items.Any(item => item.Id == firstId && item.Value == "First description") &&
                items.Any(item => item.Id == secondId && item.Value == "Second description"))), Times.Once);
        }

        [Fact]
        public async Task UpdateDescriptions_WhenRequestIsValid_PreservesDescriptionsPassedByController()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var request = new BulkUpdateFotoDescriptionRequest([new BulkUpdateFotoDescriptionItem(photoId, "  Description with spaces  ")]);
            var photo = CreatePhoto("Fashion", "Portrait_001.jpg", photoId);

            _fotoService.Setup(service => service.BulkUpdateDescriptions(It.IsAny<IReadOnlyCollection<BulkUpdateItem<string>>>())).ReturnsAsync([photo]);

            // Act
            await _controller.UpdateDescriptions(request);

            // Assert
            _fotoService.Verify(service => service.BulkUpdateDescriptions(It.Is<IReadOnlyCollection<BulkUpdateItem<string>>>(items =>
                items.Count == 1 &&
                items.Single().Id == photoId &&
                items.Single().Value == "  Description with spaces  ")), Times.Once);
        }

        private static Foto CreatePhoto(string albumPath, string fileName, Guid? photoId = null)
        {
            var album = CreateAlbumHierarchy(albumPath);

            return new Foto
            {
                Id = photoId ?? Guid.NewGuid(),
                AlbumId = album.Id,
                Album = album,
                FileName = fileName
            };
        }

        private static Album CreateAlbumHierarchy(string albumPath)
        {
            Album? parent = null;

            foreach (var segment in albumPath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                parent = new Album
                {
                    Id = Guid.NewGuid(),
                    Name = segment,
                    Path = segment,
                    ParentId = parent?.Id,
                    Parent = parent
                };
            }

            return parent ?? new Album { Id = Guid.NewGuid(), Name = string.Empty, Path = string.Empty };
        }
    }
}
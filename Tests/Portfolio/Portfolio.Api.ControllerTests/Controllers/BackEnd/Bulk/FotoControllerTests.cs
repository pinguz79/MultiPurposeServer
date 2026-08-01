using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Enums;
using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.BackEnd.Bulk;
using Portfolio.Api.Services;
using Portfolio.Contracts.Bulk;
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

        private Mock<IApplicationOperation> SetupOperation()
        {
            var operation = new Mock<IApplicationOperation>();
            _fotoService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);
            return operation;
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
        public async Task Update_WhenItemsAreEmpty_ReturnsBadRequestWithoutBeginningOperation()
        {
            // Arrange
            var request = new BulkUpdateFotoRequest(new BulkOptions(), []);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("At least one album is required.");

            _fotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task Update_WhenRequestContainsDuplicateIds_ReturnsBadRequestWithoutBeginningOperation()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var request = new BulkUpdateFotoRequest(
                new BulkOptions(),
                [
                    new BulkUpdateFotoItem(photoId, "Fashion Milano"),
                    new BulkUpdateFotoItem(photoId, "New description")
                ]);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("The request contains duplicate photo ids.");

            _fotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task Update_WhenErrorStrategyIsNotSupported_ReturnsBadRequestWithoutBeginningOperation()
        {
            // Arrange
            var options = new BulkOptions((BulkErrorStrategy)999);
            var request = new BulkUpdateFotoRequest(
                options,
                [new BulkUpdateFotoItem(Guid.NewGuid(), "Fashion")]);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("The requested error strategy is not supported.");

            _fotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Update_WhenItemHasNoFieldsToUpdate_AddsWarningWithoutBeginningOperation(string? description)
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var request = new BulkUpdateFotoRequest(
                new BulkOptions(),
                [new BulkUpdateFotoItem(photoId, description)]);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateFotoResponse>().Subject;

            response.UpdatedItems.Should().BeEmpty();
            response.Warnings.Should().ContainSingle().Which.Should().Be(
                new BulkUpdateFotoWarning(photoId, "At least one field must be specified."));

            _fotoService.Verify(service => service.BeginOperation(), Times.Never);
        }

        [Fact]
        public async Task Update_WhenDescriptionIsSpecified_UpdatesFieldAndCompletesOperation()
        {
            // Arrange
            var fotoId = Guid.NewGuid();
            var descriptionUpdatedFoto = new Foto { Id = fotoId, Description = "Description" };
            var updatedFoto = new Foto
            {
                Id = fotoId,
                Description = "New description",
            };

            var request = new BulkUpdateFotoRequest(
                new BulkOptions(),
                [new BulkUpdateFotoItem(fotoId, "  New description  ")]);

            var operation = SetupOperation();

            _fotoService.Setup(service => service.UpdateDescription(fotoId, "New description")).ReturnsAsync(updatedFoto);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateFotoResponse>().Subject;

            response.UpdatedItems.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Id = fotoId,
                Description = "New description"
            });

            response.Warnings.Should().BeEmpty();

            _fotoService.Verify(service => service.UpdateDescription(fotoId, "New description"), Times.Once);
            operation.Verify(value => value.Complete(), Times.Once);
            operation.Verify(value => value.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenPhotoDoesNotExist_AddsWarningWithoutCompletingOperation()
        {
            // Arrange
            var fotoId = Guid.NewGuid();
            var request = new BulkUpdateFotoRequest(
                new BulkOptions(),
                [new BulkUpdateFotoItem(fotoId, null)]);

            var operation = SetupOperation();

            _fotoService.Setup(service => service.UpdateDescription(fotoId, "Fashion Milano"))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateAlbumResponse>().Subject;

            response.UpdatedItems.Should().BeEmpty();
            response.Warnings.Should().ContainSingle().Which.Should().Be(
                new BulkUpdateFotoWarning(fotoId, "Foto not found."));

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

            var request = new BulkUpdateFotoRequest(
                new BulkOptions(),
                [
                    new BulkUpdateFotoItem(firstId, "First updated"),
                    new BulkUpdateFotoItem(missingId, "Missing"),
                    new BulkUpdateFotoItem(thirdId, "Third description")
                ]);

            var firstOperation = new Mock<IApplicationOperation>();
            var missingOperation = new Mock<IApplicationOperation>();
            var thirdOperation = new Mock<IApplicationOperation>();

            _fotoService.SetupSequence(service => service.BeginOperation())
                .ReturnsAsync(firstOperation.Object)
                .ReturnsAsync(missingOperation.Object)
                .ReturnsAsync(thirdOperation.Object);

            var firstFoto = new Foto { Id = firstId, Description = "First updated" };
            var thirdFoto = new Foto { Id = thirdId, Description = "Third description" };

            _fotoService.Setup(service => service.UpdateDescription(firstId, "First updated")).ReturnsAsync(firstFoto);
            _fotoService.Setup(service => service.UpdateDescription(missingId, "Missing")).ThrowsAsync(new KeyNotFoundException());
            _fotoService.Setup(service => service.UpdateDescription(thirdId, "Third description")).ReturnsAsync(thirdFoto);

            // Act
            var result = await _controller.Update(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<BulkUpdateFotoResponse>().Subject;

            response.UpdatedItems.Select(item => item.Id).Should().BeEquivalentTo([firstId, thirdId]);
            response.Warnings.Should().ContainSingle().Which.Should().Be(
                new BulkUpdateFotoWarning(missingId, "Foto not found."));

            firstOperation.Verify(value => value.Complete(), Times.Once);
            missingOperation.Verify(value => value.Complete(), Times.Never);
            thirdOperation.Verify(value => value.Complete(), Times.Once);

            firstOperation.Verify(value => value.DisposeAsync(), Times.Once);
            missingOperation.Verify(value => value.DisposeAsync(), Times.Once);
            thirdOperation.Verify(value => value.DisposeAsync(), Times.Once);
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
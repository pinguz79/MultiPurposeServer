using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.BackEnd;
using Portfolio.Contracts.Requests;
using Portfolio.Contracts.Responses;
using Portfolio.Data.Enums;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Controllers.BackEnd
{
    public class FotoControllerTests
    {
        private readonly Mock<IFotoService> _fotoService;
        private readonly Mock<ICacheService> _cacheService;
        private readonly FotoController _controller;

        public FotoControllerTests()
        {
            _fotoService = new Mock<IFotoService>();
            _cacheService = new Mock<ICacheService>();

            _controller = new FotoController(_fotoService.Object, _cacheService.Object);
        }

        #region Get

        [Fact]
        public async Task GetList_WhenPhotosExist_ReturnsOkWithMappedDtos()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            var photos = new List<Foto>
            {
                CreatePhoto("Photo_001.jpg"),
                CreatePhoto("Photo_002.jpg")
            };

            _fotoService.Setup(service => service.GetByAlbum(albumId)).ReturnsAsync(photos);

            // Act
            var result = await _controller.GetList(albumId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<PhotoDto>>().Subject;

            dtos.Should().HaveCount(2);

            dtos.Select(dto => dto.Id).Should().BeEquivalentTo(photos.Select(photo => photo.Id));
            dtos.Select(dto => dto.Name).Should().BeEquivalentTo(photos.Select(photo => photo.PhotoName));
            dtos.Select(dto => dto.Alt).Should().BeEquivalentTo(photos.Select(photo => photo.AltText));

            _fotoService.Verify(service => service.GetByAlbum(albumId), Times.Once);
        }

        [Fact]
        public async Task GetList_WhenAlbumHasNoPhotos_ReturnsEmptyList()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            _fotoService.Setup(service => service.GetByAlbum(albumId)).ReturnsAsync([]);

            // Act
            var result = await _controller.GetList(albumId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<PhotoDto>>().Subject;

            dtos.Should().BeEmpty();
        }

        [Fact]
        public async Task Get_WhenPhotoExists_ReturnsMappedDto()
        {
            // Arrange
            var photo = CreatePhoto("Portrait.jpg");

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);

            // Act
            var result = await _controller.Get(photo.Id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<PhotoDto>().Subject;

            dto.Id.Should().Be(photo.Id);
            dto.Name.Should().Be(photo.PhotoName);
            dto.Alt.Should().Be(photo.AltText);
            dto.SelectionCode.Should().Be(photo.SelectionCode);

            _fotoService.Verify(service => service.GetById(photo.Id), Times.Once);
        }

        [Fact]
        public async Task Get_WhenPhotoDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            _fotoService.Setup(service => service.GetById(photoId)).ReturnsAsync((Foto?)null);

            // Act
            var result = await _controller.Get(photoId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();

            _fotoService.Verify(service => service.GetById(photoId), Times.Once);
        }
        #endregion

        #region Update

        [Fact]
        public async Task Update_WhenPhotoExists_ReturnsUpdatedDto()
        {
            // Arrange
            var photo = CreatePhoto("Portrait.jpg");
            photo.Description = "Updated description";

            var request = new UpdatePhotoRequest("Updated description");
            var operation = SetupOperation();

            _fotoService.Setup(service => service.UpdateDescription(photo.Id, "Updated description")).ReturnsAsync(photo);

            // Act
            var result = await _controller.Update(photo.Id, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeOfType<PhotoDto>().Subject;

            dto.Id.Should().Be(photo.Id);
            dto.Name.Should().Be(photo.PhotoName);
            dto.Alt.Should().Be(photo.AltText);

            _fotoService.Verify(service => service.BeginOperation(), Times.Once);
            _fotoService.Verify(service => service.UpdateDescription(photo.Id, "Updated description"), Times.Once);
            operation.Verify(o => o.Complete(), Times.Once);
        }
        [Fact]
        public async Task Update_WhenContentRatingIsSpecified_UpdatesRatingAndClearsAffectedCaches()
        {
            // Arrange
            var photo = CreatePhoto("Portrait.jpg");
            photo.ContentRating = PhotoContentRating.Restricted;
            var request = new UpdatePhotoRequest(null, PhotoContentRating.Restricted);
            var operation = SetupOperation();
            _fotoService.Setup(service => service.UpdateContentRating(photo.Id, PhotoContentRating.Restricted)).ReturnsAsync(photo);
            _cacheService.Setup(service => service.Clear(true, false, true)).ReturnsAsync(new CacheClearOperationResult());

            // Act
            var result = await _controller.Update(photo.Id, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeOfType<PhotoDto>().Which.ContentRating.Should().Be(PhotoContentRating.Restricted);
            _fotoService.Verify(service => service.UpdateContentRating(photo.Id, PhotoContentRating.Restricted), Times.Once);
            _cacheService.Verify(service => service.Clear(true, false, true), Times.Once);
            operation.Verify(value => value.Complete(), Times.Once);
        }

        #endregion

        #region Helper

        private static Foto CreatePhoto(string fileName)
        {
            return new Foto
            {
                Id = Guid.NewGuid(),
                AlbumId = Guid.NewGuid(),
                FileName = fileName,
                Description = "Description",
            };
        }

        private Mock<IApplicationOperation> SetupOperation()
        {
            var operation = new Mock<IApplicationOperation>();

            _fotoService.Setup(service => service.BeginOperation()).ReturnsAsync(operation.Object);

            return operation;
        }
        #endregion

    }
}

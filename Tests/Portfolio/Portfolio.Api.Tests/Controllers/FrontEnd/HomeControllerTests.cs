using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MultiPurposeServer.Shared.Contracts.Responses;
using MultiPurposeServer.Shared.Models;

using Portfolio.Api.Application.Services;
using Portfolio.Api.Controllers.FrontEnd;
using Portfolio.Contracts.Responses;
using Portfolio.DataModel.Models;

namespace Portfolio.Api.Tests.Controllers.FrontEnd
{
    public class HomeControllerTests
    {
        private readonly Mock<IAlbumService> _albumService;
        private readonly Mock<IFotoService> _fotoService;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _albumService = new Mock<IAlbumService>();
            _fotoService = new Mock<IFotoService>();
            _controller = new HomeController(_albumService.Object, _fotoService.Object);
        }

        #region GetAlbums

        [Fact]
        public async Task GetAlbums_WhenRootAlbumsExist_ReturnsOkWithMappedDtos()
        {
            // Arrange
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null },
                new() { Id = Guid.NewGuid(), Name = "Glamour", Path = "Glamour", ParentId = null }
            };

            _albumService.Setup(service => service.GetAlbums(null)).ReturnsAsync(albums);

            // Act
            var result = await _controller.GetAlbums();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Select(dto => new { dto.Id, dto.Name }).Should().BeEquivalentTo(
            [
                new { albums[0].Id, albums[0].Name },
                new { albums[1].Id, albums[1].Name }
            ]);

            _albumService.Verify(service => service.GetAlbums(null), Times.Once);
        }

        [Fact]
        public async Task GetAlbums_WhenParentIdIsProvided_ReturnsChildrenForRequestedAlbum()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano", ParentId = parentId }
            };

            _albumService.Setup(service => service.GetAlbums(parentId)).ReturnsAsync(albums);

            // Act
            var result = await _controller.GetAlbums(parentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Should().ContainSingle();
            dtos.Single().Should().BeEquivalentTo(new { albums[0].Id, albums[0].Name });

            _albumService.Verify(service => service.GetAlbums(parentId), Times.Once);
        }

        [Fact]
        public async Task GetAlbums_WhenNoAlbumsExist_ReturnsOkWithEmptyList()
        {
            // Arrange
            _albumService.Setup(service => service.GetAlbums(null)).ReturnsAsync([]);

            // Act
            var result = await _controller.GetAlbums();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtos = okResult.Value.Should().BeAssignableTo<List<AlbumDto>>().Subject;

            dtos.Should().BeEmpty();
        }

        #endregion

        #region GetAlbumPhotos

        [Fact]
        public async Task GetAlbumPhotos_WhenRequestIsValid_ReturnsMappedPage()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var photos = new List<Foto>
            {
                CreatePhoto("Portrait_001.jpg"),
                CreatePhoto("Portrait_002.jpg")
            };
            var pagedResult = new PagedResult<Foto>(photos, 5);

            _fotoService.Setup(service => service.GetByAlbumId(albumId, 2, 24)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAlbumPhotos(albumId, 2, 24);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PageDto<PhotoDto>>().Subject;

            response.Items.Should().HaveCount(2);
            response.Items.Select(photo => photo.Id).Should().BeEquivalentTo(photos.Select(photo => photo.Id));
            response.Items.Select(photo => photo.Name).Should().BeEquivalentTo(photos.Select(photo => photo.PhotoName));
            response.Page.Should().Be(2);
            response.PageSize.Should().Be(24);
            response.TotalItems.Should().Be(5);

            _fotoService.Verify(service => service.GetByAlbumId(albumId, 2, 24), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-20)]
        public async Task GetAlbumPhotos_WhenPageIsLessThanOne_UsesFirstPage(int requestedPage)
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var pagedResult = new PagedResult<Foto>([], 0);

            _fotoService.Setup(service => service.GetByAlbumId(albumId, 1, 12)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAlbumPhotos(albumId, requestedPage, 12);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PageDto<PhotoDto>>().Subject;

            response.Page.Should().Be(1);
            _fotoService.Verify(service => service.GetByAlbumId(albumId, 1, 12), Times.Once);
        }

        [Theory]
        [InlineData(12)]
        [InlineData(24)]
        [InlineData(48)]
        public async Task GetAlbumPhotos_WhenPageSizeIsSupported_PreservesPageSize(int pageSize)
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var pagedResult = new PagedResult<Foto>([], 0);

            _fotoService.Setup(service => service.GetByAlbumId(albumId, 1, pageSize)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAlbumPhotos(albumId, 1, pageSize);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PageDto<PhotoDto>>().Subject;

            response.PageSize.Should().Be(pageSize);
            _fotoService.Verify(service => service.GetByAlbumId(albumId, 1, pageSize), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(13)]
        [InlineData(25)]
        [InlineData(100)]
        public async Task GetAlbumPhotos_WhenPageSizeIsUnsupported_UsesDefaultPageSize(int requestedPageSize)
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var pagedResult = new PagedResult<Foto>([], 0);

            _fotoService.Setup(service => service.GetByAlbumId(albumId, 1, 12)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAlbumPhotos(albumId, 1, requestedPageSize);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PageDto<PhotoDto>>().Subject;

            response.PageSize.Should().Be(12);
            _fotoService.Verify(service => service.GetByAlbumId(albumId, 1, 12), Times.Once);
        }

        [Fact]
        public async Task GetAlbumPhotos_WhenPageContainsNoPhotos_ReturnsEmptyPageWithTotalItems()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var pagedResult = new PagedResult<Foto>([], 25);

            _fotoService.Setup(service => service.GetByAlbumId(albumId, 3, 12)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAlbumPhotos(albumId, 3, 12);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<PageDto<PhotoDto>>().Subject;

            response.Items.Should().BeEmpty();
            response.Page.Should().Be(3);
            response.PageSize.Should().Be(12);
            response.TotalItems.Should().Be(25);
        }

        #endregion

        private static Foto CreatePhoto(string fileName)
        {
            return new Foto
            {
                Id = Guid.NewGuid(),
                AlbumId = Guid.NewGuid(),
                FileName = fileName,
                Description = "Description"
            };
        }

    }
}

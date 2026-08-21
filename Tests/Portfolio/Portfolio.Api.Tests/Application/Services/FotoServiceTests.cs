using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Models;

using MultiPurposeServer.Shared.Persistence.Operations;
using MultiPurposeServer.Shared.Persistence.Transactions;

using Portfolio.Api.Application.Services;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.DataModel.Enums;
using Portfolio.DataModel.Models;

namespace Portfolio.Api.Tests.Application.Services
{
    public class FotoServiceTests
    {
        private readonly Mock<IFotoRepository> _repository;
        private readonly FotoService _service;

        public FotoServiceTests()
        {
            _repository = new Mock<IFotoRepository>();
            _service = new FotoService(_repository.Object);
        }

        #region Get

        [Fact]
        public async Task GetByAlbum_WhenRepositoryReturnsPhotos_ReturnsRepositoryResult()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var photos = new List<Foto>
            {
                new() { Id = Guid.NewGuid(), AlbumId = albumId, FileName = "Photo_001.jpg" },
                new() { Id = Guid.NewGuid(), AlbumId = albumId, FileName = "Photo_002.jpg" }
            };

            _repository.Setup(repository => repository.GetByAlbum(albumId)).ReturnsAsync(photos);

            // Act
            var result = await _service.GetByAlbum(albumId);

            // Assert
            result.Should().BeSameAs(photos);
            _repository.Verify(repository => repository.GetByAlbum(albumId), Times.Once);
        }

        [Fact]
        public async Task GetByAlbum_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            _repository.Setup(repository => repository.GetByAlbum(albumId)).ReturnsAsync([]);

            // Act
            var result = await _service.GetByAlbum(albumId);

            // Assert
            result.Should().BeEmpty();
            _repository.Verify(repository => repository.GetByAlbum(albumId), Times.Once);
        }

        [Fact]
        public async Task GetByAlbumId_WhenRepositoryReturnsPagedResult_ReturnsRepositoryResult()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var photos = new List<Foto>
            {
                new() { Id = Guid.NewGuid(), AlbumId = albumId, FileName = "Photo_001.jpg" },
                new() { Id = Guid.NewGuid(), AlbumId = albumId, FileName = "Photo_002.jpg" }
            };
            var pagedResult = new PagedResult<Foto>(photos, 5);

            _repository.Setup(repository => repository.GetByAlbumId(albumId, 2, 2)).ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetByAlbumId(albumId, 2, 2);

            // Assert
            result.Should().BeSameAs(pagedResult);
            result.Items.Should().BeEquivalentTo(photos);
            result.TotalItems.Should().Be(5);
            _repository.Verify(repository => repository.GetByAlbumId(albumId, 2, 2), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenPhotoExists_ReturnsRepositoryResult()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var photo = new Foto { Id = photoId, AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg" };

            _repository.Setup(repository => repository.GetById(photoId)).ReturnsAsync(photo);

            // Act
            var result = await _service.GetById(photoId);

            // Assert
            result.Should().BeSameAs(photo);
            _repository.Verify(repository => repository.GetById(photoId), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenPhotoDoesNotExist_ReturnsNull()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            _repository.Setup(repository => repository.GetById(photoId)).ReturnsAsync((Foto?)null);

            // Act
            var result = await _service.GetById(photoId);

            // Assert
            result.Should().BeNull();
            _repository.Verify(repository => repository.GetById(photoId), Times.Once);
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateDescription_WhenPhotoExists_ReturnsRepositoryResult()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            const string description = "Ritratto in studio";
            var photo = new Foto { Id = photoId, AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg", Description = description };

            _repository.Setup(repository => repository.UpdateDescription(photoId, description)).ReturnsAsync(photo);

            // Act
            var result = await _service.UpdateDescription(photoId, description);

            // Assert
            result.Should().BeSameAs(photo);
            _repository.Verify(repository => repository.UpdateDescription(photoId, description), Times.Once);
        }

        [Fact]
        public async Task UpdateDescription_WhenPhotoDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            const string description = "Ritratto in studio";

            _repository.Setup(repository => repository.UpdateDescription(photoId, description)).ThrowsAsync(new KeyNotFoundException());

            // Act
            Func<Task> action = async () => await _service.UpdateDescription(photoId, description);

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>();
            _repository.Verify(repository => repository.UpdateDescription(photoId, description), Times.Once);
        }

        [Fact]
        public async Task UpdateContentRating_WhenCalled_DelegatesToRepository()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var photo = new Foto { Id = photoId, ContentRating = PhotoContentRating.Restricted };
            _repository.Setup(repository => repository.UpdateContentRating(photoId, PhotoContentRating.Restricted)).ReturnsAsync(photo);

            // Act
            var result = await _service.UpdateContentRating(photoId, PhotoContentRating.Restricted);

            // Assert
            result.Should().BeSameAs(photo);
            _repository.Verify(repository => repository.UpdateContentRating(photoId, PhotoContentRating.Restricted), Times.Once);
        }

        [Fact]
        public async Task GetMissingDescriptions_WhenRepositoryReturnsPhotos_ReturnsRepositoryResult()
        {
            // Arrange
            var photos = new List<Foto>
            {
                new() { Id = Guid.NewGuid(), AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg", Description = null },
                new() { Id = Guid.NewGuid(), AlbumId = Guid.NewGuid(), FileName = "Photo_002.jpg", Description = null }
            };

            _repository.Setup(repository => repository.GetMissingDescriptions()).ReturnsAsync(photos);

            // Act
            var result = await _service.GetMissingDescriptions();

            // Assert
            result.Should().BeSameAs(photos);
            _repository.Verify(repository => repository.GetMissingDescriptions(), Times.Once);
        }

        [Fact]
        public async Task GetMissingDescriptions_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _repository.Setup(repository => repository.GetMissingDescriptions()).ReturnsAsync([]);

            // Act
            var result = await _service.GetMissingDescriptions();

            // Assert
            result.Should().BeEmpty();
            _repository.Verify(repository => repository.GetMissingDescriptions(), Times.Once);
        }

        #endregion

        #region Operazioni

        [Fact]
        public async Task BeginOperation_WhenCalled_BeginsRepositoryTransaction()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();

            _repository
                .Setup(repository => repository.BeginTransaction())
                .ReturnsAsync(transaction.Object);

            // Act
            await using var operation = await _service.BeginOperation();

            // Assert
            operation.Should().BeOfType<ApplicationOperation>();
            _repository.Verify(repository => repository.BeginTransaction(), Times.Once);
        }

        [Fact]
        public async Task UpdateDescription_WhenCalled_DelegatesToRepository()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            var album = new Album
            {
                Id = Guid.NewGuid(),
                Name = "Album"
            };

            var foto = new Foto
            {
                Id = photoId,
                AlbumId = album.Id,
                Album = album,
                FileName = "Photo_001.jpg",
                Description = "New description"
            };

            _repository.Setup(repository => repository.UpdateDescription(photoId, "New description")).ReturnsAsync(foto);

            // Act
            var result = await _service.UpdateDescription(photoId, "New description");

            // Assert
            result.Should().BeSameAs(foto);

            _repository.Verify(repository => repository.UpdateDescription(photoId, "New description"), Times.Once);
        }
        #endregion

    }
}

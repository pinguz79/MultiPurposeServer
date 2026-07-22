using FluentAssertions;
using Moq;
using MultiPurposeServer.Shared.Models;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Api.Services;
using Portfolio.Api.Services.Models;
using Portfolio.Data.Models;

namespace Portfolio.Api.ServiceTests.Services
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
        public async Task UpdateDescription_WhenPhotoDoesNotExist_ReturnsNull()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            const string description = "Ritratto in studio";

            _repository.Setup(repository => repository.UpdateDescription(photoId, description)).ReturnsAsync((Foto?)null);

            // Act
            var result = await _service.UpdateDescription(photoId, description);

            // Assert
            result.Should().BeNull();
            _repository.Verify(repository => repository.UpdateDescription(photoId, description), Times.Once);
        }

        [Fact]
        public async Task UpdateDescription_WhenDescriptionIsNull_PassesNullToRepository()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            var photo = new Foto { Id = photoId, AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg", Description = null };

            _repository.Setup(repository => repository.UpdateDescription(photoId, null)).ReturnsAsync(photo);

            // Act
            var result = await _service.UpdateDescription(photoId, null);

            // Assert
            result.Should().BeSameAs(photo);
            _repository.Verify(repository => repository.UpdateDescription(photoId, null), Times.Once);
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

        [Fact]
        public async Task BulkUpdateDescriptions_WhenAllPhotosExist_UpdatesDescriptionsAndSaves()
        {
            // Arrange
            var firstPhotoId = Guid.NewGuid();
            var secondPhotoId = Guid.NewGuid();

            IReadOnlyCollection<BulkUpdateItem<string>> items =
            [
                new(firstPhotoId, "Prima descrizione"),
                new(secondPhotoId, "Seconda descrizione")
            ];

            var photos = new List<Foto>
            {
                new() { Id = firstPhotoId, AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg" },
                new() { Id = secondPhotoId, AlbumId = Guid.NewGuid(), FileName = "Photo_002.jpg" }
            };

            _repository.Setup(repository => repository.GetByIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(photos);
            _repository.Setup(repository => repository.Save()).ReturnsAsync(2);

            // Act
            var result = await _service.BulkUpdateDescriptions(items);

            // Assert
            result.Should().BeSameAs(photos);
            result.Should().SatisfyRespectively(
                first => first.Description.Should().Be("Prima descrizione"),
                second => second.Description.Should().Be("Seconda descrizione"));

            _repository.Verify(repository => repository.GetByIds(It.Is<IEnumerable<Guid>>(ids => ids.ToHashSet().SetEquals(new[] { firstPhotoId, secondPhotoId }))), Times.Once);
            _repository.Verify(repository => repository.Save(), Times.Once);
        }

        [Fact]
        public async Task BulkUpdateDescriptions_WhenDescriptionsHaveOuterSpaces_TrimsDescriptions()
        {
            // Arrange
            var photoId = Guid.NewGuid();
            IReadOnlyCollection<BulkUpdateItem<string>> items = [new(photoId, "  Ritratto in studio  ")];
            var photos = new List<Foto> { new() { Id = photoId, AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg" } };

            _repository.Setup(repository => repository.GetByIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(photos);
            _repository.Setup(repository => repository.Save()).ReturnsAsync(1);

            // Act
            var result = await _service.BulkUpdateDescriptions(items);

            // Assert
            result.Should().ContainSingle().Which.Description.Should().Be("Ritratto in studio");
            _repository.Verify(repository => repository.Save(), Times.Once);
        }

        [Fact]
        public async Task BulkUpdateDescriptions_WhenSomePhotosDoNotExist_ReturnsNullWithoutSaving()
        {
            // Arrange
            var existingPhotoId = Guid.NewGuid();
            var missingPhotoId = Guid.NewGuid();

            IReadOnlyCollection<BulkUpdateItem<string>> items =
            [
                new(existingPhotoId, "Prima descrizione"),
                new(missingPhotoId, "Seconda descrizione")
            ];

            var photos = new List<Foto>
            {
                new() { Id = existingPhotoId, AlbumId = Guid.NewGuid(), FileName = "Photo_001.jpg" }
            };

            _repository.Setup(repository => repository.GetByIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(photos);

            // Act
            var result = await _service.BulkUpdateDescriptions(items);

            // Assert
            result.Should().BeNull();
            photos.Single().Description.Should().BeNull();
            _repository.Verify(repository => repository.GetByIds(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _repository.Verify(repository => repository.Save(), Times.Never);
        }

        [Fact]
        public async Task BulkUpdateDescriptions_WhenItemsAreEmpty_ReturnsEmptyListWithoutCallingRepository()
        {
            // Arrange
            IReadOnlyCollection<BulkUpdateItem<string>> items = [];

            // Act
            var result = await _service.BulkUpdateDescriptions(items);

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
            _repository.Verify(repository => repository.GetByIds(It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _repository.Verify(repository => repository.Save(), Times.Never);
        }
    }
}
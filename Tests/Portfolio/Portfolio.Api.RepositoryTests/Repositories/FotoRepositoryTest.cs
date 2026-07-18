using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Repositories;
using Portfolio.Api.RepositoryTests.Infrastructure;
using Portfolio.Data.Models;

namespace Portfolio.Api.RepositoryTests.Repositories
{
    public class FotoRepositoryTests : RepositoryTestBase
    {
        private readonly FotoRepository _repository;

        public FotoRepositoryTests()
        {
            _repository = new FotoRepository(DbContext);
        }

        [Fact]
        public async Task CreatePhoto_WhenAlbumExists_CreatesPhoto()
        {
            // Arrange
            var album = await CreateAlbum();

            // Act
            var photo = await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");
            var storedPhotos = await DbContext.Foto.ToListAsync();

            // Assert
            photo.Should().BeEquivalentTo(new { AlbumId = album.Id, FileName = "Fashion_001.jpg" });
            photo.Id.Should().NotBeEmpty();
            storedPhotos.Should().ContainSingle(item => item.Id == photo.Id);
        }

        [Fact]
        public async Task CreatePhoto_WhenAlbumDoesNotExist_ThrowsDbUpdateException()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            // Act
            var action = async () => await _repository.CreatePhoto(albumId, "Fashion_001.jpg");

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task CreatePhoto_WhenFileNameAlreadyExistsInSameAlbum_ThrowsDbUpdateException()
        {
            // Arrange
            var album = await CreateAlbum();
            await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");

            // Act
            var action = async () => await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task CreatePhoto_WhenFileNameExistsInDifferentAlbum_CreatesPhoto()
        {
            // Arrange
            var firstAlbum = await CreateAlbum("Fashion", "Fashion");
            var secondAlbum = await CreateAlbum("Glamour", "Glamour");
            await _repository.CreatePhoto(firstAlbum.Id, "Photo_001.jpg");

            // Act
            var photo = await _repository.CreatePhoto(secondAlbum.Id, "Photo_001.jpg");

            // Assert
            photo.Should().BeEquivalentTo(new { AlbumId = secondAlbum.Id, FileName = "Photo_001.jpg" });
        }

        [Fact]
        public async Task GetByAlbum_WhenAlbumContainsPhotos_ReturnsPhotosOrderedByFileName()
        {
            // Arrange
            var album = await CreateAlbum();
            await _repository.CreatePhoto(album.Id, "Fashion_003.jpg");
            await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");
            await _repository.CreatePhoto(album.Id, "Fashion_002.jpg");

            // Act
            var photos = await _repository.GetByAlbum(album.Id);

            // Assert
            photos.Select(photo => photo.FileName).Should().ContainInOrder("Fashion_001.jpg", "Fashion_002.jpg", "Fashion_003.jpg");
        }

        [Fact]
        public async Task GetByAlbum_WhenOtherAlbumsContainPhotos_ReturnsOnlyRequestedAlbumPhotos()
        {
            // Arrange
            var requestedAlbum = await CreateAlbum("Fashion", "Fashion");
            var otherAlbum = await CreateAlbum("Glamour", "Glamour");
            await _repository.CreatePhoto(requestedAlbum.Id, "Fashion_001.jpg");
            await _repository.CreatePhoto(requestedAlbum.Id, "Fashion_002.jpg");
            await _repository.CreatePhoto(otherAlbum.Id, "Glamour_001.jpg");

            // Act
            var photos = await _repository.GetByAlbum(requestedAlbum.Id);

            // Assert
            photos.Select(photo => photo.FileName).Should().BeEquivalentTo(["Fashion_001.jpg", "Fashion_002.jpg"]);
        }

        [Fact]
        public async Task GetByAlbum_WhenAlbumHasNoPhotos_ReturnsEmptyList()
        {
            // Arrange
            var album = await CreateAlbum();

            // Act
            var photos = await _repository.GetByAlbum(album.Id);

            // Assert
            photos.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByAlbumId_WhenRequestingFirstPage_ReturnsCorrectItemsAndTotal()
        {
            // Arrange
            var album = await CreateAlbum();
            await CreatePhotos(album.Id, 5);

            // Act
            var result = await _repository.GetByAlbumId(album.Id, 1, 2);

            // Assert
            result.Items.Select(photo => photo.FileName).Should().ContainInOrder("Photo_001.jpg", "Photo_002.jpg");
            result.TotalItems.Should().Be(5);
        }

        [Fact]
        public async Task GetByAlbumId_WhenRequestingSecondPage_ReturnsCorrectItemsAndTotal()
        {
            // Arrange
            var album = await CreateAlbum();
            await CreatePhotos(album.Id, 5);

            // Act
            var result = await _repository.GetByAlbumId(album.Id, 2, 2);

            // Assert
            result.Items.Select(photo => photo.FileName).Should().ContainInOrder("Photo_003.jpg", "Photo_004.jpg");
            result.TotalItems.Should().Be(5);
        }

        [Fact]
        public async Task GetByAlbumId_WhenRequestingLastPartialPage_ReturnsRemainingItems()
        {
            // Arrange
            var album = await CreateAlbum();
            await CreatePhotos(album.Id, 5);

            // Act
            var result = await _repository.GetByAlbumId(album.Id, 3, 2);

            // Assert
            result.Items.Should().ContainSingle().Which.FileName.Should().Be("Photo_005.jpg");
            result.TotalItems.Should().Be(5);
        }

        [Fact]
        public async Task GetByAlbumId_WhenPageIsBeyondLast_ReturnsEmptyItemsAndCorrectTotal()
        {
            // Arrange
            var album = await CreateAlbum();
            await CreatePhotos(album.Id, 5);

            // Act
            var result = await _repository.GetByAlbumId(album.Id, 4, 2);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(5);
        }

        [Fact]
        public async Task GetByAlbumId_WhenOtherAlbumsContainPhotos_ExcludesTheirPhotosFromItemsAndTotal()
        {
            // Arrange
            var requestedAlbum = await CreateAlbum("Fashion", "Fashion");
            var otherAlbum = await CreateAlbum("Glamour", "Glamour");
            await CreatePhotos(requestedAlbum.Id, 3);
            await _repository.CreatePhoto(otherAlbum.Id, "Other_001.jpg");

            // Act
            var result = await _repository.GetByAlbumId(requestedAlbum.Id, 1, 12);

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items.Should().OnlyContain(photo => photo.AlbumId == requestedAlbum.Id);
            result.TotalItems.Should().Be(3);
        }

        [Fact]
        public async Task GetById_WhenPhotoExists_ReturnsPhoto()
        {
            // Arrange
            var album = await CreateAlbum();
            var created = await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");

            // Act
            var photo = await _repository.GetById(created.Id);

            // Assert
            photo.Should().NotBeNull();
            photo.Should().BeEquivalentTo(new { created.Id, AlbumId = album.Id, FileName = "Fashion_001.jpg" });
        }

        [Fact]
        public async Task GetById_WhenPhotoDoesNotExist_ReturnsNull()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            // Act
            var photo = await _repository.GetById(photoId);

            // Assert
            photo.Should().BeNull();
        }

        [Fact]
        public async Task GetAllPhotos_WhenPhotosExist_ReturnsEveryPhoto()
        {
            // Arrange
            var firstAlbum = await CreateAlbum("Fashion", "Fashion");
            var secondAlbum = await CreateAlbum("Glamour", "Glamour");
            await _repository.CreatePhoto(firstAlbum.Id, "Fashion_001.jpg");
            await _repository.CreatePhoto(firstAlbum.Id, "Fashion_002.jpg");
            await _repository.CreatePhoto(secondAlbum.Id, "Glamour_001.jpg");

            // Act
            var photos = await _repository.GetAllPhotos();

            // Assert
            photos.Select(photo => photo.FileName).Should().BeEquivalentTo(["Fashion_001.jpg", "Fashion_002.jpg", "Glamour_001.jpg"]);
        }

        [Fact]
        public async Task GetAllPhotos_WhenDatabaseIsEmpty_ReturnsEmptyList()
        {
            // Arrange

            // Act
            var photos = await _repository.GetAllPhotos();

            // Assert
            photos.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateDescription_WhenPhotoExists_UpdatesAndReturnsPhoto()
        {
            // Arrange
            var album = await CreateAlbum();
            var created = await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");

            // Act
            var updated = await _repository.UpdateDescription(created.Id, "Ritratto in studio");
            DbContext.ChangeTracker.Clear();
            var storedPhoto = await DbContext.Foto.SingleAsync(photo => photo.Id == created.Id);

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Ritratto in studio");
            storedPhoto.Description.Should().Be("Ritratto in studio");
        }

        [Fact]
        public async Task UpdateDescription_WhenDescriptionHasOuterSpaces_TrimsDescription()
        {
            // Arrange
            var album = await CreateAlbum();
            var photo = await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");

            // Act
            var updated = await _repository.UpdateDescription(photo.Id, "  Ritratto in studio  ");

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Ritratto in studio");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task UpdateDescription_WhenDescriptionIsMissing_SetsDescriptionToNull(string? description)
        {
            // Arrange
            var album = await CreateAlbum();
            var photo = await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");
            await _repository.UpdateDescription(photo.Id, "Descrizione iniziale");

            // Act
            var updated = await _repository.UpdateDescription(photo.Id, description);

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().BeNull();
        }

        [Fact]
        public async Task UpdateDescription_WhenPhotoDoesNotExist_ReturnsNull()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            // Act
            var photo = await _repository.UpdateDescription(photoId, "Descrizione");

            // Assert
            photo.Should().BeNull();
        }

        [Fact]
        public async Task GetMissingDescriptions_WhenSomeDescriptionsAreMissing_ReturnsOnlyMissingPhotos()
        {
            // Arrange
            var album = await CreateAlbum();
            var nullDescription = await _repository.CreatePhoto(album.Id, "Photo_001.jpg");
            var emptyDescription = await _repository.CreatePhoto(album.Id, "Photo_002.jpg");
            var described = await _repository.CreatePhoto(album.Id, "Photo_003.jpg");

            await _repository.UpdateDescription(nullDescription.Id, null);
            emptyDescription.Description = string.Empty;
            described.Description = "Descrizione";
            await _repository.Save();

            // Act
            var photos = await _repository.GetMissingDescriptions();

            // Assert
            photos.Select(photo => photo.Id).Should().BeEquivalentTo([nullDescription.Id, emptyDescription.Id]);
        }

        [Fact]
        public async Task GetMissingDescriptions_WhenAllDescriptionsExist_ReturnsEmptyList()
        {
            // Arrange
            var album = await CreateAlbum();
            var first = await _repository.CreatePhoto(album.Id, "Photo_001.jpg");
            var second = await _repository.CreatePhoto(album.Id, "Photo_002.jpg");

            first.Description = "Prima foto";
            second.Description = "Seconda foto";
            await _repository.Save();

            // Act
            var photos = await _repository.GetMissingDescriptions();

            // Assert
            photos.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIds_WhenSomeIdsExist_ReturnsMatchingPhotosOnly()
        {
            // Arrange
            var album = await CreateAlbum();
            var first = await _repository.CreatePhoto(album.Id, "Photo_001.jpg");
            var second = await _repository.CreatePhoto(album.Id, "Photo_002.jpg");
            await _repository.CreatePhoto(album.Id, "Photo_003.jpg");
            var requestedIds = new[] { first.Id, second.Id, Guid.NewGuid() };

            // Act
            var photos = await _repository.GetByIds(requestedIds);

            // Assert
            photos.Select(photo => photo.Id).Should().BeEquivalentTo([first.Id, second.Id]);
        }

        [Fact]
        public async Task GetByIds_WhenIdsAreEmpty_ReturnsEmptyList()
        {
            // Arrange
            Guid[] photoIds = [];

            // Act
            var photos = await _repository.GetByIds(photoIds);

            // Assert
            photos.Should().BeEmpty();
        }

        [Fact]
        public async Task Save_WhenTrackedPhotoIsModified_PersistsChangesAndReturnsAffectedRows()
        {
            // Arrange
            var album = await CreateAlbum();
            var photo = await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");
            photo.Description = "Nuova descrizione";

            // Act
            var affectedRows = await _repository.Save();
            DbContext.ChangeTracker.Clear();
            var storedPhoto = await DbContext.Foto.SingleAsync(item => item.Id == photo.Id);

            // Assert
            affectedRows.Should().Be(1);
            storedPhoto.Description.Should().Be("Nuova descrizione");
        }

        [Fact]
        public async Task Save_WhenMultipleTrackedPhotosAreModified_ReturnsAffectedRows()
        {
            // Arrange
            var album = await CreateAlbum();
            var first = await _repository.CreatePhoto(album.Id, "Photo_001.jpg");
            var second = await _repository.CreatePhoto(album.Id, "Photo_002.jpg");

            first.Description = "Prima descrizione";
            second.Description = "Seconda descrizione";

            // Act
            var affectedRows = await _repository.Save();

            // Assert
            affectedRows.Should().Be(2);
        }

        [Fact]
        public async Task Save_WhenNothingIsModified_ReturnsZero()
        {
            // Arrange
            var album = await CreateAlbum();
            await _repository.CreatePhoto(album.Id, "Fashion_001.jpg");

            // Act
            var affectedRows = await _repository.Save();

            // Assert
            affectedRows.Should().Be(0);
        }

        [Fact]
        public async Task DeletingAlbum_WhenAlbumContainsPhotos_DeletesPhotosByCascade()
        {
            // Arrange
            var album = await CreateAlbum();
            await _repository.CreatePhoto(album.Id, "Photo_001.jpg");
            await _repository.CreatePhoto(album.Id, "Photo_002.jpg");

            // Act
            DbContext.Albums.Remove(album);
            await DbContext.SaveChangesAsync();
            var photos = await DbContext.Foto.ToListAsync();

            // Assert
            photos.Should().BeEmpty();
        }

        private async Task<Album> CreateAlbum(string name = "Test album", string path = "Test-album")
        {
            var album = new Album { Name = name, Path = path };

            DbContext.Albums.Add(album);
            await DbContext.SaveChangesAsync();

            return album;
        }

        private async Task CreatePhotos(Guid albumId, int count)
        {
            for (var index = 1; index <= count; index++)
            {
                await _repository.CreatePhoto(albumId, $"Photo_{index:000}.jpg");
            }
        }
    }
}
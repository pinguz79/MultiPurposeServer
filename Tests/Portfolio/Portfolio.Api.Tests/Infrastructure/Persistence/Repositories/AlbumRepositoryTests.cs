using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Infrastructure.Persistence.Repositories
{
    public class AlbumRepositoryTests : RepositoryTestBase
    {
        private readonly AlbumRepository _repository;

        public AlbumRepositoryTests()
        {
            _repository = new AlbumRepository(DbContext);
        }

        #region Create e Delete

        [Fact]
        public async Task CreateAlbum_WhenParentIsNull_CreatesRootAlbum()
        {
            // Arrange
            const string name = "Fashion";
            const string path = "Fashion";

            // Act
            var album = await _repository.CreateAlbum(name, null, path);
            var storedAlbums = await DbContext.Albums.ToListAsync();

            // Assert
            album.Should().BeEquivalentTo(new { Name = name, Path = path, ParentId = (Guid?)null });
            album.Id.Should().NotBeEmpty();
            storedAlbums.Should().ContainSingle(item => item.Id == album.Id);
        }

        [Fact]
        public async Task CreateAlbum_WhenParentExists_CreatesChildAlbum()
        {
            // Arrange
            var parent = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            var child = await _repository.CreateAlbum("Milano", parent.Id, "Milano");
            var storedChild = await DbContext.Albums.SingleAsync(album => album.Id == child.Id);

            // Assert
            child.Should().BeEquivalentTo(new { Name = "Milano", Path = "Milano", ParentId = (Guid?)parent.Id });
            storedChild.ParentId.Should().Be(parent.Id);
        }

        [Fact]
        public async Task CreateAlbum_WhenDescriptionIsSpecified_PersistsDescription()
        {
            // Arrange
            const string description = "Editorial fashion";

            // Act
            var album = await _repository.CreateAlbum("Fashion", null, "Fashion", description);
            var storedAlbum = await DbContext.Albums.SingleAsync(item => item.Id == album.Id);

            // Assert
            album.Description.Should().Be(description);
            storedAlbum.Description.Should().Be(description);
        }

        [Fact]
        public async Task DeleteAlbum_WhenAlbumExists_RemovesAlbum()
        {
            // Arrange
            var album = await _repository.CreateAlbum("Temporary", null, "Temporary");

            // Act
            await _repository.DeleteAlbum(album.Id);

            // Assert
            (await DbContext.Albums.AnyAsync(item => item.Id == album.Id)).Should().BeFalse();
        }

        #endregion

        #region Get

        [Fact]
        public async Task GetAlbums_WhenParentIsNull_ReturnsOnlyRootAlbums()
        {
            // Arrange
            var fashion = await _repository.CreateAlbum("Fashion", null, "Fashion");
            await _repository.CreateAlbum("Glamour", null, "Glamour");
            await _repository.CreateAlbum("Milano", fashion.Id, "Milano");

            // Act
            var albums = await _repository.GetAlbums(null);

            // Assert
            albums.Select(album => album.Name).Should().BeEquivalentTo(["Fashion", "Glamour"]);
        }

        [Fact]
        public async Task GetAlbums_WhenParentHasChildren_ReturnsOnlyDirectChildren()
        {
            // Arrange
            var fashion = await _repository.CreateAlbum("Fashion", null, "Fashion");
            var milano = await _repository.CreateAlbum("Milano", fashion.Id, "Milano");
            await _repository.CreateAlbum("Roma", fashion.Id, "Roma");
            await _repository.CreateAlbum("Studio", milano.Id, "Studio");

            // Act
            var albums = await _repository.GetAlbums(fashion.Id);

            // Assert
            albums.Select(album => album.Name).Should().BeEquivalentTo(["Milano", "Roma"]);
        }

        [Fact]
        public async Task GetAlbums_WhenParentHasNoChildren_ReturnsEmptyList()
        {
            // Arrange
            var album = await _repository.CreateAlbum("Portraits", null, "Portraits");

            // Act
            var albums = await _repository.GetAlbums(album.Id);

            // Assert
            albums.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMissingDescriptions_WhenSomeDescriptionsAreMissing_ReturnsOnlyMissingAlbums()
        {
            // Arrange
            var nullDescription = await _repository.CreateAlbum("2019", null, "2019");
            var emptyDescription = await _repository.CreateAlbum("2020", null, "2020", string.Empty);
            await _repository.CreateAlbum("2021", null, "2021", "Calendari realizzati nel 2021.");

            // Act
            var albums = await _repository.GetMissingDescriptions();

            // Assert
            albums.Select(album => album.Id).Should().BeEquivalentTo([nullDescription.Id, emptyDescription.Id]);
        }

        [Fact]
        public async Task GetMissingDescriptions_WhenAllDescriptionsExist_ReturnsEmptyList()
        {
            // Arrange
            await _repository.CreateAlbum("2019", null, "2019", "Calendari realizzati nel 2019.");
            await _repository.CreateAlbum("2020", null, "2020", "Calendari realizzati nel 2020.");

            // Act
            var albums = await _repository.GetMissingDescriptions();

            // Assert
            albums.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAlbums_WhenAlbumsExist_ReturnsEveryAlbum()
        {
            // Arrange
            var parent = await _repository.CreateAlbum("Fashion", null, "Fashion");
            await _repository.CreateAlbum("Milano", parent.Id, "Milano");
            await _repository.CreateAlbum("Glamour", null, "Glamour");

            // Act
            var albums = await _repository.GetAll();

            // Assert
            albums.Select(album => album.Name).Should().BeEquivalentTo(["Fashion", "Milano", "Glamour"]);
        }

        [Fact]
        public async Task GetAllAlbums_WhenDatabaseIsEmpty_ReturnsEmptyList()
        {
            // Arrange

            // Act
            var albums = await _repository.GetAll();

            // Assert
            albums.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_WhenAlbumExists_ReturnsAlbum()
        {
            // Arrange
            var created = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            var album = await _repository.GetById(created.Id);

            // Assert
            album.Should().NotBeNull();
            album.Should().BeEquivalentTo(new { created.Id, Name = "Fashion", Path = "Fashion", ParentId = (Guid?)null });
        }

        [Fact]
        public async Task GetById_WhenAlbumDoesNotExist_ReturnsNull()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            // Act
            var album = await _repository.GetById(albumId);

            // Assert
            album.Should().BeNull();
        }

        #endregion

        #region Update

        [Fact]
        public async Task UpdateName_WhenAlbumExists_UpdatesAndReturnsAlbum()
        {
            // Arrange
            var created = await _repository.CreateAlbum("Old name", null, "Old-name");

            // Act
            var updated = await _repository.UpdateName(created.Id, "New name");
            DbContext.ChangeTracker.Clear();
            var storedAlbum = await DbContext.Albums.SingleAsync(album => album.Id == created.Id);

            // Assert
            updated.Should().NotBeNull();
            updated.Should().BeEquivalentTo(new { created.Id, Name = "New name", Path = "Old-name" });
            storedAlbum.Should().BeEquivalentTo(new { created.Id, Name = "New name", Path = "Old-name" });
        }

        [Fact]
        public async Task UpdateName_WhenAlbumDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            // Act
            Func<Task> action = () => _repository.UpdateName(albumId, "New name");

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateName_WhenNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var album = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            Func<Task> action = () => _repository.UpdateName(album.Id, null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task UpdateName_WhenNameIsEmpty_ThrowsArgumentException(string name)
        {
            // Arrange
            var album = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            Func<Task> action = () => _repository.UpdateName(album.Id, name);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetByIds_WhenSomeAlbumsExist_ReturnsMatchingAlbumsOnly()
        {
            // Arrange
            var fashion = await _repository.CreateAlbum("Fashion", null, "Fashion");
            var glamour = await _repository.CreateAlbum("Glamour", null, "Glamour");
            await _repository.CreateAlbum("Portraits", null, "Portraits");
            var requestedIds = new[] { fashion.Id, glamour.Id, Guid.NewGuid() };

            // Act
            var albums = await _repository.GetByIds(requestedIds);

            // Assert
            albums.Select(album => album.Id).Should().BeEquivalentTo([fashion.Id, glamour.Id]);
        }

        [Fact]
        public async Task GetByIds_WhenIdsAreEmpty_ReturnsEmptyList()
        {
            // Arrange
            Guid[] albumIds = [];

            // Act
            var albums = await _repository.GetByIds(albumIds);

            // Assert
            albums.Should().BeEmpty();
        }

        #endregion

        #region Save

        [Fact]
        public async Task Save_WhenTrackedAlbumIsModified_PersistsChanges()
        {
            // Arrange
            var album = await _repository.CreateAlbum("Fashion", null, "Fashion");
            album.Description = "Fashion photography";

            // Act
            var affectedRows = await _repository.SaveIfRequired();
            DbContext.ChangeTracker.Clear();
            var storedAlbum = await DbContext.Albums.SingleAsync(item => item.Id == album.Id);

            // Assert
            affectedRows.Should().Be(1);
            storedAlbum.Description.Should().Be("Fashion photography");
        }

        [Fact]
        public async Task Save_WhenNothingIsModified_ReturnsZero()
        {
            // Arrange
            await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            var affectedRows = await _repository.SaveIfRequired();

            // Assert
            affectedRows.Should().Be(0);
        }

        #endregion

        #region Path

        [Fact]
        public async Task ResolvePath_WhenPathExists_ReturnsFinalAlbum()
        {
            // Arrange
            var fashion = await _repository.CreateAlbum("Fashion", null, "Fashion");
            var milano = await _repository.CreateAlbum("Milano", fashion.Id, "Milano");
            var studio = await _repository.CreateAlbum("Studio", milano.Id, "Studio");

            // Act
            var album = await _repository.ResolvePath("Fashion/Milano/Studio");

            // Assert
            album.Should().NotBeNull();
            album.Should().BeEquivalentTo(new { studio.Id, Name = "Studio", Path = "Studio", ParentId = (Guid?)milano.Id });
        }

        [Theory]
        [InlineData("fashion/milano/studio")]
        [InlineData("FASHION/MILANO/STUDIO")]
        [InlineData("/Fashion/Milano/Studio/")]
        [InlineData("Fashion//Milano//Studio")]
        [InlineData(@"Fashion\Milano\Studio")]
        public async Task ResolvePath_WhenPathUsesDifferentFormatting_ReturnsAlbum(string path)
        {
            // Arrange
            var fashion = await _repository.CreateAlbum("Fashion", null, "Fashion");
            var milano = await _repository.CreateAlbum("Milano", fashion.Id, "Milano");
            var studio = await _repository.CreateAlbum("Studio", milano.Id, "Studio");

            // Act
            var album = await _repository.ResolvePath(path);

            // Assert
            album.Should().NotBeNull();
            album!.Id.Should().Be(studio.Id);
        }

        [Fact]
        public async Task ResolvePath_WhenFinalSegmentDoesNotExist_ReturnsNull()
        {
            // Arrange
            var fashion = await _repository.CreateAlbum("Fashion", null, "Fashion");
            await _repository.CreateAlbum("Milano", fashion.Id, "Milano");

            // Act
            var album = await _repository.ResolvePath("Fashion/Milano/Unknown");

            // Assert
            album.Should().BeNull();
        }

        [Fact]
        public async Task ResolvePath_WhenIntermediateSegmentDoesNotExist_ReturnsNull()
        {
            // Arrange
            await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            var album = await _repository.ResolvePath("Fashion/Unknown/Studio");

            // Assert
            album.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("/")]
        [InlineData(@"\\")]
        public async Task ResolvePath_WhenPathIsEmpty_ReturnsNull(string path)
        {
            // Arrange

            // Act
            var album = await _repository.ResolvePath(path);

            // Assert
            album.Should().BeNull();
        }

        #endregion

        #region Transazioni

        [Fact]
        public async Task UpdateName_WhenNoTransactionIsActive_SavesImmediately()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);

            var album = new Album { Name = "Old name" };

            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var repository = new AlbumRepository(context);

            // Act
            await repository.UpdateName(album.Id, "New name");

            context.ChangeTracker.Clear();
            var persistedAlbum = await context.Albums.SingleAsync(value => value.Id == album.Id);

            // Assert
            persistedAlbum.Name.Should().Be("New name");
        }

        [Fact]
        public async Task UpdateName_WhenTransactionIsActive_DoesNotSaveBeforeCommit()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);

            var album = new Album { Name = "Old name" };

            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var repository = new AlbumRepository(context);
            await using var transaction = await repository.BeginTransaction();

            // Act
            await repository.UpdateName(album.Id, "New name");

            var persistedName = await GetPersistedValue<string>(connection, album.Id, "Albums", "Name");

            // Assert
            persistedName.Should().Be("Old name");
        }

        [Fact]
        public async Task Commit_WhenTransactionContainsChanges_PersistsChanges()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);

            var album = new Album { Name = "Old name" };

            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var repository = new AlbumRepository(context);
            await using var transaction = await repository.BeginTransaction();

            await repository.UpdateName(album.Id, "New name");

            // Act
            await transaction.Commit();

            context.ChangeTracker.Clear();
            var persistedAlbum = await context.Albums.SingleAsync(value => value.Id == album.Id);

            // Assert
            persistedAlbum.Name.Should().Be("New name");
        }

        [Fact]
        public async Task Checkpoint_WhenCompletedAndTransactionIsCommitted_PersistsCheckpointChanges()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);
            var album = new Album { Name = "Old name" };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
            var repository = new AlbumRepository(context);
            await using var transaction = await repository.BeginTransaction();
            await using var checkpoint = await transaction.BeginCheckpoint();

            await repository.UpdateName(album.Id, "New name");

            // Act
            await checkpoint.Complete();
            await transaction.Commit();

            // Assert
            var persistedAlbum = await context.Albums.AsNoTracking().SingleAsync(value => value.Id == album.Id);
            persistedAlbum.Name.Should().Be("New name");
        }

        [Fact]
        public async Task Checkpoint_WhenDisposedWithoutCompletion_RollsBackOnlyCheckpointChanges()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);
            var album = new Album { Name = "Old name", Description = "Old description" };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
            var repository = new AlbumRepository(context);
            await using var transaction = await repository.BeginTransaction();

            await using (var completedCheckpoint = await transaction.BeginCheckpoint())
            {
                await repository.UpdateName(album.Id, "New name");
                await completedCheckpoint.Complete();
            }

            await using (var rolledBackCheckpoint = await transaction.BeginCheckpoint())
            {
                await repository.UpdateDescription(album.Id, "New description");
            }

            // Act
            await transaction.Commit();

            // Assert
            var persistedAlbum = await context.Albums.AsNoTracking().SingleAsync(value => value.Id == album.Id);
            persistedAlbum.Name.Should().Be("New name");
            persistedAlbum.Description.Should().Be("Old description");
        }

        [Fact]
        public async Task Dispose_WhenTransactionIsNotCommitted_DiscardsChanges()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);

            var album = new Album { Name = "Old name" };

            context.Albums.Add(album);
            await context.SaveChangesAsync();

            var repository = new AlbumRepository(context);
            var transaction = await repository.BeginTransaction();

            await repository.UpdateName(album.Id, "New name");

            // Act
            await transaction.DisposeAsync();

            var persistedAlbum = await context.Albums.AsNoTracking().SingleAsync(value => value.Id == album.Id);

            // Assert
            persistedAlbum.Name.Should().Be("Old name");
        }

        [Fact]
        public async Task BeginTransaction_WhenTransactionIsAlreadyActive_ThrowsInvalidOperationException()
        {
            // Arrange
            await using var connection = await CreateInitializedConnection();
            await using var context = CreateContext(connection);

            var repository = new AlbumRepository(context);
            await using var transaction = await repository.BeginTransaction();

            // Act
            Func<Task> action = repository.BeginTransaction;

            // Assert
            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A repository transaction is already active.");
        }

        [Fact]
        public async Task UpdateDescription_WhenAlbumExists_UpdatesAndReturnsAlbum()
        {
            // Arrange
            var created = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            var updated = await _repository.UpdateDescription(created.Id, "  Fashion photography  ");

            DbContext.ChangeTracker.Clear();

            var storedAlbum = await DbContext.Albums.SingleAsync(album => album.Id == created.Id);

            // Assert
            updated.Should().NotBeNull();
            updated!.Description.Should().Be("Fashion photography");
            storedAlbum.Description.Should().Be("Fashion photography");
        }

        [Fact]
        public async Task UpdateDescription_WhenDescriptionIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var album = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            Func<Task> action = () => _repository.UpdateDescription(album.Id, null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task UpdateDescription_WhenDescriptionIsEmpty_ThrowsArgumentException(string description)
        {
            // Arrange
            var album = await _repository.CreateAlbum("Fashion", null, "Fashion");

            // Act
            Func<Task> action = () => _repository.UpdateDescription(album.Id, description);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateDescription_WhenAlbumDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var albumId = Guid.NewGuid();

            // Act
            Func<Task> action = () => _repository.UpdateDescription(albumId, "Description");

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>();
        }
        #endregion

    }
}

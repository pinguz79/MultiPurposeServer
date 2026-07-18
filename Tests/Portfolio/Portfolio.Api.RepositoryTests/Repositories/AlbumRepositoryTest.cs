using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Repositories;
using Portfolio.Api.RepositoryTests.Infrastructure;

namespace Portfolio.Api.RepositoryTests.Repositories;

public class AlbumRepositoryTests : RepositoryTestBase
{
    private readonly AlbumRepository _repository;

    public AlbumRepositoryTests()
    {
        _repository = new AlbumRepository(DbContext);
    }

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
    public async Task GetAllAlbums_WhenAlbumsExist_ReturnsEveryAlbum()
    {
        // Arrange
        var parent = await _repository.CreateAlbum("Fashion", null, "Fashion");
        await _repository.CreateAlbum("Milano", parent.Id, "Milano");
        await _repository.CreateAlbum("Glamour", null, "Glamour");

        // Act
        var albums = await _repository.GetAllAlbums();

        // Assert
        albums.Select(album => album.Name).Should().BeEquivalentTo(["Fashion", "Milano", "Glamour"]);
    }

    [Fact]
    public async Task GetAllAlbums_WhenDatabaseIsEmpty_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var albums = await _repository.GetAllAlbums();

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
    public async Task UpdateName_WhenAlbumDoesNotExist_ReturnsNull()
    {
        // Arrange
        var albumId = Guid.NewGuid();

        // Act
        var album = await _repository.UpdateName(albumId, "New name");

        // Assert
        album.Should().BeNull();
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

    [Fact]
    public async Task Save_WhenTrackedAlbumIsModified_PersistsChanges()
    {
        // Arrange
        var album = await _repository.CreateAlbum("Fashion", null, "Fashion");
        album.Description = "Fashion photography";

        // Act
        var affectedRows = await _repository.Save();
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
        var affectedRows = await _repository.Save();

        // Assert
        affectedRows.Should().Be(0);
    }

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
}
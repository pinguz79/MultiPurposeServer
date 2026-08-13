using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Portfolio.Api.Application.Diagnostics;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Application.Services
{
    public class AlbumServiceTests : IDisposable
    {
        private readonly Mock<IAlbumRepository> _albumRepository;
        private readonly Mock<IFotoRepository> _fotoRepository;
        private readonly Mock<IAlbumSyncReportStore> _reportStore;
        private readonly Mock<ILogger<AlbumService>> _logger;
        private readonly Mock<IPersistenceTransaction> _syncTransaction;
        private readonly string _rootPath;
        private readonly AlbumService _service;

        public AlbumServiceTests()
        {
            _albumRepository = new Mock<IAlbumRepository>();
            _fotoRepository = new Mock<IFotoRepository>();
            _reportStore = new Mock<IAlbumSyncReportStore>();
            _logger = new Mock<ILogger<AlbumService>>();
            _syncTransaction = new Mock<IPersistenceTransaction>();
            _albumRepository.Setup(repository => repository.BeginTransaction()).ReturnsAsync(_syncTransaction.Object);
            _rootPath = Path.Combine(Path.GetTempPath(), "Portfolio.Api.ServiceTests", Guid.NewGuid().ToString("N"));

            var options = Options.Create(new PortfolioAlbumOptions { RootPath = _rootPath });
            _service = new AlbumService(_albumRepository.Object, _fotoRepository.Object, options, _reportStore.Object, _logger.Object);
        }

        #region GetAlbums

        [Fact]
        public async Task GetAlbums_WhenRepositoryReturnsAlbums_ReturnsRepositoryResult()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "Fashion", ParentId = parentId },
                new() { Id = Guid.NewGuid(), Name = "Glamour", ParentId = parentId }
            };

            _albumRepository.Setup(repository => repository.GetAlbums(parentId)).ReturnsAsync(albums);

            // Act
            var result = await _service.GetAlbums(parentId);

            // Assert
            result.Should().BeSameAs(albums);
            _albumRepository.Verify(repository => repository.GetAlbums(parentId), Times.Once);
        }

        [Fact]
        public async Task GetAlbums_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _albumRepository.Setup(repository => repository.GetAlbums(null)).ReturnsAsync([]);

            // Act
            var result = await _service.GetAlbums(null);

            // Assert
            result.Should().BeEmpty();
            _albumRepository.Verify(repository => repository.GetAlbums(null), Times.Once);
        }

        #endregion

        #region CreateAlbum

        [Fact]
        public async Task CreateAlbum_WhenCreatingRootAlbum_NormalizesPathAndCreatesDirectory()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion Week", Path = "Fashion-Week", Description = "Editorial fashion" };

            _albumRepository.Setup(repository => repository.CreateAlbum(" Fashion Week ", null, "Fashion-Week", "Editorial fashion")).ReturnsAsync(album);

            // Act
            var result = await _service.CreateAlbum(" Fashion Week ", null, "Editorial fashion");

            // Assert
            result.Should().BeSameAs(album);
            Directory.Exists(Path.Combine(_rootPath, "Fashion-Week")).Should().BeTrue();
            _albumRepository.Verify(repository => repository.CreateAlbum(" Fashion Week ", null, "Fashion-Week", "Editorial fashion"), Times.Once);
        }

        [Fact]
        public async Task CreateAlbum_WhenExplicitPathIsSpecified_UsesItInsteadOfName()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Sunset @ Paraggi", Path = "sunset-at-paraggi" };

            _albumRepository.Setup(repository => repository.CreateAlbum("Sunset @ Paraggi", null, "sunset-at-paraggi", null)).ReturnsAsync(album);

            // Act
            var result = await _service.CreateAlbum("Sunset @ Paraggi", null, path: " sunset at paraggi ");

            // Assert
            result.Should().BeSameAs(album);
            Directory.Exists(Path.Combine(_rootPath, "sunset-at-paraggi")).Should().BeTrue();
            _albumRepository.Verify(repository => repository.CreateAlbum("Sunset @ Paraggi", null, "sunset-at-paraggi", null), Times.Once);
        }

        [Theory]
        [InlineData("../Fashion")]
        [InlineData("Fashion/Portraits")]
        [InlineData("Fashion\\Portraits")]
        [InlineData("cache-albums")]
        [InlineData("Fashion?")]
        public async Task CreateAlbum_WhenExplicitPathIsInvalid_ThrowsArgumentExceptionWithoutCreatingAlbum(string path)
        {
            // Arrange

            // Act
            var action = async () => await _service.CreateAlbum("Fashion", null, path: path);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>().WithParameterName("path");
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateAlbum_WhenCreatingChildAlbum_CreatesNestedDirectory()
        {
            // Arrange
            var parent = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", Description = "Editorial fashion" };
            var child = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano", ParentId = parent.Id, Parent = parent };

            _albumRepository.Setup(repository => repository.GetById(parent.Id)).ReturnsAsync(parent);
            _albumRepository.Setup(repository => repository.CreateAlbum("Milano", parent.Id, "Milano", "Editorial fashion")).ReturnsAsync(child);

            // Act
            var result = await _service.CreateAlbum("Milano", parent.Id, "Editorial fashion");

            // Assert
            result.Should().BeSameAs(child);
            Directory.Exists(Path.Combine(_rootPath, "Fashion", "Milano")).Should().BeTrue();
        }

        [Fact]
        public async Task CreateAlbum_WhenParentNavigationIsNotHydrated_LogsDiagnosticWarning()
        {
            // Arrange
            var grandParentId = Guid.NewGuid();
            var parent = new Album { Id = Guid.NewGuid(), Name = "Alessandra", Path = "Alessandra", ParentId = grandParentId };
            var child = new Album { Id = Guid.NewGuid(), Name = "Miss Villetta 2023", Path = "Miss-Villetta-2023", ParentId = parent.Id };

            _albumRepository.Setup(repository => repository.GetById(parent.Id)).ReturnsAsync(parent);
            _albumRepository.Setup(repository => repository.CreateAlbum("Miss Villetta 2023", parent.Id, "Miss-Villetta-2023", null)).ReturnsAsync(child);

            // Act
            await _service.CreateAlbum("Miss Villetta 2023", parent.Id);

            // Assert
            Directory.Exists(Path.Combine(_rootPath, "Miss-Villetta-2023")).Should().BeTrue();
            _logger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((value, _) => value.ToString()!.Contains("hierarchy is not fully loaded")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAlbum_WhenAlbumPathIsNull_UsesNormalizedNameForDirectory()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fine Art", Path = null, Description = "Editorial fashion" };

            _albumRepository.Setup(repository => repository.CreateAlbum("Fine Art", null, "Fine-Art", "Editorial fashion")).ReturnsAsync(album);

            // Act
            await _service.CreateAlbum("Fine Art", null, "Editorial fashion");

            // Assert
            Directory.Exists(Path.Combine(_rootPath, "Fine-Art")).Should().BeTrue();
        }

        [Fact]
        public async Task CreateAlbum_WhenParentContainsPhotos_ThrowsInvalidOperationExceptionWithoutCreatingAlbum()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var parent = new Album
            {
                Id = parentId,
                Name = "Fashion",
                Photos =
                [
                    new Foto
                    {
                        Id = Guid.NewGuid(),
                        FileName = "Photo.jpg",
                        AlbumId = parentId
                    }
                ]
            };

            _albumRepository.Setup(repository => repository.GetById(parentId)).ReturnsAsync(parent);

            // Act
            var action = async () => await _service.CreateAlbum("Milano", parentId);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Album 'Fashion' contains photos and cannot contain child albums.");
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateAlbum_WhenParentDoesNotExist_ThrowsKeyNotFoundExceptionWithoutCreatingAlbum()
        {
            // Arrange
            var parentId = Guid.NewGuid();

            _albumRepository.Setup(repository => repository.GetById(parentId)).ReturnsAsync((Album?)null);

            // Act
            var action = async () => await _service.CreateAlbum("Milano", parentId);

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"Album '{parentId}' was not found.");
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        #endregion

        #region GetMissingDescriptions

        [Fact]
        public async Task GetMissingDescriptions_WhenRepositoryReturnsAlbums_ReturnsRepositoryResult()
        {
            // Arrange
            var albums = new List<Album>
            {
                new() { Id = Guid.NewGuid(), Name = "2019", Description = null },
                new() { Id = Guid.NewGuid(), Name = "2020", Description = string.Empty }
            };
            _albumRepository.Setup(repository => repository.GetMissingDescriptions()).ReturnsAsync(albums);

            // Act
            var result = await _service.GetMissingDescriptions();

            // Assert
            result.Should().BeSameAs(albums);
            _albumRepository.Verify(repository => repository.GetMissingDescriptions(), Times.Once);
        }

        #endregion

        #region DeleteEmptyAlbum

        [Fact]
        public async Task DeleteEmptyAlbum_WhenAlbumAndDirectoryAreEmpty_DeletesBoth()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Temporary", Path = "Temporary" };
            var albumPath = Path.Combine(_rootPath, album.Path);
            Directory.CreateDirectory(albumPath);
            _albumRepository.Setup(repository => repository.GetById(album.Id)).ReturnsAsync(album);

            // Act
            await _service.DeleteEmptyAlbum(album.Id);

            // Assert
            Directory.Exists(albumPath).Should().BeFalse();
            _albumRepository.Verify(repository => repository.DeleteAlbum(album.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteEmptyAlbum_WhenAlbumDoesNotExist_ThrowsWithoutDeleting()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            _albumRepository.Setup(repository => repository.GetById(albumId)).ReturnsAsync((Album?)null);

            // Act
            var action = async () => await _service.DeleteEmptyAlbum(albumId);

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>();
            _albumRepository.Verify(repository => repository.DeleteAlbum(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEmptyAlbum_WhenAlbumHasChildren_ThrowsWithoutDeletingDirectoryOrEntity()
        {
            // Arrange
            var album = new Album
            {
                Id = Guid.NewGuid(),
                Name = "Fashion",
                Path = "Fashion",
                Children = [new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano" }]
            };
            var albumPath = Path.Combine(_rootPath, album.Path);
            Directory.CreateDirectory(albumPath);
            _albumRepository.Setup(repository => repository.GetById(album.Id)).ReturnsAsync(album);

            // Act
            var action = async () => await _service.DeleteEmptyAlbum(album.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*contains child albums*");
            Directory.Exists(albumPath).Should().BeTrue();
            _albumRepository.Verify(repository => repository.DeleteAlbum(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEmptyAlbum_WhenAlbumHasPhotos_ThrowsWithoutDeletingDirectoryOrEntity()
        {
            // Arrange
            var album = new Album
            {
                Id = Guid.NewGuid(),
                Name = "Fashion",
                Path = "Fashion",
                Photos = [new Foto { Id = Guid.NewGuid(), FileName = "photo.jpg" }]
            };
            var albumPath = Path.Combine(_rootPath, album.Path);
            Directory.CreateDirectory(albumPath);
            _albumRepository.Setup(repository => repository.GetById(album.Id)).ReturnsAsync(album);

            // Act
            var action = async () => await _service.DeleteEmptyAlbum(album.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*contains photos*");
            Directory.Exists(albumPath).Should().BeTrue();
            _albumRepository.Verify(repository => repository.DeleteAlbum(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEmptyAlbum_WhenDirectoryContainsEntries_ThrowsWithoutDeleting()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Temporary", Path = "Temporary" };
            var albumPath = Path.Combine(_rootPath, album.Path);
            Directory.CreateDirectory(albumPath);
            File.WriteAllText(Path.Combine(albumPath, "unexpected.txt"), "content");
            _albumRepository.Setup(repository => repository.GetById(album.Id)).ReturnsAsync(album);

            // Act
            var action = async () => await _service.DeleteEmptyAlbum(album.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*is not empty*");
            Directory.Exists(albumPath).Should().BeTrue();
            _albumRepository.Verify(repository => repository.DeleteAlbum(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteEmptyAlbum_WhenRepositoryDeletionFails_RecreatesDirectory()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Temporary", Path = "Temporary" };
            var albumPath = Path.Combine(_rootPath, album.Path);
            Directory.CreateDirectory(albumPath);
            _albumRepository.Setup(repository => repository.GetById(album.Id)).ReturnsAsync(album);
            _albumRepository.Setup(repository => repository.DeleteAlbum(album.Id)).ThrowsAsync(new InvalidOperationException("Database failure."));

            // Act
            var action = async () => await _service.DeleteEmptyAlbum(album.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database failure.");
            Directory.Exists(albumPath).Should().BeTrue();
        }

        #endregion

        #region ResolvePath

        [Fact]
        public async Task ResolvePath_WhenCalled_DelegatesToRepository()
        {
            // Arrange
            const string path = "Fashion/Milano";
            var album = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano" };

            _albumRepository.Setup(repository => repository.ResolvePath(path)).ReturnsAsync(album);

            // Act
            var result = await _service.ResolvePath(path);

            // Assert
            result.Should().BeSameAs(album);
            _albumRepository.Verify(repository => repository.ResolvePath(path), Times.Once);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_WhenCalled_DelegatesToRepository()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Fashion" };

            _albumRepository.Setup(repository => repository.GetById(albumId)).ReturnsAsync(album);

            // Act
            var result = await _service.GetById(albumId);

            // Assert
            result.Should().BeSameAs(album);
            _albumRepository.Verify(repository => repository.GetById(albumId), Times.Once);
        }

        #endregion

        #region UpdateName

        [Fact]
        public async Task UpdateName_WhenCalled_DelegatesToRepository()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "New name" };

            _albumRepository.Setup(repository => repository.UpdateName(albumId, "New name")).ReturnsAsync(album);

            // Act
            var result = await _service.UpdateName(albumId, "New name");

            // Assert
            result.Should().BeSameAs(album);
            _albumRepository.Verify(repository => repository.UpdateName(albumId, "New name"), Times.Once);
        }

        #endregion

        #region UpdateDescription

        [Fact]
        public async Task UpdateDescription_WhenCalled_DelegatesToRepository()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Description = "New description" };

            _albumRepository.Setup(repository => repository.UpdateDescription(albumId, "New description")).ReturnsAsync(album);

            // Act
            var result = await _service.UpdateDescription(albumId, "New description");

            // Assert
            result.Should().BeSameAs(album);
            _albumRepository.Verify(repository => repository.UpdateDescription(albumId, "New description"), Times.Once);
        }

        #endregion

        #region GetByNamePattern

        [Fact]
        public async Task GetByNamePattern_WhenAlbumsMatch_ReturnsMatchingAlbums()
        {
            // Arrange
            var fashion = new Album { Id = Guid.NewGuid(), Name = "Fashion Milano" };
            var glamour = new Album { Id = Guid.NewGuid(), Name = "Glamour Studio" };
            var fashionRoma = new Album { Id = Guid.NewGuid(), Name = "Fashion Roma" };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([fashion, glamour, fashionRoma]);

            // Act
            var result = await _service.GetByNamePattern("^fashion");

            // Assert
            result.Should().BeEquivalentTo([fashion, fashionRoma]);
        }

        [Fact]
        public async Task GetByNamePattern_WhenPatternUsesDifferentCase_MatchesCaseInsensitively()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion Milano" };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);

            // Act
            var result = await _service.GetByNamePattern("FASHION");

            // Assert
            result.Should().ContainSingle().Which.Should().BeSameAs(album);
        }

        [Fact]
        public async Task GetByNamePattern_WhenNoAlbumsMatch_ReturnsEmptyList()
        {
            // Arrange
            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync(
            [
                new Album { Id = Guid.NewGuid(), Name = "Fashion" },
                new Album { Id = Guid.NewGuid(), Name = "Glamour" }
            ]);

            // Act
            var result = await _service.GetByNamePattern("Portrait");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByNamePattern_WhenPatternIsInvalid_ThrowsArgumentExceptionWithoutCallingRepository()
        {
            // Arrange
            const string pattern = "[";

            // Act
            var action = async () => await _service.GetByNamePattern(pattern);

            // Assert
            var exception = await action.Should().ThrowAsync<ArgumentException>();
            exception.Which.ParamName.Should().Be("pattern");
            exception.Which.Message.Should().Contain("Invalid regular expression.");
            _albumRepository.Verify(repository => repository.GetAll(), Times.Never);
        }

        #endregion

        #region AmendDirectoryTree

        [Fact]
        public async Task AmendDirectoryTree_WhenRootDoesNotExist_CreatesRootAndSaves()
        {
            // Arrange
            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([]);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(0);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            Directory.Exists(_rootPath).Should().BeTrue();
            _albumRepository.Verify(repository => repository.SaveIfRequired(), Times.Once);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenDatabaseAlbumHasNoFolder_CreatesFolder()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(0);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            Directory.Exists(Path.Combine(_rootPath, "Fashion")).Should().BeTrue();
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenDatabaseAlbumPathIsNull_NormalizesPathAndCreatesFolder()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion Week", Path = null, ParentId = null };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(1);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            album.Path.Should().Be("Fashion-Week");
            Directory.Exists(Path.Combine(_rootPath, "Fashion-Week")).Should().BeTrue();
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenFilesystemContainsUnknownFolder_CreatesAlbum()
        {
            // Arrange
            Directory.CreateDirectory(Path.Combine(_rootPath, "Fashion"));
            var createdAlbum = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([]);
            _albumRepository.Setup(repository => repository.CreateAlbum("Fashion", null, "Fashion")).ReturnsAsync(createdAlbum);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(1);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            _albumRepository.Verify(repository => repository.CreateAlbum("Fashion", null, "Fashion"), Times.Once);
            _albumRepository.Verify(repository => repository.SaveIfRequired(), Times.Once);
        }

        [Theory]
        [InlineData("cache")]
        [InlineData("Cache")]
        [InlineData("cache-images")]
        public async Task AmendDirectoryTree_WhenFilesystemContainsCacheFolder_IgnoresFolder(string folderName)
        {
            // Arrange
            Directory.CreateDirectory(Path.Combine(_rootPath, folderName));

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([]);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(0);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenAlbumFolderContainsUnknownPhoto_CreatesPhoto()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null };
            var albumPath = Path.Combine(_rootPath, "Fashion");

            Directory.CreateDirectory(albumPath);
            await File.WriteAllBytesAsync(Path.Combine(albumPath, "Photo_001.jpg"), []);

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(1);

            var createdPhoto = new Foto { Id = Guid.NewGuid(), AlbumId = album.Id, FileName = "Photo_001.jpg" };
            _fotoRepository.Setup(repository => repository.CreatePhoto(album.Id, "Photo_001.jpg")).ReturnsAsync(createdPhoto);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            album.Photos.Should().ContainSingle().Which.Should().BeSameAs(createdPhoto);
            _fotoRepository.Verify(repository => repository.CreatePhoto(album.Id, "Photo_001.jpg"), Times.Once);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenAlbumAlreadyContainsPhoto_DoesNotCreateDuplicate()
        {
            // Arrange
            var album = new Album
            {
                Id = Guid.NewGuid(),
                Name = "Fashion",
                Path = "Fashion",
                ParentId = null,
                Photos = [new Foto { Id = Guid.NewGuid(), FileName = "Photo_001.jpg" }]
            };

            var albumPath = Path.Combine(_rootPath, "Fashion");
            Directory.CreateDirectory(albumPath);
            await File.WriteAllBytesAsync(Path.Combine(albumPath, "photo_001.jpg"), []);

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(0);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            album.Photos.Should().ContainSingle();
            _fotoRepository.Verify(repository => repository.CreatePhoto(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenNestedFilesystemFolderExists_CreatesChildAlbumWithParentId()
        {
            // Arrange
            var parent = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null };
            var parentPath = Path.Combine(_rootPath, "Fashion");

            Directory.CreateDirectory(Path.Combine(parentPath, "Milano"));

            var child = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano", ParentId = parent.Id };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([parent]);
            _albumRepository.Setup(repository => repository.CreateAlbum("Milano", parent.Id, "Milano")).ReturnsAsync(child);
            _albumRepository.Setup(repository => repository.SaveIfRequired()).ReturnsAsync(1);

            // Act
            await _service.AmendDirectoryTree();

            // Assert
            _albumRepository.Verify(repository => repository.CreateAlbum("Milano", parent.Id, "Milano"), Times.Once);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenAlbumContainsPhotosAndChildDirectory_ReportsDegradedStatusWithoutMutatingAlbum()
        {
            // Arrange
            var album = new Album
            {
                Id = Guid.NewGuid(),
                Name = "Fashion",
                Path = "Fashion",
                ParentId = null,
                Photos = [new Foto { Id = Guid.NewGuid(), FileName = "Photo_001.jpg" }]
            };

            var albumPath = Path.Combine(_rootPath, "Fashion");
            Directory.CreateDirectory(Path.Combine(albumPath, "Milano"));

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);

            // Act
            var report = await _service.AmendDirectoryTree();

            // Assert
            report.Status.Should().Be(AlbumSyncStatus.Degraded);
            report.Findings.Should().ContainSingle(finding => finding.Type == "MixedAlbumContent" && finding.AlbumId == album.Id);
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            _fotoRepository.Verify(repository => repository.CreatePhoto(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenAlbumContainsChildrenAndPhotoFile_ReportsDegradedStatusWithoutMutatingAlbum()
        {
            // Arrange
            var parent = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null };
            var child = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano", ParentId = parent.Id };
            var parentPath = Path.Combine(_rootPath, "Fashion");

            Directory.CreateDirectory(parentPath);
            await File.WriteAllBytesAsync(Path.Combine(parentPath, "Photo_001.jpg"), []);

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([parent, child]);

            // Act
            var report = await _service.AmendDirectoryTree();

            // Assert
            report.Status.Should().Be(AlbumSyncStatus.Degraded);
            report.Findings.Should().ContainSingle(finding => finding.Type == "MixedAlbumContent" && finding.AlbumId == parent.Id);
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            _fotoRepository.Verify(repository => repository.CreatePhoto(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenPhotoIsMissingAndStrategyIsKeepAndReport_KeepsEntityAndReportsDegradedStatus()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion" };
            var photo = new Foto { Id = Guid.NewGuid(), AlbumId = album.Id, FileName = "Missing.jpg" };
            album.Photos.Add(photo);
            Directory.CreateDirectory(Path.Combine(_rootPath, album.Path));
            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);

            // Act
            var report = await _service.AmendDirectoryTree();

            // Assert
            report.Status.Should().Be(AlbumSyncStatus.Degraded);
            report.MissingPhotos.Should().Be(1);
            report.PhotosDeleted.Should().Be(0);
            album.Photos.Should().Contain(photo);
            _fotoRepository.Verify(repository => repository.Delete(It.IsAny<Guid>()), Times.Never);
            _reportStore.Verify(store => store.Write(It.Is<AlbumSyncReport>(value => value.Status == AlbumSyncStatus.Degraded)), Times.Once);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenPhotoIsMissingAndDeletionIsEnabled_DeletesEntityBeforeCheckingAlbumKind()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "FairyTales 2021", Path = "FairyTales-2021" };
            var photo = new Foto { Id = Guid.NewGuid(), AlbumId = album.Id, FileName = "Page.jpg" };
            album.Photos.Add(photo);
            Directory.CreateDirectory(Path.Combine(_rootPath, album.Path, "Impaginato"));
            var child = new Album { Id = Guid.NewGuid(), Name = "Impaginato", Path = "Impaginato", ParentId = album.Id, Parent = album };

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);
            _albumRepository.Setup(repository => repository.CreateAlbum("Impaginato", album.Id, "Impaginato")).ReturnsAsync(child);
            var options = Options.Create(new PortfolioAlbumOptions
            {
                RootPath = _rootPath,
                MissingPhotoStrategy = MissingPhotoStrategy.DeleteDatabaseEntity,
                MaxMissingPhotoDeletions = 1
            });
            var service = new AlbumService(_albumRepository.Object, _fotoRepository.Object, options, _reportStore.Object, _logger.Object);

            // Act
            var report = await service.AmendDirectoryTree();

            // Assert
            report.Status.Should().Be(AlbumSyncStatus.Healthy);
            report.PhotosDeleted.Should().Be(1);
            report.AlbumsCreated.Should().Be(1);
            album.Photos.Should().BeEmpty();
            _fotoRepository.Verify(repository => repository.Delete(photo.Id), Times.Once);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenMissingPhotosExceedDeletionLimit_AbortsBeforeDeletingAnyEntity()
        {
            // Arrange
            var album = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion" };
            album.Photos.Add(new Foto { Id = Guid.NewGuid(), AlbumId = album.Id, FileName = "Missing-1.jpg" });
            album.Photos.Add(new Foto { Id = Guid.NewGuid(), AlbumId = album.Id, FileName = "Missing-2.jpg" });
            Directory.CreateDirectory(Path.Combine(_rootPath, album.Path));
            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([album]);
            var options = Options.Create(new PortfolioAlbumOptions
            {
                RootPath = _rootPath,
                MissingPhotoStrategy = MissingPhotoStrategy.DeleteDatabaseEntity,
                MaxMissingPhotoDeletions = 1
            });
            var service = new AlbumService(_albumRepository.Object, _fotoRepository.Object, options, _reportStore.Object, _logger.Object);

            // Act
            var action = async () => await service.AmendDirectoryTree();

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Found 2 missing photos, exceeding the configured deletion limit of 1. No photo was deleted.");
            _fotoRepository.Verify(repository => repository.Delete(It.IsAny<Guid>()), Times.Never);
            _reportStore.Verify(store => store.Write(It.Is<AlbumSyncReport>(value => value.Status == AlbumSyncStatus.Unhealthy)), Times.Once);
        }

        #endregion

        #region BeginOperation

        [Fact]
        public async Task BeginOperation_WhenCalled_BeginsRepositoryTransaction()
        {
            // Arrange
            var transaction = new Mock<IPersistenceTransaction>();

            _albumRepository.Setup(repository => repository.BeginTransaction()).ReturnsAsync(transaction.Object);

            // Act
            await using var operation = await _service.BeginOperation();

            // Assert
            operation.Should().BeOfType<ApplicationOperation>();
            _albumRepository.Verify(repository => repository.BeginTransaction(), Times.Once);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, true);
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

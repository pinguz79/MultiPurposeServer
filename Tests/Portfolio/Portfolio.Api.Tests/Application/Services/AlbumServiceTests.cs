using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Portfolio.Api.Application.Operations;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Infrastructure.Persistence.Repositories;
using Portfolio.Api.Infrastructure.Persistence.Transactions;
using Portfolio.Api.Services;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Application.Services
{
    public class AlbumServiceTests : IDisposable
    {
        private readonly Mock<IAlbumRepository> _albumRepository;
        private readonly Mock<IFotoRepository> _fotoRepository;
        private readonly string _rootPath;
        private readonly AlbumService _service;

        public AlbumServiceTests()
        {
            _albumRepository = new Mock<IAlbumRepository>();
            _fotoRepository = new Mock<IFotoRepository>();
            _rootPath = Path.Combine(Path.GetTempPath(), "Portfolio.Api.ServiceTests", Guid.NewGuid().ToString("N"));

            var options = Options.Create(new PortfolioAlbumOptions { RootPath = _rootPath });
            _service = new AlbumService(_albumRepository.Object, _fotoRepository.Object, options);
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
        public async Task AmendDirectoryTree_WhenAlbumContainsPhotosAndChildDirectory_ThrowsInvalidOperationException()
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
            var action = async () => await _service.AmendDirectoryTree();

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Album 'Fashion' cannot contain both child albums and photos.");
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            _fotoRepository.Verify(repository => repository.CreatePhoto(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _albumRepository.Verify(repository => repository.SaveIfRequired(), Times.Never);
        }

        [Fact]
        public async Task AmendDirectoryTree_WhenAlbumContainsChildrenAndPhotoFile_ThrowsInvalidOperationException()
        {
            // Arrange
            var parent = new Album { Id = Guid.NewGuid(), Name = "Fashion", Path = "Fashion", ParentId = null };
            var child = new Album { Id = Guid.NewGuid(), Name = "Milano", Path = "Milano", ParentId = parent.Id };
            var parentPath = Path.Combine(_rootPath, "Fashion");

            Directory.CreateDirectory(parentPath);
            await File.WriteAllBytesAsync(Path.Combine(parentPath, "Photo_001.jpg"), []);

            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync([parent, child]);

            // Act
            var action = async () => await _service.AmendDirectoryTree();

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Album 'Fashion' cannot contain both child albums and photos.");
            _albumRepository.Verify(repository => repository.CreateAlbum(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            _fotoRepository.Verify(repository => repository.CreatePhoto(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _albumRepository.Verify(repository => repository.SaveIfRequired(), Times.Never);
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

using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Portfolio.Api.Application.Options;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Tests.Infrastructure;
using Portfolio.Data.Models;

namespace Portfolio.Api.Tests.Application.Services
{
    public class MediaServiceTests : IDisposable
    {
        private readonly TemporaryDirectory _temporaryDirectory;
        private readonly Mock<IFotoService> _fotoService;
        private readonly Mock<IImageResizer> _imageResizer;
        private readonly PortfolioMediaOptions _options;
        private readonly string _originalsRoot;
        private readonly string _cacheRoot;
        private readonly MediaService _service;

        public MediaServiceTests()
        {
            _temporaryDirectory = new TemporaryDirectory();
            _fotoService = new Mock<IFotoService>();
            _imageResizer = new Mock<IImageResizer>();

            _options = new PortfolioMediaOptions
            {
                OriginalsRoot = "originals",
                CacheRoot = "cache",
                CoverWidth = 360,
                CoverHeight = 240,
                ThumbnailWidth = 320,
                ThumbnailHeight = 200,
                ImageWidth = 800,
                ImageHeight = 1200
            };

            _originalsRoot = _temporaryDirectory.Combine(_options.OriginalsRoot);
            _cacheRoot = _temporaryDirectory.Combine(_options.CacheRoot);

            Directory.CreateDirectory(_originalsRoot);
            Directory.CreateDirectory(_cacheRoot);

            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(item => item.ContentRootPath).Returns(_temporaryDirectory.Path);

            _service = new MediaService(_fotoService.Object, _imageResizer.Object, Options.Create(_options), environment.Object);
        }

        [Fact]
        public async Task GetImagePhoto_WhenPhotoDoesNotExist_ReturnsNullWithoutResizing()
        {
            // Arrange
            var photoId = Guid.NewGuid();

            _fotoService.Setup(service => service.GetById(photoId)).ReturnsAsync((Foto?)null);

            // Act
            var result = await _service.GetImagePhoto(photoId);

            // Assert
            result.Should().BeNull();
            _fotoService.Verify(service => service.GetById(photoId), Times.Once);
            VerifyResizeWasNeverCalled();
        }

        [Fact]
        public async Task GetImagePhoto_WhenRelativePathIsEmpty_ReturnsNullWithoutResizing()
        {
            // Arrange
            var photo = CreatePhoto(string.Empty, string.Empty);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);

            // Act
            var result = await _service.GetImagePhoto(photo.Id);

            // Assert
            photo.RelativePath.Should().BeEmpty();
            result.Should().BeNull();
            VerifyResizeWasNeverCalled();
        }

        [Fact]
        public async Task GetImagePhoto_WhenOriginalFileDoesNotExist_ReturnsNullWithoutResizing()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);

            // Act
            var result = await _service.GetImagePhoto(photo.Id);

            // Assert
            result.Should().BeNull();
            VerifyResizeWasNeverCalled();
        }

        [Fact]
        public async Task GetImagePhoto_WhenCacheDoesNotExist_ResizesUsingImageProfile()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");
            var sourcePath = await CreateOriginalFile(photo);
            var expectedCachePath = GetExpectedCachePath(photo.Id, "images", _options.ImageWidth, _options.ImageHeight);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);
            _imageResizer.Setup(resizer => resizer.Resize(sourcePath, expectedCachePath, _options.ImageWidth, _options.ImageHeight, false)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetImagePhoto(photo.Id);

            // Assert
            result.Should().BeEquivalentTo(new { FilePath = expectedCachePath, ContentType = "image/jpeg" });
            _imageResizer.Verify(resizer => resizer.Resize(sourcePath, expectedCachePath, _options.ImageWidth, _options.ImageHeight, false), Times.Once);
        }

        [Fact]
        public async Task GetThumbnailPhoto_WhenCacheDoesNotExist_ResizesUsingThumbnailProfileWithoutCropping()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");
            var sourcePath = await CreateOriginalFile(photo);
            var expectedCachePath = GetExpectedCachePath(photo.Id, "thumbnails", _options.ThumbnailWidth, _options.ThumbnailHeight);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);
            _imageResizer.Setup(resizer => resizer.Resize(sourcePath, expectedCachePath, _options.ThumbnailWidth, _options.ThumbnailHeight, false)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetThumbnailPhoto(photo.Id);

            // Assert
            result.Should().BeEquivalentTo(new { FilePath = expectedCachePath, ContentType = "image/jpeg" });
            _imageResizer.Verify(resizer => resizer.Resize(sourcePath, expectedCachePath, _options.ThumbnailWidth, _options.ThumbnailHeight, false), Times.Once);
        }

        [Fact]
        public async Task GetCoverPhoto_WhenCacheDoesNotExist_ResizesUsingCoverProfileWithCropping()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");
            var sourcePath = await CreateOriginalFile(photo);
            var expectedCachePath = GetExpectedCachePath(photo.Id, "covers", _options.CoverWidth, _options.CoverHeight);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);
            _imageResizer.Setup(resizer => resizer.Resize(sourcePath, expectedCachePath, _options.CoverWidth, _options.CoverHeight, true)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetCoverPhoto(photo.Id);

            // Assert
            result.Should().BeEquivalentTo(new { FilePath = expectedCachePath, ContentType = "image/jpeg" });
            _imageResizer.Verify(resizer => resizer.Resize(sourcePath, expectedCachePath, _options.CoverWidth, _options.CoverHeight, true), Times.Once);
        }

        [Fact]
        public async Task GetImagePhoto_WhenCacheAlreadyExists_ReturnsCacheWithoutResizing()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");
            await CreateOriginalFile(photo);

            var cachePath = GetExpectedCachePath(photo.Id, "images", _options.ImageWidth, _options.ImageHeight);
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            await File.WriteAllBytesAsync(cachePath, [1, 2, 3]);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);

            // Act
            var result = await _service.GetImagePhoto(photo.Id);

            // Assert
            result.Should().BeEquivalentTo(new { FilePath = cachePath, ContentType = "image/jpeg" });
            VerifyResizeWasNeverCalled();
        }

        [Fact]
        public async Task GetThumbnailPhoto_WhenCalledTwiceAndFirstResizeCreatesCache_ResizesOnlyOnce()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");
            var sourcePath = await CreateOriginalFile(photo);
            var cachePath = GetExpectedCachePath(photo.Id, "thumbnails", _options.ThumbnailWidth, _options.ThumbnailHeight);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);
            _imageResizer.Setup(resizer => resizer.Resize(sourcePath, cachePath, _options.ThumbnailWidth, _options.ThumbnailHeight, false)).Returns(async () =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
                await File.WriteAllBytesAsync(cachePath, [1, 2, 3]);
            });

            // Act
            var firstResult = await _service.GetThumbnailPhoto(photo.Id);
            var secondResult = await _service.GetThumbnailPhoto(photo.Id);

            // Assert
            firstResult.Should().BeEquivalentTo(secondResult);
            _fotoService.Verify(service => service.GetById(photo.Id), Times.Exactly(2));
            _imageResizer.Verify(resizer => resizer.Resize(sourcePath, cachePath, _options.ThumbnailWidth, _options.ThumbnailHeight, false), Times.Once);
        }

        [Fact]
        public async Task GetImagePhoto_WhenPhotoBelongsToNestedAlbum_UsesComputedRelativePath()
        {
            // Arrange
            var photo = CreatePhoto("Fashion/Milano/Studio", "Photo_001.jpg");
            var sourcePath = await CreateOriginalFile(photo);
            var cachePath = GetExpectedCachePath(photo.Id, "images", _options.ImageWidth, _options.ImageHeight);

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);
            _imageResizer.Setup(resizer => resizer.Resize(sourcePath, cachePath, _options.ImageWidth, _options.ImageHeight, false)).Returns(Task.CompletedTask);

            // Act
            await _service.GetImagePhoto(photo.Id);

            // Assert
            photo.RelativePath.Should().Be(Path.Combine("Fashion", "Milano", "Studio", "Photo_001.jpg"));
            _imageResizer.Verify(resizer => resizer.Resize(sourcePath, cachePath, _options.ImageWidth, _options.ImageHeight, false), Times.Once);
        }

        [Fact]
        public async Task GetImagePhoto_WhenComputedRelativePathEscapesOriginalsRoot_ThrowsInvalidOperationException()
        {
            // Arrange
            var photo = CreatePhoto("..", "Secret.jpg");

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);

            // Act
            var action = async () => await _service.GetImagePhoto(photo.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid media path.");
            VerifyResizeWasNeverCalled();
        }

        [Fact]
        public async Task GetImagePhoto_WhenResizerThrows_PropagatesException()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");
            var sourcePath = await CreateOriginalFile(photo);
            var cachePath = GetExpectedCachePath(photo.Id, "images", _options.ImageWidth, _options.ImageHeight);
            var expectedException = new InvalidOperationException("Resize failed.");

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);
            _imageResizer.Setup(resizer => resizer.Resize(sourcePath, cachePath, _options.ImageWidth, _options.ImageHeight, false)).ThrowsAsync(expectedException);

            // Act
            var action = async () => await _service.GetImagePhoto(photo.Id);

            // Assert
            var exception = await action.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Should().BeSameAs(expectedException);
        }

        [Fact]
        public async Task GetImagePhoto_WhenCalled_RequestsPhotoOnlyOnce()
        {
            // Arrange
            var photo = CreatePhoto("Fashion", "Photo_001.jpg");

            _fotoService.Setup(service => service.GetById(photo.Id)).ReturnsAsync(photo);

            // Act
            await _service.GetImagePhoto(photo.Id);

            // Assert
            _fotoService.Verify(service => service.GetById(photo.Id), Times.Once);
        }

        public void Dispose()
        {
            _temporaryDirectory.Dispose();
            GC.SuppressFinalize(this);
        }

        private static Foto CreatePhoto(string albumPath, string fileName)
        {
            var album = CreateAlbumHierarchy(albumPath);

            return new Foto
            {
                Id = Guid.NewGuid(),
                AlbumId = album.Id,
                Album = album,
                FileName = fileName
            };
        }

        private static Album CreateAlbumHierarchy(string albumPath)
        {
            if (string.IsNullOrEmpty(albumPath))
            {
                return new Album { Id = Guid.NewGuid(), Name = string.Empty, Path = string.Empty };
            }

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

            return parent!;
        }

        private async Task<string> CreateOriginalFile(Foto photo)
        {
            var filePath = Path.Combine(_originalsRoot, photo.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllBytesAsync(filePath, [1, 2, 3]);
            return Path.GetFullPath(filePath);
        }

        private string GetExpectedCachePath(Guid photoId, string folder, int width, int height)
        {
            return Path.Combine(_cacheRoot, folder, $"{photoId}_{width}x{height}.jpg");
        }

        private void VerifyResizeWasNeverCalled()
        {
            _imageResizer.Verify(resizer => resizer.Resize(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
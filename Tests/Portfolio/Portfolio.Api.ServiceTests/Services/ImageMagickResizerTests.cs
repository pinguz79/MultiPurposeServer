using FluentAssertions;
using ImageMagick;
using Portfolio.Api.ServiceTests.Infrastructure;
using Portfolio.Api.Services;

namespace Portfolio.Api.ServiceTests.Services
{
    public class ImageMagickResizerTests : IDisposable
    {
        private readonly TemporaryDirectory _temporaryDirectory;
        private readonly ImageMagickResizer _resizer;

        public ImageMagickResizerTests()
        {
            _temporaryDirectory = new TemporaryDirectory();
            _resizer = new ImageMagickResizer();
        }

        [Fact]
        public async Task Resize_WhenDestinationDirectoryDoesNotExist_CreatesDirectoryAndImage()
        {
            // Arrange
            var sourcePath = CreateSourceImage("source.jpg", 1200, 800);
            var destinationPath = _temporaryDirectory.Combine("cache", "images", "destination.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            Directory.Exists(Path.GetDirectoryName(destinationPath)).Should().BeTrue();
            File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        public async Task Resize_WhenCropIsFalseAndSourceIsLandscape_PreservesAspectRatio()
        {
            // Arrange
            var sourcePath = CreateSourceImage("landscape.jpg", 1200, 800);
            var destinationPath = _temporaryDirectory.Combine("cache", "landscape.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(300);
            result.Height.Should().Be(200);
        }

        [Fact]
        public async Task Resize_WhenCropIsFalseAndSourceIsPortrait_PreservesAspectRatio()
        {
            // Arrange
            var sourcePath = CreateSourceImage("portrait.jpg", 800, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "portrait.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(200);
            result.Height.Should().Be(300);
        }

        [Fact]
        public async Task Resize_WhenCropIsFalseAndSourceIsSquare_ResizesToRequestedBounds()
        {
            // Arrange
            var sourcePath = CreateSourceImage("square.jpg", 1000, 1000);
            var destinationPath = _temporaryDirectory.Combine("cache", "square.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(300);
            result.Height.Should().Be(300);
        }

        [Fact]
        public async Task Resize_WhenCropIsFalseAndSourceIsSmaller_DoesNotEnlargeImage()
        {
            // Arrange
            var sourcePath = CreateSourceImage("small.jpg", 120, 80);
            var destinationPath = _temporaryDirectory.Combine("cache", "small.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(120);
            result.Height.Should().Be(80);
        }

        [Fact]
        public async Task Resize_WhenCropIsTrueAndSourceIsLandscape_CreatesExactRequestedDimensions()
        {
            // Arrange
            var sourcePath = CreateSourceImage("landscape-cover.jpg", 1200, 800);
            var destinationPath = _temporaryDirectory.Combine("cache", "landscape-cover.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 360, 240, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(360);
            result.Height.Should().Be(240);
        }

        [Fact]
        public async Task Resize_WhenCropIsTrueAndSourceIsPortrait_CreatesExactRequestedDimensions()
        {
            // Arrange
            var sourcePath = CreateSourceImage("portrait-cover.jpg", 800, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "portrait-cover.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 360, 240, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(360);
            result.Height.Should().Be(240);
        }

        [Fact]
        public async Task Resize_WhenCompleted_WritesJpegImage()
        {
            // Arrange
            var sourcePath = CreateSourceImage("source.png", 800, 600, MagickFormat.Png);
            var destinationPath = _temporaryDirectory.Combine("cache", "destination.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 400, 300, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Format.Should().Be(MagickFormat.Jpeg);
        }

        [Fact]
        public async Task Resize_WhenDestinationFileAlreadyExists_OverwritesFile()
        {
            // Arrange
            var sourcePath = CreateSourceImage("source.jpg", 1200, 800);
            var destinationPath = _temporaryDirectory.Combine("cache", "destination.jpg");

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            await File.WriteAllBytesAsync(destinationPath, [1, 2, 3]);

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(300);
            result.Height.Should().Be(200);
        }

        [Fact]
        public async Task Resize_WhenDestinationPathIsOccupiedByDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            var sourcePath = CreateSourceImage("source.jpg", 1200, 800);
            var destinationPath = _temporaryDirectory.Combine("cache", "destination.jpg");

            Directory.CreateDirectory(destinationPath);

            // Act
            var action = async () => await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{destinationPath}*");
        }

        [Fact]
        public async Task Resize_WhenDestinationHasNoDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            var sourcePath = CreateSourceImage("source.jpg", 1200, 800);
            const string destinationPath = "destination.jpg";

            // Act
            var action = async () => await _resizer.Resize(sourcePath, destinationPath, 300, 300, false);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Impossibile determinare la directory della cache*");
        }

        [Fact]
        public async Task Resize_WhenDifferentDimensionsAreRequested_UsesRequestedDimensions()
        {
            // Arrange
            var sourcePath = CreateSourceImage("source.jpg", 1600, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "custom.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 800, 1200, false);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Width.Should().Be(800);
            result.Height.Should().Be(600);
        }

        public void Dispose()
        {
            _temporaryDirectory.Dispose();
            GC.SuppressFinalize(this);
        }

        private string CreateSourceImage(string fileName, uint width, uint height, MagickFormat format = MagickFormat.Jpeg)
        {
            var sourcePath = _temporaryDirectory.Combine("originals", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);

            using var image = new MagickImage(MagickColors.Red, width, height);
            image.Format = format;
            image.Write(sourcePath);

            return sourcePath;
        }
    }
}
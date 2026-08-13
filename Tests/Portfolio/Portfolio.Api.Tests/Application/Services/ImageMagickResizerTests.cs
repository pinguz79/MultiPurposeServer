using FluentAssertions;

using ImageMagick;

using Portfolio.Api.Application.Models;
using Portfolio.Api.Application.Services;
using Portfolio.Api.Services;
using Portfolio.Api.Tests.Infrastructure;

namespace Portfolio.Api.Tests.Application.Services
{
    public class ImageMagickResizerTests : IDisposable
    {
        private readonly TemporaryDirectory _temporaryDirectory;
        private readonly ImageMagickResizer _resizer;

        public ImageMagickResizerTests()
        {
            _temporaryDirectory = new TemporaryDirectory();
            _resizer = new ImageMagickResizer(new NoCropFocusDetector());
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
        public async Task Resize_WhenCropIsTrueAndSourceIsPortrait_CropsFromTop()
        {
            // Arrange
            var sourcePath = CreateHorizontalSplitSourceImage("portrait-top-crop.png", 800, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "portrait-top-crop.jpg");

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 360, 240, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            using var expected = new MagickImage(MagickColors.Red, 360, 240);
            result.Compare(expected, ErrorMetric.RootMeanSquared).Should().BeLessThan(0.01);
        }

        [Fact]
        public async Task Resize_WhenCropFocusIsDetected_CentersCropAroundFocus()
        {
            // Arrange
            var sourcePath = CreateHorizontalSplitSourceImage("portrait-smart-crop.png", 800, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "portrait-smart-crop.jpg");
            var resizer = new ImageMagickResizer(new FixedCropFocusDetector(new CropFocus(0.35, 0.72, 0.30, 0.20)));

            // Act
            await resizer.Resize(sourcePath, destinationPath, 360, 240, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            using var expected = new MagickImage(MagickColors.Blue, 360, 240);
            result.Compare(expected, ErrorMetric.RootMeanSquared).Should().BeLessThan(0.01);
        }

        [Fact]
        public async Task Resize_WhenCropFocusIsAlreadySafe_KeepsDeterministicFallback()
        {
            // Arrange
            var sourcePath = CreateHorizontalSplitSourceImage("portrait-safe-focus.png", 800, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "portrait-safe-focus.jpg");
            var resizer = new ImageMagickResizer(new FixedCropFocusDetector(new CropFocus(0.35, 0.12, 0.30, 0.15)));

            // Act
            await resizer.Resize(sourcePath, destinationPath, 360, 240, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            using var expected = new MagickImage(MagickColors.Red, 360, 240);
            result.Compare(expected, ErrorMetric.RootMeanSquared).Should().BeLessThan(0.01);
        }

        [Fact]
        public async Task Resize_WhenCropFocusIsCloseToFallbackEdge_RepositionsCropWithContext()
        {
            // Arrange
            var sourcePath = CreateHorizontalSplitSourceImage("portrait-edge-focus.png", 800, 1200);
            var destinationPath = _temporaryDirectory.Combine("cache", "portrait-edge-focus.jpg");
            var resizer = new ImageMagickResizer(new FixedCropFocusDetector(new CropFocus(0.45, 0.30, 0.10, 0.075)));

            using var expected = new MagickImage(sourcePath);
            expected.Crop(new MagickGeometry(0, 202, 800, 533));
            expected.Resize(360, 240);

            // Act
            await resizer.Resize(sourcePath, destinationPath, 360, 240, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Compare(expected, ErrorMetric.RootMeanSquared).Should().BeLessThan(0.03);
        }

        [Fact]
        public async Task Resize_WhenCropIsTrueAndSourceIsLandscape_KeepsCenteredCrop()
        {
            // Arrange
            var sourcePath = CreateHorizontalSplitSourceImage("landscape-center-crop.png", 1200, 800);
            var destinationPath = _temporaryDirectory.Combine("cache", "landscape-center-crop.jpg");

            using var expected = new MagickImage(sourcePath);
            expected.Resize(new MagickGeometry(600, 200) { FillArea = true });
            expected.Extent(600, 200, Gravity.Center);

            // Act
            await _resizer.Resize(sourcePath, destinationPath, 600, 200, true);

            // Assert
            using var result = new MagickImage(destinationPath);
            result.Compare(expected, ErrorMetric.RootMeanSquared).Should().BeLessThan(0.02);
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

        private string CreateHorizontalSplitSourceImage(string fileName, uint width, uint height)
        {
            var sourcePath = _temporaryDirectory.Combine("originals", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);

            using var image = new MagickImage(MagickColors.Red, width, height);
            using var bottomHalf = new MagickImage(MagickColors.Blue, width, height / 2);
            image.Composite(bottomHalf, Gravity.South, CompositeOperator.Over);
            image.Format = MagickFormat.Png;
            image.Write(sourcePath);

            return sourcePath;
        }
    }
}

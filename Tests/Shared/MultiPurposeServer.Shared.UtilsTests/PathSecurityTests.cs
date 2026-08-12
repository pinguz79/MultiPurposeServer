using FluentAssertions;

using MultiPurposeServer.Shared.Utils;

namespace MultiPurposeServer.Shared.UtilsTests
{
    public class PathSecurityTests : IDisposable
    {
        private readonly string _rootPath;

        public PathSecurityTests()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), "PathSecurityTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_rootPath);
        }

        [Fact]
        public void ResolveRootPath_WhenConfiguredPathIsAbsolute_ReturnsAbsolutePath()
        {
            // Arrange
            var configuredPath = Path.Combine(_rootPath, "Originals");

            // Act
            var result = PathSecurity.ResolveRootPath("Ignored", configuredPath);

            // Assert
            result.Should().Be(Path.GetFullPath(configuredPath));
        }

        [Fact]
        public void ResolveRootPath_WhenConfiguredPathIsRelative_CombinesWithContentRoot()
        {
            // Arrange
            const string configuredPath = "Portfolio/Originals";

            // Act
            var result = PathSecurity.ResolveRootPath(_rootPath, configuredPath);

            // Assert
            result.Should().Be(Path.GetFullPath(Path.Combine(_rootPath, configuredPath)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ResolveRootPath_WhenConfiguredPathIsEmpty_ThrowsInvalidOperationException(string configuredPath)
        {
            // Arrange

            // Act
            var action = () => PathSecurity.ResolveRootPath(_rootPath, configuredPath);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetSafePath_WhenRelativePathIsInsideRoot_ReturnsFullPath()
        {
            // Arrange
            var relativePath = Path.Combine("Fashion", "Photo.jpg");

            // Act
            var result = PathSecurity.GetSafePath(_rootPath, relativePath);

            // Assert
            result.Should().Be(Path.GetFullPath(Path.Combine(_rootPath, relativePath)));
        }

        [Fact]
        public void GetSafePath_WhenRelativePathEscapesRoot_ThrowsInvalidOperationException()
        {
            // Arrange
            var relativePath = Path.Combine("..", "Secret", "Photo.jpg");

            // Act
            var action = () => PathSecurity.GetSafePath(_rootPath, relativePath);

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("Invalid media path.");
        }

        [Fact]
        public void GetSafePath_WhenRelativePathResolvesToRootItself_ThrowsInvalidOperationException()
        {
            // Arrange
            const string relativePath = ".";

            // Act
            var action = () => PathSecurity.GetSafePath(_rootPath, relativePath);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void GetSafePath_WhenRootIsEmpty_ThrowsArgumentException(string root)
        {
            // Arrange

            // Act
            var action = () => PathSecurity.GetSafePath(root, "Photo.jpg");

            // Assert
            action.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("root");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void GetSafePath_WhenRelativePathIsEmpty_ThrowsArgumentException(string relativePath)
        {
            // Arrange

            // Act
            var action = () => PathSecurity.GetSafePath(_rootPath, relativePath);

            // Assert
            action.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("relativePath");
        }

        public void Dispose()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, true);
            }

            GC.SuppressFinalize(this);
        }
    }
}

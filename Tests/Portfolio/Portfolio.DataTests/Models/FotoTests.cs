using FluentAssertions;

using MultiPurposeServer.Shared.Utils;

using Portfolio.Data.Models;

namespace Portfolio.DataTests.Models
{
    public class FotoTests
    {
        #region Path e nome

        [Fact]
        public void AlbumName_WhenAlbumExists_ReturnsAlbumName()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "Portrait.jpg");

            // Act
            var result = photo.AlbumName;

            // Assert
            result.Should().Be("Fashion");
        }

        [Fact]
        public void RelativePath_WhenAlbumIsRoot_CombinesAlbumPathAndFileName()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "Portrait.jpg");

            // Act
            var result = photo.RelativePath;

            // Assert
            result.Should().Be(Path.Combine("fashion", "Portrait.jpg"));
        }

        [Fact]
        public void RelativePath_WhenAlbumHasAncestors_CombinesEntireAlbumPathAndFileName()
        {
            // Arrange
            var root = CreateAlbum("Portfolio", "portfolio");
            var album = CreateAlbum("Fashion", "fashion", root);
            var photo = CreatePhoto(album, "Portrait.jpg");

            // Act
            var result = photo.RelativePath;

            // Assert
            result.Should().Be(Path.Combine("portfolio", "fashion", "Portrait.jpg"));
        }

        [Fact]
        public void PhotoName_WhenDescriptionIsPresent_ReturnsDescription()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "portraitDiModa.jpg");
            photo.Description = "Ritratto editoriale";

            // Act
            var result = photo.PhotoName;

            // Assert
            result.Should().Be("Ritratto editoriale");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void PhotoName_WhenDescriptionIsMissing_ReturnsHumanizedFileName(string? description)
        {
            // Arrange
            const string fileName = "ritrattoDiModa.jpg";
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, fileName);
            photo.Description = description;

            // Act
            var result = photo.PhotoName;

            // Assert
            result.Should().Be(new FileNameFormatter(fileName).HumanizedName);
        }

        [Fact]
        public void AltText_WhenCalled_ReturnsPhotoName()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "Portrait.jpg");
            photo.Description = "Ritratto editoriale";

            // Act
            var result = photo.AltText;

            // Assert
            result.Should().Be(photo.PhotoName);
        }

        #endregion

        #region SelectionCode

        [Fact]
        public void SelectionCode_WhenFileNameContainsSelectionCode_ReturnsCalculatedCode()
        {
            // Arrange
            const string fileName = "Portrait_001A.jpg";
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, fileName);

            // Act
            var result = photo.SelectionCode;

            // Assert
            result.Should().Be(new NamingConventions(fileName).SelectionCode);
        }

        [Fact]
        public void SelectionCode_WhenFileNameDoesNotContainSelectionCode_ReturnsNull()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "Portrait.jpg");

            // Act
            var result = photo.SelectionCode;

            // Assert
            result.Should().BeNull();
        }

        #endregion

        [Fact]
        public void ToString_WhenCalled_ReturnsFileNameAndAlbumName()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "Portrait.jpg");

            // Act
            var result = photo.ToString();

            // Assert
            result.Should().Be("Portrait.jpg - Fashion");
        }


        #region Helper

        private static Album CreateAlbum(string name, string path, Album? parent = null)
        {
            return new Album
            {
                Id = Guid.NewGuid(),
                Name = name,
                Path = path,
                ParentId = parent?.Id,
                Parent = parent
            };
        }

        private static Foto CreatePhoto(Album album, string fileName)
        {
            return new Foto
            {
                Id = Guid.NewGuid(),
                AlbumId = album.Id,
                Album = album,
                FileName = fileName
            };
        }
        #endregion

    }
}

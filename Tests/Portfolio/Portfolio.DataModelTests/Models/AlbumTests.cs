using FluentAssertions;

using Portfolio.DataModel.Enums;
using Portfolio.DataModel.Models;

namespace Portfolio.DataModelTests.Models
{
    public class AlbumTests
    {
        #region Path e nome

        [Fact]
        public void FullPath_WhenAlbumIsRoot_ReturnsOwnPath()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");

            // Act
            var result = album.FullPath;

            // Assert
            result.Should().Be("fashion");
        }

        [Fact]
        public void FullPath_WhenAlbumHasParent_CombinesParentAndOwnPath()
        {
            // Arrange
            var parent = CreateAlbum("Fashion", "fashion");
            var album = CreateAlbum("Milano", "milano", parent);

            // Act
            var result = album.FullPath;

            // Assert
            result.Should().Be(Path.Combine("fashion", "milano"));
        }

        [Fact]
        public void FullPath_WhenAlbumHasMultipleAncestors_CombinesEntireHierarchy()
        {
            // Arrange
            var root = CreateAlbum("Portfolio", "portfolio");
            var parent = CreateAlbum("Fashion", "fashion", root);
            var album = CreateAlbum("Milano", "milano", parent);

            // Act
            var result = album.FullPath;

            // Assert
            result.Should().Be(Path.Combine("portfolio", "fashion", "milano"));
        }

        [Fact]
        public void FullName_WhenAlbumIsRoot_ReturnsOwnName()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");

            // Act
            var result = album.FullName;

            // Assert
            result.Should().Be("Fashion");
        }

        [Fact]
        public void FullName_WhenAlbumHasMultipleAncestors_CombinesEntireHierarchy()
        {
            // Arrange
            var root = CreateAlbum("Portfolio", "portfolio");
            var parent = CreateAlbum("Fashion", "fashion", root);
            var album = CreateAlbum("Milano", "milano", parent);

            // Act
            var result = album.FullName;

            // Assert
            result.Should().Be(Path.Combine("Portfolio", "Fashion", "Milano"));
        }

        #endregion

        #region Children e Photos

        [Fact]
        public void ChildrenCounter_WhenAlbumHasChildren_ReturnsChildrenCount()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Children =
            [
                CreateAlbum("Milano", "milano", album),
                CreateAlbum("Torino", "torino", album)
            ];

            // Act
            var result = album.ChildrenCounter;

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public void PhotosCounter_WhenAlbumHasPhotos_ReturnsPhotosCount()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Photos =
            [
                CreatePhoto(album, "Photo_001.jpg"),
                CreatePhoto(album, "Photo_002.jpg")
            ];

            // Act
            var result = album.PhotosCounter;

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public void AllPhotos_WhenAlbumHasDirectPhotos_ReturnsDirectPhotos()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var firstPhoto = CreatePhoto(album, "Photo_001.jpg");
            var secondPhoto = CreatePhoto(album, "Photo_002.jpg");

            album.Photos = [firstPhoto, secondPhoto];

            // Act
            var result = album.AllPhotos;

            // Assert
            result.Should().BeEquivalentTo([firstPhoto, secondPhoto]);
        }

        [Fact]
        public void AllPhotos_WhenChildrenHavePhotos_ReturnsPhotosFromEntireHierarchy()
        {
            // Arrange
            var root = CreateAlbum("Portfolio", "portfolio");
            var child = CreateAlbum("Fashion", "fashion", root);
            var grandchild = CreateAlbum("Milano", "milano", child);

            var rootPhoto = CreatePhoto(root, "Root.jpg");
            var childPhoto = CreatePhoto(child, "Child.jpg");
            var grandchildPhoto = CreatePhoto(grandchild, "Grandchild.jpg");

            root.Photos = [rootPhoto];
            child.Photos = [childPhoto];
            grandchild.Photos = [grandchildPhoto];

            root.Children = [child];
            child.Children = [grandchild];

            // Act
            var result = root.AllPhotos;

            // Assert
            result.Should().BeEquivalentTo([rootPhoto, childPhoto, grandchildPhoto]);
        }

        [Fact]
        public void AllPhotos_WhenReadMultipleTimes_ReturnsCachedCollection()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Photos = [CreatePhoto(album, "Photo_001.jpg")];

            // Act
            var firstResult = album.AllPhotos;
            var secondResult = album.AllPhotos;

            // Assert
            secondResult.Should().BeSameAs(firstResult);
        }

        [Fact]
        public void AllPhotos_WhenSourceCollectionsChangeAfterFirstRead_PreservesMaterializedResult()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var firstPhoto = CreatePhoto(album, "Photo_001.jpg");
            var secondPhoto = CreatePhoto(album, "Photo_002.jpg");

            album.Photos = [firstPhoto];
            _ = album.AllPhotos;

            // Act
            album.Photos.Add(secondPhoto);
            var result = album.AllPhotos;

            // Assert
            result.Should().ContainSingle().Which.Should().BeSameAs(firstPhoto);
        }

        #endregion

        #region Cover

        [Fact]
        public void CoverImage_WhenHierarchyHasNoPhotos_ReturnsNull()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");

            // Act
            var result = album.CoverImage;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void CoverImage_WhenHierarchyHasOnePhoto_ReturnsThatPhoto()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var photo = CreatePhoto(album, "Photo_001.jpg");

            album.Photos = [photo];

            // Act
            var result = album.CoverImage;

            // Assert
            result.Should().BeSameAs(photo);
        }

        [Fact]
        public void CoverImage_WhenHierarchyHasMultiplePhotos_ReturnsPhotoFromAllPhotos()
        {
            // Arrange
            var root = CreateAlbum("Portfolio", "portfolio");
            var child = CreateAlbum("Fashion", "fashion", root);

            root.Photos = [CreatePhoto(root, "Root.jpg")];
            child.Photos = [CreatePhoto(child, "Child.jpg")];
            root.Children = [child];

            // Act
            var result = root.CoverImage;

            // Assert
            result.Should().NotBeNull();
            root.AllPhotos.Should().Contain(result);
        }

        [Fact]
        public void CoverImage_WhenReadMultipleTimes_ReturnsCachedPhoto()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Photos =
            [
                CreatePhoto(album, "Photo_001.jpg"),
                CreatePhoto(album, "Photo_002.jpg")
            ];

            // Act
            var firstResult = album.CoverImage;
            var secondResult = album.CoverImage;

            // Assert
            secondResult.Should().BeSameAs(firstResult);
        }

        #endregion

        #region ContentRating

        [Fact]
        public void ContentRating_WhenPhotoAlbumContainsStandardAndRestrictedPhotos_ReturnsPartiallyRestricted()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Photos =
            [
                CreatePhoto(album, "Standard.jpg"),
                CreatePhoto(album, "Restricted.jpg", PhotoContentRating.Restricted)
            ];

            // Act
            var result = album.ContentRating;

            // Assert
            result.Should().Be(AlbumContentRating.PartiallyRestricted);
        }

        [Fact]
        public void ContentRating_WhenAllDirectPhotosAreRestricted_ReturnsRestricted()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Photos =
            [
                CreatePhoto(album, "First.jpg", PhotoContentRating.Restricted),
                CreatePhoto(album, "Second.jpg", PhotoContentRating.Restricted)
            ];

            // Act
            var result = album.ContentRating;

            // Assert
            result.Should().Be(AlbumContentRating.Restricted);
        }

        [Fact]
        public void ContentRating_WhenRestrictedDescendantHasStandardCover_DoesNotRestrictParent()
        {
            // Arrange
            var parent = CreateAlbum("Calendari", "calendari");
            var child = CreateAlbum("2025", "2025", parent);
            child.Photos =
            [
                CreatePhoto(child, "Standard.jpg"),
                CreatePhoto(child, "Restricted.jpg", PhotoContentRating.Restricted)
            ];
            parent.Children = [child];

            // Act
            var result = parent.ContentRating;

            // Assert
            child.ContentRating.Should().Be(AlbumContentRating.PartiallyRestricted);
            result.Should().Be(AlbumContentRating.Standard);
        }

        [Fact]
        public void ContentRating_WhenDirectChildrenExposeStandardAndRestrictedCovers_ReturnsPartiallyRestricted()
        {
            // Arrange
            var parent = CreateAlbum("Calendari", "calendari");
            var standardChild = CreateAlbum("Standard", "standard", parent);
            standardChild.Photos = [CreatePhoto(standardChild, "Standard.jpg")];
            var restrictedChild = CreateAlbum("Restricted", "restricted", parent);
            restrictedChild.Photos = [CreatePhoto(restrictedChild, "Restricted.jpg", PhotoContentRating.Restricted)];
            parent.Children = [standardChild, restrictedChild];

            // Act
            var result = parent.ContentRating;

            // Assert
            result.Should().Be(AlbumContentRating.PartiallyRestricted);
        }

        [Fact]
        public void CoverImage_WhenStandardPhotoExists_NeverUsesRestrictedFallback()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var standardPhoto = CreatePhoto(album, "Standard.jpg");
            album.Photos =
            [
                CreatePhoto(album, "Restricted.jpg", PhotoContentRating.Restricted),
                standardPhoto
            ];

            // Act
            var result = album.CoverImage;

            // Assert
            result.Should().BeSameAs(standardPhoto);
        }

        [Fact]
        public void CoverImage_WhenOnlyRestrictedPhotosExist_UsesRestrictedFallback()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            var restrictedPhoto = CreatePhoto(album, "Restricted.jpg", PhotoContentRating.Restricted);
            album.Photos = [restrictedPhoto];

            // Act
            var result = album.CoverImage;

            // Assert
            result.Should().BeSameAs(restrictedPhoto);
        }

        [Fact]
        public void ToString_WhenCalled_ReturnsNameAndCounters()
        {
            // Arrange
            var album = CreateAlbum("Fashion", "fashion");
            album.Children = [CreateAlbum("Milano", "milano", album)];
            album.Photos =
            [
                CreatePhoto(album, "Photo_001.jpg"),
                CreatePhoto(album, "Photo_002.jpg")
            ];

            // Act
            var result = album.ToString();

            // Assert
            result.Should().Be("Fashion (Gallery, 1 - 2)");
        }

        #endregion

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

        private static Foto CreatePhoto(Album album, string fileName, PhotoContentRating contentRating = PhotoContentRating.Standard)
        {
            return new Foto
            {
                Id = Guid.NewGuid(),
                AlbumId = album.Id,
                Album = album,
                FileName = fileName,
                ContentRating = contentRating
            };
        }

        #endregion

        #region Kind

        [Fact]
        public void Kind_WhenParentIsNull_ReturnsGallery()
        {
            // Arrange
            var album = new Album
            {
                ParentId = null
            };

            // Act
            var result = album.Kind;

            // Assert
            result.Should().Be(AlbumKind.Gallery);
        }

        [Fact]
        public void Kind_WhenAlbumHasChildren_ReturnsCollection()
        {
            // Arrange
            var album = new Album
            {
                ParentId = Guid.NewGuid(),
                Children = [new Album()]
            };

            // Act
            var result = album.Kind;

            // Assert
            result.Should().Be(AlbumKind.Collection);
        }

        [Fact]
        public void Kind_WhenAlbumHasParentAndNoChildren_ReturnsPhotoAlbum()
        {
            // Arrange
            var album = new Album
            {
                ParentId = Guid.NewGuid()
            };

            // Act
            var result = album.Kind;

            // Assert
            result.Should().Be(AlbumKind.PhotoAlbum);
        }

        [Fact]
        public void Kind_WhenParentIsNullAndAlbumHasChildren_ReturnsGallery()
        {
            // Arrange
            var album = new Album
            {
                ParentId = null,
                Children = [new Album()]
            };

            // Act
            var result = album.Kind;

            // Assert
            result.Should().Be(AlbumKind.Gallery);
        }
        #endregion

    }
}

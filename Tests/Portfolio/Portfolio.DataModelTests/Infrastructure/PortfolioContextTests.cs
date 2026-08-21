using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Portfolio.DataModel;
using Portfolio.DataModel.Models;

namespace Portfolio.DataModelTests.Infrastructure
{
    public class PortfolioContextTests
    {
        #region Configurazione modello

        [Fact]
        public async Task Model_WhenCreated_ConfiguresAlbumParentRelationship()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            // Act
            var albumEntity = context.Model.FindEntityType(typeof(Album));
            var foreignKey = albumEntity!.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(Album));

            // Assert
            foreignKey.Properties.Should().ContainSingle().Which.Name.Should().Be(nameof(Album.ParentId));
            foreignKey.PrincipalKey.Properties.Should().ContainSingle().Which.Name.Should().Be(nameof(Album.Id));
            foreignKey.PrincipalToDependent!.Name.Should().Be(nameof(Album.Children));
            foreignKey.DependentToPrincipal!.Name.Should().Be(nameof(Album.Parent));
            foreignKey.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
        }

        [Fact]
        public async Task Model_WhenCreated_ConfiguresFotoAlbumRelationship()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            // Act
            var fotoEntity = context.Model.FindEntityType(typeof(Foto));
            var foreignKey = fotoEntity!.GetForeignKeys().Single();

            // Assert
            foreignKey.Properties.Should().ContainSingle().Which.Name.Should().Be(nameof(Foto.AlbumId));
            foreignKey.PrincipalEntityType.ClrType.Should().Be<Album>();
            foreignKey.PrincipalToDependent!.Name.Should().Be(nameof(Album.Photos));
            foreignKey.DependentToPrincipal!.Name.Should().Be(nameof(Foto.Album));
            foreignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        }

        [Fact]
        public async Task Model_WhenCreated_ConfiguresUniqueAlbumParentAndPathIndex()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            // Act
            var albumEntity = context.Model.FindEntityType(typeof(Album));
            var index = albumEntity!.GetIndexes().Single(item => item.Properties.Select(property => property.Name).SequenceEqual([nameof(Album.ParentId), nameof(Album.Path)]));

            // Assert
            index.IsUnique.Should().BeTrue();
        }

        [Fact]
        public async Task Model_WhenCreated_ConfiguresUniqueFotoAlbumAndFileNameIndex()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            // Act
            var fotoEntity = context.Model.FindEntityType(typeof(Foto));
            var index = fotoEntity!.GetIndexes().Single(item => item.Properties.Select(property => property.Name).SequenceEqual([nameof(Foto.AlbumId), nameof(Foto.FileName)]));

            // Assert
            index.IsUnique.Should().BeTrue();
        }

        [Fact]
        public async Task Model_WhenCreated_DoesNotMapCalculatedAlbumProperties()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            // Act
            var albumEntity = context.Model.FindEntityType(typeof(Album));

            // Assert
            albumEntity.Should().NotBeNull();
            albumEntity!.FindProperty(nameof(Album.FullPath)).Should().BeNull();
            albumEntity.FindProperty(nameof(Album.FullName)).Should().BeNull();
            albumEntity.FindProperty(nameof(Album.ChildrenCounter)).Should().BeNull();
            albumEntity.FindProperty(nameof(Album.PhotosCounter)).Should().BeNull();
            albumEntity.FindProperty(nameof(Album.AllPhotos)).Should().BeNull();
            albumEntity.FindProperty(nameof(Album.CoverImage)).Should().BeNull();
        }

        [Fact]
        public async Task Model_WhenCreated_DoesNotMapCalculatedFotoProperties()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            // Act
            var fotoEntity = context.Model.FindEntityType(typeof(Foto));

            // Assert
            fotoEntity.Should().NotBeNull();
            fotoEntity!.FindProperty(nameof(Foto.RelativePath)).Should().BeNull();
            fotoEntity.FindProperty(nameof(Foto.PhotoName)).Should().BeNull();
            fotoEntity.FindProperty(nameof(Foto.AltText)).Should().BeNull();
            fotoEntity.FindProperty(nameof(Foto.SelectionCode)).Should().BeNull();
        }

        #endregion

        #region Vincoli di unicità

        [Fact]
        public async Task SaveChanges_WhenSiblingAlbumsHaveSamePath_ThrowsDbUpdateException()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            var parent = CreateAlbum("Fashion", "fashion");
            context.Albums.Add(parent);
            await context.SaveChangesAsync();

            context.Albums.AddRange(
                CreateAlbum("Milano One", "milano", parent),
                CreateAlbum("Milano Two", "milano", parent));

            // Act
            var action = async () => await context.SaveChangesAsync();

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task SaveChanges_WhenDifferentParentsContainSamePath_SavesAlbums()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            var firstParent = CreateAlbum("Fashion", "fashion");
            var secondParent = CreateAlbum("Glamour", "glamour");

            context.Albums.AddRange(firstParent, secondParent);
            await context.SaveChangesAsync();

            context.Albums.AddRange(
                CreateAlbum("Milano", "milano", firstParent),
                CreateAlbum("Milano", "milano", secondParent));

            // Act
            var action = async () => await context.SaveChangesAsync();

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SaveChanges_WhenPhotosInSameAlbumHaveSameFileName_ThrowsDbUpdateException()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            var album = CreateAlbum("Fashion", "fashion");
            context.Albums.Add(album);
            await context.SaveChangesAsync();

            context.Foto.AddRange(
                CreatePhoto(album, "Photo_001.jpg"),
                CreatePhoto(album, "Photo_001.jpg"));

            // Act
            var action = async () => await context.SaveChangesAsync();

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task SaveChanges_WhenDifferentAlbumsContainSameFileName_SavesPhotos()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();
            await using var context = await CreateContext(connection);

            var firstAlbum = CreateAlbum("Fashion", "fashion");
            var secondAlbum = CreateAlbum("Glamour", "glamour");

            context.Albums.AddRange(firstAlbum, secondAlbum);
            await context.SaveChangesAsync();

            context.Foto.AddRange(
                CreatePhoto(firstAlbum, "Photo_001.jpg"),
                CreatePhoto(secondAlbum, "Photo_001.jpg"));

            // Act
            var action = async () => await context.SaveChangesAsync();

            // Assert
            await action.Should().NotThrowAsync();
        }

        #endregion

        #region Cancellazione

        [Fact]
        public async Task DeleteAlbum_WhenAlbumHasChildren_IsRejectedByDatabase()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();

            Guid parentId;

            await using (var arrangeContext = await CreateContext(connection))
            {
                var parent = CreateAlbum("Portfolio", "portfolio");
                var child = CreateAlbum("Fashion", "fashion", parent);

                arrangeContext.Albums.AddRange(parent, child);
                await arrangeContext.SaveChangesAsync();

                parentId = parent.Id;
            }

            await using var actContext = await CreateContext(connection);
            var parentToDelete = await actContext.Albums.SingleAsync(album => album.Id == parentId);

            actContext.Albums.Remove(parentToDelete);

            // Act
            var action = async () => await actContext.SaveChangesAsync();

            // Assert
            await action.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task DeleteAlbum_WhenAlbumHasPhotos_DeletesPhotosInCascade()
        {
            // Arrange
            await using var connection = await CreateOpenConnection();

            Guid albumId;
            Guid photoId;

            await using (var arrangeContext = await CreateContext(connection))
            {
                var album = CreateAlbum("Fashion", "fashion");
                var photo = CreatePhoto(album, "Photo_001.jpg");

                arrangeContext.Albums.Add(album);
                arrangeContext.Foto.Add(photo);
                await arrangeContext.SaveChangesAsync();

                albumId = album.Id;
                photoId = photo.Id;
            }

            await using (var deleteContext = await CreateContext(connection))
            {
                var albumToDelete = await deleteContext.Albums.SingleAsync(album => album.Id == albumId);
                deleteContext.Albums.Remove(albumToDelete);
                await deleteContext.SaveChangesAsync();
            }

            await using var assertContext = await CreateContext(connection);

            // Act
            var albumExists = await assertContext.Albums.AnyAsync(album => album.Id == albumId);
            var photoExists = await assertContext.Foto.AnyAsync(photo => photo.Id == photoId);

            // Assert
            albumExists.Should().BeFalse();
            photoExists.Should().BeFalse();
        }

        #endregion

        #region Helper

        private static async Task<SqliteConnection> CreateOpenConnection()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            await command.ExecuteNonQueryAsync();

            return connection;
        }

        private static async Task<PortfolioContext> CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<PortfolioContext>().UseSqlite(connection).Options;
            var context = new PortfolioContext(options);

            await context.Database.EnsureCreatedAsync();

            return context;
        }

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

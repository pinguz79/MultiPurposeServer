using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class SqliteManifestationStoreTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-tests-").FullName;

        private string DatabasePath => Path.Combine(_directory, "demo.skating");

        [Fact]
        public void Load_WhenCreatedByAnotherStore_ReturnsPersistedManifestation()
        {
            // Arrange
            var expected = new Manifestation(Guid.NewGuid(), "L'evento di prova – 舞");
            new SqliteManifestationStore(DatabasePath).Create(expected);
            var store = new SqliteManifestationStore(DatabasePath);

            // Act
            var actual = store.Load();

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Create_WhenFileAlreadyExists_ThrowsWithoutChangingFile()
        {
            // Arrange
            File.WriteAllText(DatabasePath, "File preesistente da preservare");
            var before = File.ReadAllBytes(DatabasePath);
            var store = new SqliteManifestationStore(DatabasePath);
            var manifestation = new Manifestation(Guid.NewGuid(), "Demo");

            // Act
            var action = () => store.Create(manifestation);

            // Assert
            action.Should().Throw<IOException>();
            File.ReadAllBytes(DatabasePath).Should().Equal(before);
        }

        [Fact]
        public void Load_WhenFileDoesNotExist_ThrowsWithoutCreatingFile()
        {
            // Arrange
            var store = new SqliteManifestationStore(DatabasePath);

            // Act
            var action = () => store.Load();

            // Assert
            action.Should().Throw<InvalidDataException>();
            File.Exists(DatabasePath).Should().BeFalse();
        }

        [Fact]
        public void Load_WhenFileIsNotSqlite_ThrowsWithoutChangingFile()
        {
            // Arrange
            File.WriteAllText(DatabasePath, "Non è un database");
            var before = File.ReadAllBytes(DatabasePath);
            var store = new SqliteManifestationStore(DatabasePath);

            // Act
            var action = () => store.Load();

            // Assert
            action.Should().Throw<InvalidDataException>();
            File.ReadAllBytes(DatabasePath).Should().Equal(before);
        }

        [Theory]
        [InlineData("PRAGMA application_id = 0;")]
        [InlineData("PRAGMA user_version = 999;")]
        [InlineData("DELETE FROM Manifestation;")]
        [InlineData("UPDATE Manifestation SET Id = 'invalid';")]
        public void Load_WhenArchiveIsIncompatible_ThrowsWithoutChangingFile(string alteration)
        {
            // Arrange
            var store = new SqliteManifestationStore(DatabasePath);
            store.Create(new Manifestation(Guid.NewGuid(), "Demo"));
            using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath, Pooling = false }.ToString()))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = alteration;
                command.ExecuteNonQuery();
            }

            var before = File.ReadAllBytes(DatabasePath);

            // Act
            var action = () => store.Load();

            // Assert
            action.Should().Throw<InvalidDataException>();
            File.ReadAllBytes(DatabasePath).Should().Equal(before);
        }

        public void Dispose() => Directory.Delete(_directory, true);
    }
}

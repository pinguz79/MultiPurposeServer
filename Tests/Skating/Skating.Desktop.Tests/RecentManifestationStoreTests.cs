using FluentAssertions;

using Skating.Desktop.Manifestations;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class RecentManifestationStoreTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-recent-tests-").FullName;

        [Fact]
        public void Remember_WhenFifthEntryAdded_KeepsLatestFourAcrossInstances()
        {
            // Arrange
            var file = Path.Combine(_directory, "recent.json");
            var store = new RecentManifestationStore(file);
            for (var index = 0; index < 4; index++)
            {
                store.Remember($"Evento {index}", Path.Combine(_directory, $"{index}.skating"));
            }

            // Act
            store.Remember("Evento 4", Path.Combine(_directory, "4.skating"));

            // Assert
            new RecentManifestationStore(file).Load().Select(entry => entry.Name).Should().Equal("Evento 4", "Evento 3", "Evento 2", "Evento 1");
        }

        [Fact]
        public void Remember_WhenSamePathHasDifferentCase_MovesToTopAndUpdatesName()
        {
            // Arrange
            var store = new RecentManifestationStore(Path.Combine(_directory, "recent.json"));
            var path = Path.Combine(_directory, "prima.skating");
            store.Remember("Prima", path);
            store.Remember("Seconda", Path.Combine(_directory, "seconda.skating"));

            // Act
            store.Remember("Rinominata", path.ToUpperInvariant());

            // Assert
            store.Load().Select(entry => entry.Name).Should().Equal("Rinominata", "Seconda");
        }

        [Theory]
        [InlineData("{invalid")]
        [InlineData("null")]
        [InlineData("[null,{}, { \"Name\": \"Relativo\", \"Path\": \"file.skating\" }]")]
        public void Load_WhenPreferencesInvalid_ReturnsEmptyWithoutChangingFile(string json)
        {
            // Arrange
            var path = Path.Combine(_directory, "recent.json");
            File.WriteAllText(path, json);
            var store = new RecentManifestationStore(path);

            // Act
            var entries = store.Load();

            // Assert
            entries.Should().BeEmpty();
            File.ReadAllText(path).Should().Be(json);
        }

        [Fact]
        public void Remember_WhenSameNameAtDifferentPaths_KeepsBothArchives()
        {
            // Arrange
            var store = new RecentManifestationStore(Path.Combine(_directory, "recent.json"));
            store.Remember("Evento", Path.Combine(_directory, "a", "evento.skating"));

            // Act
            store.Remember("Evento", Path.Combine(_directory, "b", "evento.skating"));

            // Assert
            store.Load().Should().HaveCount(2);
        }

        public void Dispose() => Directory.Delete(_directory, true);
    }
}

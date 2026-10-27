using FluentAssertions;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class ChoreographyStoreTests
    {
        [Fact]
        public void AddChoreography_WhenValid_PersistsAllFieldsAndMembers()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var entry = archive.Entry();

            // Act
            archive.Store.AddChoreography(entry);

            // Assert
            new SqliteChoreographyStore(archive.DatabasePath).GetChoreographies().Should().ContainSingle().Which.Should().BeEquivalentTo(entry);
        }

        [Fact]
        public void UpdateChoreography_WhenMembersChange_PreservesIdentityAndAthleteRegistry()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var original = archive.Entry();
            archive.Store.AddChoreography(original);
            var changed = archive.Entry("Corretto", original.Id, archive.Athletes.Skip(1));

            // Act
            archive.Store.UpdateChoreography(changed);

            // Assert
            archive.Store.GetChoreographies().Should().ContainSingle().Which.Should().BeEquivalentTo(changed);
            archive.Registry.GetAthletes().Should().HaveCount(3);
        }

        [Fact]
        public void AddChoreography_WhenSameMembersHaveDifferentName_AllowsSecondEntry()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());

            // Act
            archive.Store.AddChoreography(archive.Entry("Seconda coreografia"));

            // Assert
            archive.Store.GetChoreographies().Should().HaveCount(2);
        }

        [Fact]
        public void AddChoreography_WhenNameAndRaceMatchButMembersDiffer_AllowsEntry()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());

            // Act
            archive.Store.AddChoreography(archive.Entry(athletes: archive.Athletes.Skip(1)));

            // Assert
            archive.Store.GetChoreographies().Should().HaveCount(2);
        }

        [Fact]
        public void AddChoreography_WhenRaceDiffers_AllowsSameNameAndMembers()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var discipline = new Discipline(Guid.NewGuid(), "Moderno");
            archive.Registry.AddDiscipline(discipline);

            // Act
            archive.Store.AddChoreography(archive.Entry(discipline: discipline.Id));

            // Assert
            archive.Store.GetChoreographies().Should().HaveCount(2);
        }

        [Fact]
        public void AddChoreography_WhenDuplicateMembersAreReordered_RejectsWithoutWriting()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());

            // Act
            var action = () => archive.Store.AddChoreography(archive.Entry("  COREOGRAFIA  ", athletes: archive.Athletes.Take(2).Reverse()));

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies().Should().ContainSingle();
        }

        [Fact]
        public void UpdateChoreography_WhenDuplicateWouldResult_PreservesOriginal()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var original = archive.Entry("Seconda");
            archive.Store.AddChoreography(original);

            // Act
            var action = () => archive.Store.UpdateChoreography(archive.Entry(id: original.Id));

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies().Single(entry => entry.Id == original.Id).Should().BeEquivalentTo(original);
        }

        [Fact]
        public void UpdateChoreography_WhenAthleteDoesNotExist_RollsBackNameAndMembers()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            var original = archive.Entry();
            archive.Store.AddChoreography(original);
            var invalid = archive.Entry("Non salvare", original.Id, [archive.Athletes[2], Guid.NewGuid()]);

            // Act
            var action = () => archive.Store.UpdateChoreography(invalid);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies().Should().ContainSingle().Which.Should().BeEquivalentTo(original);
        }

        [Fact]
        public void AddChoreography_WhenSchoolDoesNotExist_RejectsWithoutWriting()
        {
            // Arrange
            using var archive = new ChoreographyArchive();

            // Act
            var action = () => archive.Store.AddChoreography(archive.Entry(school: Guid.NewGuid()));

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies().Should().BeEmpty();
        }

        [Fact]
        public void UpdateChoreography_WhenIdMissing_DoesNotInsert()
        {
            // Arrange
            using var archive = new ChoreographyArchive();

            // Act
            var action = () => archive.Store.UpdateChoreography(archive.Entry());

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Store.GetChoreographies().Should().BeEmpty();
        }

        [Fact]
        public void AddChoreography_WhenAthletesRepresentAnotherSchool_KeepsSharedIdentities()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            var school = new School(Guid.NewGuid(), "Altra scuola");
            archive.Registry.AddSchool(school);

            // Act
            archive.Store.AddChoreography(archive.Entry("Altro spettacolo", school: school.Id));

            // Assert
            archive.Store.GetChoreographies().SelectMany(entry => entry.AthleteIds).Distinct().Should().HaveCount(2);
            archive.Registry.GetAthletes().Should().HaveCount(3);
        }

        [Fact]
        public void UpdateSchool_WhenReferenced_PreservesChoreographyLink()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());

            // Act
            archive.Registry.UpdateSchool(new School(archive.SchoolId, "Scuola corretta"));

            // Assert
            archive.Store.GetChoreographies().Single().SchoolId.Should().Be(archive.SchoolId);
            archive.Registry.GetSchools().Single().Name.Should().Be("Scuola corretta");
        }
    }
}

using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class AbsoluteAwardStoreTests
    {
        [Theory]
        [InlineData(AwardAggregation.Discipline)]
        [InlineData(AwardAggregation.Category)]
        public void GetAward_WhenNonPro_UsesOnlyIntermediateWinnersWithoutWriting(AwardAggregation mode)
        {
            // Arrange
            using var archive = new AwardArchive(mode);
            ValidateIntermediates(archive, mode);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            var expected = archive.Awards.GetAwards(mode).SelectMany(award => award.Entries.Where(entry => entry.Position == 1)).Select(entry => entry.ChoreographyId);
            var before = File.ReadAllBytes(archive.DatabasePath);

            // Act
            var result = store.GetAward(AbsoluteAwardKind.NonPro, mode);

            // Assert
            result.Entries.Select(entry => entry.ChoreographyId).Should().BeEquivalentTo(expected);
            result.Entries.Should().NotContain(entry => entry.ChoreographyId == archive.ProId);
            result.CanValidate.Should().BeTrue();
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
        }

        [Fact]
        public void GetAward_WhenPro_DoesNotRequireIntermediateConfiguration()
        {
            // Arrange
            using var archive = new AwardArchive();
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);

            // Act
            var result = store.GetAward(AbsoluteAwardKind.Pro);

            // Assert
            result.Entries.Should().ContainSingle().Which.ChoreographyId.Should().Be(archive.ProId);
            result.IntermediateSources.Should().BeEmpty();
            result.CanValidate.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenIntermediatesNotValidated_RejectsNonPro()
        {
            // Arrange
            using var archive = new AwardArchive();
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);

            // Act
            var action = () => store.Validate(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            store.GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline).IsValidated.Should().BeFalse();
        }

        [Theory]
        [InlineData(AbsoluteAwardKind.NonPro)]
        [InlineData(AbsoluteAwardKind.Pro)]
        public void Validate_WhenReady_PersistsIndependentValidation(AbsoluteAwardKind kind)
        {
            // Arrange
            using var archive = new AwardArchive();
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);

            // Act
            store.Validate(kind, AwardAggregation.Discipline);

            // Assert
            var reopened = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            reopened.GetAward(kind, AwardAggregation.Discipline).IsValidated.Should().BeTrue();
            reopened.GetAward(kind == AbsoluteAwardKind.Pro ? AbsoluteAwardKind.NonPro : AbsoluteAwardKind.Pro, AwardAggregation.Discipline).IsValidated.Should().BeFalse();
        }

        [Theory]
        [InlineData(AbsoluteAwardKind.NonPro)]
        [InlineData(AbsoluteAwardKind.Pro)]
        public void Reopen_WhenSourceRaceReopened_InvalidatesOnlyItsAbsolute(AbsoluteAwardKind kind)
        {
            // Arrange
            using var archive = new AwardArchive();
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            store.Validate(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);
            store.Validate(AbsoluteAwardKind.Pro);
            var key = archive.RaceOf(kind == AbsoluteAwardKind.Pro ? archive.ProId : archive.WinnerId);

            // Act
            archive.Races.Reopen(key);

            // Assert
            store.GetAward(kind, AwardAggregation.Discipline).IsValidated.Should().BeFalse();
            store.GetAward(kind == AbsoluteAwardKind.Pro ? AbsoluteAwardKind.NonPro : AbsoluteAwardKind.Pro, AwardAggregation.Discipline).IsValidated.Should().BeTrue();
        }

        [Fact]
        public void Reopen_WhenIntermediateReopened_InvalidatesNonProWithoutChangingPro()
        {
            // Arrange
            using var archive = new AwardArchive();
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            store.Validate(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);
            store.Validate(AbsoluteAwardKind.Pro);

            // Act
            archive.Awards.Reopen(AwardAggregation.Discipline, archive.DisciplineId);

            // Assert
            store.GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline).IsValidated.Should().BeFalse();
            store.GetAward(AbsoluteAwardKind.Pro).IsValidated.Should().BeTrue();
        }

        [Theory]
        [InlineData(AbsoluteAwardKind.NonPro)]
        [InlineData(AbsoluteAwardKind.Pro)]
        public void Reopen_WhenAbsoluteValidated_PreservesSourceResults(AbsoluteAwardKind kind)
        {
            // Arrange
            using var archive = new AwardArchive();
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            store.Validate(kind, AwardAggregation.Discipline);

            // Act
            store.Reopen(kind);

            // Assert
            store.GetAward(kind, AwardAggregation.Discipline).IsValidated.Should().BeFalse();
            archive.Awards.GetAwards(AwardAggregation.Discipline).Should().OnlyContain(award => award.IsValidated);
            archive.Races.GetRaces().Select(race => archive.Races.GetRanking(race.Key)).Should().OnlyContain(race => race.IsClosed);
        }

        [Fact]
        public void GetAward_WhenOriginalScoreCorrected_RequiresNewValidationAfterSourcesReclose()
        {
            // Arrange
            using var archive = new AwardArchive();
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            store.Validate(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);
            var key = archive.RaceOf(archive.WinnerId);
            archive.Races.Reopen(key);
            archive.Performances.Reopen(archive.WinnerId);
            archive.Performances.SaveScore(archive.WinnerId, archive.Registry.GetJudges()[0].Id, archive.Registry.GetCriteria()[0].Id, 81);
            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(key);
            archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Act
            var award = store.GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);

            // Assert
            award.Entries[0].TotalTenths.Should().Be(721);
            award.IsValidated.Should().BeFalse();
            award.CanValidate.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenAbsolutePodiumTied_RejectsValidation()
        {
            // Arrange
            using var archive = new AwardArchive();
            var other = archive.Choreographies.GetChoreographies().Single(entry => entry.Name == "Vincitore C");
            ChangeAllScores(archive, other.Id, 80);
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);

            // Act
            var action = () => store.Validate(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            store.GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline).HasPodiumTies.Should().BeTrue();
        }

        [Fact]
        public void GetAward_WhenIntermediateExcludedJudge_RecalculatesFromOriginalScores()
        {
            // Arrange
            using var archive = new AwardArchive();
            var other = archive.Choreographies.GetChoreographies().Single(entry => entry.Name == "Vincitore B");
            var key = archive.RaceOf(other.Id);
            var judge = archive.Registry.GetJudges()[0].Id;
            archive.Races.Reopen(key);
            archive.Performances.Reopen(other.Id);
            foreach (var criterion in archive.Registry.GetCriteria())
            {
                archive.Performances.SaveScore(other.Id, judge, criterion.Id, null);
            }

            archive.Performances.SetJudgeRequired(other.Id, judge, false);
            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(key);
            ValidateIntermediates(archive);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);

            // Act
            var award = store.GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);

            // Assert
            award.Entries[0].ChoreographyId.Should().Be(archive.WinnerId);
            award.Entries[0].TotalTenths.Should().Be(720);
            award.Entries[0].ExcludedJudgeIds.Should().BeEmpty();
            archive.Awards.GetAwards(AwardAggregation.Discipline).Single(entry => entry.GroupId == archive.DisciplineId).Entries[0].TotalTenths.Should().Be(480);
        }

        [Theory]
        [InlineData(AbsoluteAwardKind.NonPro)]
        [InlineData(AbsoluteAwardKind.Pro)]
        public void GetAward_WhenNoSources_CannotValidate(AbsoluteAwardKind kind)
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            new SqliteIntermediateAwardStore(archive.DatabasePath).Configure(AwardAggregation.Category);
            var store = new SqliteAbsoluteAwardStore(archive.DatabasePath);

            // Act
            var result = store.GetAward(kind, AwardAggregation.Category);

            // Assert
            result.Entries.Should().BeEmpty();
            result.CanValidate.Should().BeFalse();
        }

        [Fact]
        public void Upgrade_WhenVersionSeven_PreservesValidatedIntermediates()
        {
            // Arrange
            using var archive = new AwardArchive();
            ValidateIntermediates(archive);
            using (var connection = new SqliteConnection($"Data Source={archive.DatabasePath};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "DROP TABLE AbsoluteAward; PRAGMA user_version = 7;";
                command.ExecuteNonQuery();
            }

            // Act
            new SqliteManifestationStore(archive.DatabasePath).Upgrade();

            // Assert
            archive.Awards.GetAwards(AwardAggregation.Discipline).Should().OnlyContain(award => award.IsValidated);
            new SqliteAbsoluteAwardStore(archive.DatabasePath).GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline).CanValidate.Should().BeTrue();
        }

        private static void ValidateIntermediates(AwardArchive archive, AwardAggregation mode = AwardAggregation.Discipline)
        {
            foreach (var award in archive.Awards.GetAwards(mode))
            {
                archive.Awards.Validate(mode, award.GroupId);
            }
        }

        private static void ChangeAllScores(AwardArchive archive, Guid id, int score)
        {
            var key = archive.RaceOf(id);
            archive.Races.Reopen(key);
            archive.Performances.Reopen(id);
            foreach (var judge in archive.Registry.GetJudges())
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    archive.Performances.SaveScore(id, judge.Id, criterion.Id, score);
                }
            }

            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(key);
        }
    }
}

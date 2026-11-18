using FluentAssertions;

using Microsoft.Data.Sqlite;

using Skating.Core;
using Skating.Persistence;

namespace Skating.Tests.Integration
{
    [Trait("Category", "Integration")]
    public sealed class IntermediateAwardStoreTests
    {
        [Theory]
        [InlineData(AwardAggregation.Discipline)]
        [InlineData(AwardAggregation.Category)]
        public void GetAwards_WhenConfigured_GroupsOnlyNonProWinnersWithoutWriting(AwardAggregation mode)
        {
            // Arrange
            using var archive = new AwardArchive(mode);
            var before = File.ReadAllBytes(archive.DatabasePath);

            // Act
            var awards = archive.Awards.GetAwards(mode);

            // Assert
            awards.Should().HaveCount(2);
            awards.Sum(award => award.Entries.Count).Should().Be(3);
            awards.SelectMany(award => award.Entries).Should().NotContain(entry => entry.ChoreographyId == archive.ProId);
            var group = awards.Single(award => award.GroupId == (mode == AwardAggregation.Discipline ? archive.DisciplineId : archive.CategoryId));
            group.Entries.Should().HaveCount(2);
            group.Entries[0].ChoreographyId.Should().Be(archive.WinnerId);
            group.CanValidate.Should().BeTrue();
            File.ReadAllBytes(archive.DatabasePath).Should().Equal(before);
        }

        [Fact]
        public void Validate_WhenSourceOpen_RejectsValidation()
        {
            // Arrange
            using var archive = new AwardArchive(close: false);

            // Act
            var action = () => archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Awards.GetAwards(AwardAggregation.Discipline).Should().OnlyContain(award => !award.IsValidated);
        }

        [Fact]
        public void Validate_WhenReady_PersistsAcrossReopeningArchive()
        {
            // Arrange
            using var archive = new AwardArchive();

            // Act
            archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Assert
            new SqliteIntermediateAwardStore(archive.DatabasePath).GetAwards(AwardAggregation.Discipline).Single(award => award.GroupId == archive.DisciplineId).IsValidated.Should().BeTrue();
        }

        [Fact]
        public void Reopen_WhenSourceRaceReopens_InvalidatesOnlyRelatedAward()
        {
            // Arrange
            using var archive = new AwardArchive();
            foreach (var award in archive.Awards.GetAwards(AwardAggregation.Discipline))
            {
                archive.Awards.Validate(AwardAggregation.Discipline, award.GroupId);
            }

            var key = archive.RaceOf(archive.WinnerId);

            // Act
            archive.Races.Reopen(key);

            // Assert
            var awards = archive.Awards.GetAwards(AwardAggregation.Discipline);
            awards.Single(award => award.GroupId == archive.DisciplineId).IsValidated.Should().BeFalse();
            awards.Single(award => award.GroupId != archive.DisciplineId).IsValidated.Should().BeTrue();
        }

        [Fact]
        public void GetAwards_WhenOfficialScoreCorrected_UsesUpdatedOriginalAndRequiresRevalidation()
        {
            // Arrange
            using var archive = new AwardArchive();
            archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);
            var key = archive.RaceOf(archive.WinnerId);
            archive.Races.Reopen(key);
            archive.Performances.Reopen(archive.WinnerId);
            archive.Performances.SaveScore(archive.WinnerId, archive.Registry.GetJudges()[0].Id, archive.Registry.GetCriteria()[0].Id, 81);
            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(key);

            // Act
            var award = archive.Awards.GetAwards(AwardAggregation.Discipline).Single(entry => entry.GroupId == archive.DisciplineId);

            // Assert
            award.Entries[0].TotalTenths.Should().Be(721);
            award.IsValidated.Should().BeFalse();
            award.CanValidate.Should().BeTrue();
            archive.Races.GetRanking(key).Entries[0].TotalTenths.Should().Be(721);
        }

        [Fact]
        public void Reopen_WhenProRaceReopens_PreservesNonProAward()
        {
            // Arrange
            using var archive = new AwardArchive();
            archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Act
            archive.Races.Reopen(archive.RaceOf(archive.ProId));

            // Assert
            archive.Awards.GetAwards(AwardAggregation.Discipline).Single(award => award.GroupId == archive.DisciplineId).IsValidated.Should().BeTrue();
        }

        [Theory]
        [InlineData(AwardAggregation.Discipline, AwardAggregation.Category)]
        [InlineData(AwardAggregation.Category, AwardAggregation.Discipline)]
        public void Configure_WhenStartedWithAnotherMode_RecalculatesWithoutChangingRacesOrScores(AwardAggregation source, AwardAggregation target)
        {
            // Arrange
            using var archive = new AwardArchive(source);

            // Act
            archive.Awards.Configure(target);

            // Assert
            archive.Awards.GetAggregation().Should().Be(target);
            archive.Awards.GetAwards(target).Should().HaveCount(2);
            var ranking = archive.Races.GetRanking(archive.RaceOf(archive.WinnerId));
            ranking.IsClosed.Should().BeTrue();
            ranking.Entries[0].TotalTenths.Should().Be(720);
        }

        [Fact]
        public void GetAwards_WhenModeMismatch_RejectsSilentRuleChange()
        {
            // Arrange
            using var archive = new AwardArchive();

            // Act
            var action = () => archive.Awards.GetAwards(AwardAggregation.Category);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Configure_WhenValidatedWithoutConsent_RejectsAndPreservesValidations()
        {
            // Arrange
            using var archive = new AwardArchive();
            archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Act
            var action = () => archive.Awards.Configure(AwardAggregation.Category);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Awards.GetAggregation().Should().Be(AwardAggregation.Discipline);
            archive.Awards.HasValidatedAwards().Should().BeTrue();
        }

        [Fact]
        public void Configure_WhenValidatedWithConsent_InvalidatesOpenAwardsOnlyAndPreservesOriginalVotes()
        {
            // Arrange
            using var archive = new AwardArchive();
            foreach (var award in archive.Awards.GetAwards(AwardAggregation.Discipline))
            {
                archive.Awards.Validate(AwardAggregation.Discipline, award.GroupId);
            }

            var absolute = new SqliteAbsoluteAwardStore(archive.DatabasePath);
            absolute.Validate(AbsoluteAwardKind.NonPro, AwardAggregation.Discipline);
            absolute.Validate(AbsoluteAwardKind.Pro);
            var scores = archive.Performances.OpenPage(1).Scores;

            // Act
            archive.Awards.Configure(AwardAggregation.Category, revokeValidations: true);

            // Assert
            archive.Awards.HasValidatedAwards().Should().BeFalse();
            archive.Awards.GetAwards(AwardAggregation.Category).Should().OnlyContain(award => !award.IsValidated);
            absolute.GetAward(AbsoluteAwardKind.NonPro, AwardAggregation.Category).IsValidated.Should().BeFalse();
            absolute.GetAward(AbsoluteAwardKind.Pro).IsValidated.Should().BeTrue();
            archive.Performances.OpenPage(1).Scores.Should().BeEquivalentTo(scores);
            archive.Races.GetRaces().Select(race => archive.Races.GetRanking(race.Key)).Should().OnlyContain(ranking => ranking.IsClosed);
        }

        [Fact]
        public void Configure_WhenUnchanged_PreservesValidationWithoutRequiringConsent()
        {
            // Arrange
            using var archive = new AwardArchive();
            archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Act
            archive.Awards.Configure(AwardAggregation.Discipline);

            // Assert
            archive.Awards.HasValidatedAwards().Should().BeTrue();
        }

        [Fact]
        public void Start_WhenDefaultDiffersFromArchive_PreservesArchiveCriterion()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            for (var index = 0; index < 3; index++)
            {
                archive.Registry.AddJudge(new Judge(Guid.NewGuid(), $"Giudice {index}"));
            }

            var awards = new SqliteIntermediateAwardStore(archive.DatabasePath);
            awards.Configure(AwardAggregation.Category);
            var performances = new SqlitePerformanceStore(archive.DatabasePath, AwardAggregation.Discipline);
            performances.Prepare(5);
            performances.ConfirmBlocks();

            // Act
            performances.Start();

            // Assert
            awards.GetAggregation().Should().Be(AwardAggregation.Category);
            performances.GetSchedule().IsStarted.Should().BeTrue();
        }

        [Fact]
        public void Configure_WhenNotStarted_AllowsExplicitModeChange()
        {
            // Arrange
            using var archive = new RegistryArchive();
            archive.Create();
            var store = new SqliteIntermediateAwardStore(archive.DatabasePath);
            store.Configure(AwardAggregation.Discipline);

            // Act
            store.Configure(AwardAggregation.Category);

            // Assert
            store.GetAggregation().Should().Be(AwardAggregation.Category);
        }

        [Fact]
        public void Start_WhenModeProvided_PinsAggregationToManifestation()
        {
            // Arrange
            using var archive = new ChoreographyArchive();
            archive.Store.AddChoreography(archive.Entry());
            for (var index = 0; index < 3; index++)
            {
                archive.Registry.AddJudge(new Judge(Guid.NewGuid(), $"Giudice {index}"));
            }

            var performances = new SqlitePerformanceStore(archive.DatabasePath, AwardAggregation.Category);
            performances.Prepare(20);
            performances.ConfirmBlocks();

            // Act
            performances.Start();

            // Assert
            new SqliteIntermediateAwardStore(archive.DatabasePath).GetAggregation().Should().Be(AwardAggregation.Category);
            performances.GetSchedule().IsStarted.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenIntermediatePodiumTied_RejectsValidation()
        {
            // Arrange
            using var archive = new AwardArchive();
            var other = archive.Choreographies.GetChoreographies().Single(entry => entry.Name == "Vincitore B");
            var key = archive.RaceOf(other.Id);
            archive.Races.Reopen(key);
            archive.Performances.Reopen(other.Id);
            foreach (var judge in archive.Registry.GetJudges())
            {
                foreach (var criterion in archive.Registry.GetCriteria())
                {
                    archive.Performances.SaveScore(other.Id, judge.Id, criterion.Id, 80);
                }
            }

            archive.Performances.ConfirmComplete(1);
            archive.Races.Close(key);

            // Act
            var action = () => archive.Awards.Validate(AwardAggregation.Discipline, archive.DisciplineId);

            // Assert
            action.Should().Throw<InvalidOperationException>();
            archive.Awards.GetAwards(AwardAggregation.Discipline).Single(award => award.GroupId == archive.DisciplineId).HasPodiumTies.Should().BeTrue();
        }

        [Fact]
        public void GetAwards_WhenWinnerJuriesDiffer_RecalculatesExclusionsFromOriginalScores()
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

            // Act
            var award = archive.Awards.GetAwards(AwardAggregation.Discipline).Single(entry => entry.GroupId == archive.DisciplineId);

            // Assert
            award.Entries[0].TotalTenths.Should().Be(480);
            award.Entries[0].ExcludedJudgeIds.Should().Equal(judge);
            award.Entries[1].TotalTenths.Should().Be(450);
            archive.Races.GetRanking(archive.RaceOf(archive.WinnerId)).Entries[0].TotalTenths.Should().Be(720);
        }

        [Fact]
        public void Upgrade_WhenVersionSix_PreservesClosedRacesAndVotes()
        {
            // Arrange
            using var archive = new AwardArchive();
            using (var connection = new SqliteConnection($"Data Source={archive.DatabasePath};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "ALTER TABLE EventState DROP COLUMN BlockSize; DROP TABLE RaceColor; DROP TABLE AbsoluteAward; DROP TABLE IntermediateAward; DROP TABLE AwardSettings; PRAGMA user_version = 6;";
                command.ExecuteNonQuery();
            }

            // Act
            new SqliteManifestationStore(archive.DatabasePath).Upgrade();

            // Assert
            archive.Awards.GetAggregation().Should().BeNull();
            var ranking = archive.Races.GetRanking(archive.RaceOf(archive.WinnerId));
            ranking.IsClosed.Should().BeTrue();
            ranking.Entries[0].TotalTenths.Should().Be(720);
        }
    }
}

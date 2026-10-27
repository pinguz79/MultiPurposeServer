using FluentAssertions;

using Skating.Core;

namespace Skating.Tests
{
    public sealed class ChoreographyTests
    {
        [Theory]
        [InlineData(CompositionKind.Solo, 1)]
        [InlineData(CompositionKind.Duo, 2)]
        [InlineData(CompositionKind.Group, 3)]
        [InlineData(CompositionKind.Group, 20)]
        public void Constructor_WhenCompositionMatches_AcceptsMembers(CompositionKind kind, int count)
        {
            // Arrange
            var members = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();

            // Act
            var entry = Create(kind, members);

            // Assert
            entry.AthleteIds.Should().Equal(members);
        }

        [Theory]
        [InlineData(CompositionKind.Solo, 0)]
        [InlineData(CompositionKind.Solo, 2)]
        [InlineData(CompositionKind.Duo, 1)]
        [InlineData(CompositionKind.Duo, 3)]
        [InlineData(CompositionKind.Group, 2)]
        public void Constructor_WhenCompositionDoesNotMatch_RejectsMembers(CompositionKind kind, int count)
        {
            // Arrange
            var members = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();

            // Act
            var action = () => Create(kind, members);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WhenMemberRepeated_RejectsDuplicate()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var action = () => Create(CompositionKind.Duo, [id, id]);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36000)]
        public void Constructor_WhenDurationIsNonNegative_DoesNotApplyRegulatoryRange(int seconds)
        {
            // Arrange
            var members = new[] { Guid.NewGuid() };

            // Act
            var entry = Create(CompositionKind.Solo, members, seconds);

            // Assert
            entry.DurationSeconds.Should().Be(seconds);
        }

        [Fact]
        public void Constructor_WhenDurationNegative_RejectsValue()
        {
            // Arrange
            var members = new[] { Guid.NewGuid() };

            // Act
            var action = () => Create(CompositionKind.Solo, members, -1);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AthleteIds_WhenSourceMembersChange_KeepsSnapshot()
        {
            // Arrange
            var original = Guid.NewGuid();
            Guid[] members = [original];
            var entry = Create(CompositionKind.Solo, members);

            // Act
            members[0] = Guid.NewGuid();

            // Assert
            entry.AthleteIds.Should().Equal(original);
        }

        private static Choreography Create(CompositionKind kind, IEnumerable<Guid> members, int seconds = 0) => new(Guid.NewGuid(), "Prova", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), kind, seconds, members);
    }
}

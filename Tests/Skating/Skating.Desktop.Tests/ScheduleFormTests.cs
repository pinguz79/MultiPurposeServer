using System.Reflection;

using FluentAssertions;

using Skating.Desktop.Scheduling;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class ScheduleFormTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task PrepareBlocks_WhenAlreadyPrepared_RequiresConsentAndUsesSelectedCapacity(bool consent) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(choreographyCount: 12, start: false);
            var original = archive.Performances.GetSchedule();
            var prompts = 0;
            using var form = new ScheduleForm(archive.Performances, archive.Registry, archive.Choreographies, archive.Preferences,
                confirm: _ => { prompts++; return consent; });
            var size = form.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<NumericUpDown>().First();
            size.Value = 5;
            var prepare = typeof(ScheduleForm).GetMethod("PrepareBlocks", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            prepare.Invoke(form, null);

            // Assert
            prompts.Should().Be(1);
            var schedule = archive.Performances.GetSchedule();
            if (consent)
            {
                schedule.Performances.GroupBy(entry => entry.Block).Select(group => group.Count()).Should().Equal(5, 5, 2);
                schedule.BlockSize.Should().Be(5);
            }
            else
            {
                schedule.Should().BeEquivalentTo(original);
            }
        });

        [Fact]
        public Task ScheduleForm_WhenReopened_ShowsSavedCapacity() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(start: false);
            archive.Performances.Prepare(7);

            // Act
            using var form = new ScheduleForm(archive.Performances, archive.Registry, archive.Choreographies, archive.Preferences);

            // Assert
            form.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<NumericUpDown>().First().Value.Should().Be(7);
        });
    }
}

using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Choreographies;
using Skating.Desktop.Registry;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class DeletionFormTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task DeleteButtonClick_WhenChoreographySelected_RequiresConsentAndRefreshes(bool consent) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(start: false);
            var prompts = 0;
            using var form = new ChoreographyForm(archive.Registry, archive.Choreographies, confirmDelete: _ => { prompts++; return consent; });
            var grid = form.Controls.Find("entriesGrid", true).OfType<DataGridView>().Single();
            grid.BindingContext = new BindingContext();
            Invoke(form, "RefreshRows");
            var selected = (grid.Rows[0].DataBoundItem as ChoreographyRow ?? throw new InvalidOperationException()).Id;
            grid.CurrentCell = grid.Rows[0].Cells[0];

            // Act
            Invoke(form, "DeleteButtonClick", null, EventArgs.Empty);

            // Assert
            prompts.Should().Be(1);
            archive.Choreographies.GetChoreographies().Any(entry => entry.Id == selected).Should().Be(!consent);
            grid.Rows.Count.Should().Be(consent ? 1 : 2);
            archive.Registry.GetAthletes().Should().ContainSingle();
        });

        [Theory]
        [InlineData("School", false)]
        [InlineData("School", true)]
        [InlineData("Athlete", false)]
        [InlineData("Athlete", true)]
        public Task DeleteButtonClick_WhenUnusedRegistryEntry_RequiresConsent(string kind, bool consent) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive(start: false);
            var id = Guid.NewGuid();
            if (kind == "School")
            {
                archive.Registry.AddSchool(new School(id, "Libera"));
            }
            else
            {
                archive.Registry.AddAthlete(new Athlete(id, "Libero", "Atleta"));
            }

            var prompts = 0;
            using var form = new RegistryForm(archive.Registry, Enum.Parse<RegistryKind>(kind), _ => { prompts++; return consent; });
            var grid = form.Controls.Find("entriesGrid", true).OfType<DataGridView>().Single();
            grid.BindingContext = new BindingContext();
            Invoke(form, "RefreshRows");
            grid.CurrentCell = grid.Rows.Cast<DataGridViewRow>().Single(row => row.DataBoundItem is RegistryRow entry && entry.Id == id).Cells[0];

            // Act
            Invoke(form, "DeleteButtonClick", null, EventArgs.Empty);

            // Assert
            prompts.Should().Be(1);
            grid.Rows.Cast<DataGridViewRow>().Any(row => row.DataBoundItem is RegistryRow entry && entry.Id == id).Should().Be(!consent);
        });

        private static void Invoke(Form form, string method, params object?[] arguments)
        {
            var handler = form.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            handler.Invoke(form, arguments);
        }
    }
}

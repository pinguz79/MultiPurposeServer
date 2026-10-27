using System.Reflection;

using FluentAssertions;

using Skating.Core;
using Skating.Desktop.Registry;
using Skating.Persistence;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class RegistryFormTests : IDisposable
    {
        private readonly string _directory = Directory.CreateTempSubdirectory("skating-ui-tests-").FullName;

        [Theory]
        [InlineData("Athlete")]
        [InlineData("School")]
        [InlineData("Judge")]
        [InlineData("Discipline")]
        [InlineData("Category")]
        [InlineData("Level")]
        public Task AddButtonClick_WhenInputIsValid_PersistsAndRefreshesWithoutClosingForm(string kind) => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            using var form = new RegistryForm(new SqliteRegistryStore(path), Enum.Parse<RegistryKind>(kind));
            var name = form.Controls.Find("nameTextBox", true).OfType<TextBox>().Single();
            var lastName = form.Controls.Find("lastNameTextBox", true).OfType<TextBox>().Single();
            var pro = form.Controls.Find("proCheckBox", true).OfType<CheckBox>().Single();
            var grid = form.Controls.Find("entriesGrid", true).OfType<DataGridView>().Single();
            name.Text = "Anna";
            lastName.Text = "Rossi";
            pro.Checked = true;
            var handler = typeof(RegistryForm).GetMethod("AddButtonClick", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            handler.Invoke(form, [null, EventArgs.Empty]);

            // Assert
            form.IsDisposed.Should().BeFalse();
            name.Text.Should().BeEmpty();
            lastName.Text.Should().BeEmpty();
            var rows = grid.DataSource.Should().BeOfType<List<RegistryRow>>().Subject;
            rows.Should().ContainSingle().Which.Name.Should().Be("Anna");
            rows[0].Detail.Should().Be(kind == "Athlete" ? "Rossi" : kind == "Level" ? "PRO" : kind == "Judge" ? "Presente" : string.Empty);
            form.Controls.Find("statusLabel", true).Single().Text.Should().Be("Voce salvata.");
        });

        [Fact]
        public Task RefreshRows_WhenReopeningRegistry_BindsPersistedAthletes() => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            new SqliteRegistryStore(path).AddAthlete(new Athlete(Guid.NewGuid(), "Anna", "Rossi"));
            using var form = new RegistryForm(new SqliteRegistryStore(path), RegistryKind.Athlete);
            var grid = form.Controls.Find("entriesGrid", true).OfType<DataGridView>().Single();
            grid.BindingContext = new BindingContext();
            var refresh = typeof(RegistryForm).GetMethod("RefreshRows", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            refresh.Invoke(form, null);

            // Assert
            grid.Rows.Count.Should().Be(1);
            grid.Rows[0].Cells[0].Value.Should().Be("Anna");
            grid.Rows[0].Cells[1].Value.Should().Be("Rossi");
        });

        [Theory]
        [InlineData("Athlete")]
        [InlineData("Discipline")]
        [InlineData("Category")]
        [InlineData("Level")]
        public Task AddButtonClick_WhenEditing_UpdatesSameRowAndReturnsToCreation(string kind) => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            using var form = new RegistryForm(new SqliteRegistryStore(path), Enum.Parse<RegistryKind>(kind));
            var grid = form.Controls.Find("entriesGrid", true).OfType<DataGridView>().Single();
            grid.BindingContext = new BindingContext();
            var name = form.Controls.Find("nameTextBox", true).OfType<TextBox>().Single();
            var lastName = form.Controls.Find("lastNameTextBox", true).OfType<TextBox>().Single();
            name.Text = "Anna";
            lastName.Text = "Rossi";
            InvokeHandler(form, "AddButtonClick");
            grid.CurrentCell = grid.Rows[0].Cells[0];
            var original = grid.Rows[0].DataBoundItem as RegistryRow ?? throw new InvalidOperationException();
            InvokeHandler(form, "EditButtonClick");
            name.Text = "Nome corretto";
            lastName.Text = "Cognome corretto";
            form.Controls.Find("proCheckBox", true).OfType<CheckBox>().Single().Checked = true;

            // Act
            InvokeHandler(form, "AddButtonClick");

            // Assert
            var rows = grid.DataSource.Should().BeOfType<List<RegistryRow>>().Subject;
            rows.Should().ContainSingle().Which.Id.Should().Be(original.Id);
            rows[0].Name.Should().Be("Nome corretto");
            if (kind == "Athlete")
            {
                rows[0].Detail.Should().Be("Cognome corretto");
            }

            if (kind == "Level")
            {
                rows[0].IsPro.Should().BeTrue();
            }

            form.Controls.Find("addButton", true).Single().Text.Should().Be("Aggiungi");
            grid.Enabled.Should().BeTrue();
            name.Text.Should().BeEmpty();
        });

        [Fact]
        public Task CancelEditButtonClick_WhenEditing_DiscardsChangesWithoutWriting() => WinFormsTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(_directory, "demo.skating");
            new SqliteManifestationStore(path).Create(new Manifestation(Guid.NewGuid(), "Prova"));
            var store = new SqliteRegistryStore(path);
            var athlete = new Athlete(Guid.NewGuid(), "Anna", "Rossi");
            store.AddAthlete(athlete);
            using var form = new RegistryForm(store, RegistryKind.Athlete);
            var grid = form.Controls.Find("entriesGrid", true).OfType<DataGridView>().Single();
            grid.BindingContext = new BindingContext();
            var refresh = typeof(RegistryForm).GetMethod("RefreshRows", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            refresh.Invoke(form, null);
            grid.CurrentCell = grid.Rows[0].Cells[0];
            InvokeHandler(form, "EditButtonClick");
            form.Controls.Find("nameTextBox", true).Single().Text = "Da scartare";

            // Act
            InvokeHandler(form, "CancelEditButtonClick");

            // Assert
            store.GetAthletes().Should().ContainSingle().Which.Should().BeEquivalentTo(athlete);
            form.Controls.Find("nameTextBox", true).Single().Text.Should().BeEmpty();
            grid.Enabled.Should().BeTrue();
        });

        private static void InvokeHandler(RegistryForm form, string name)
        {
            var handler = typeof(RegistryForm).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            handler.Invoke(form, [null, EventArgs.Empty]);
        }

        public void Dispose() => Directory.Delete(_directory, true);
    }
}

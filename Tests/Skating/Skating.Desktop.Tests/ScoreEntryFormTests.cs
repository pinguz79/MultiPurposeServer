using System.Reflection;

using FluentAssertions;

using Skating.Desktop.Configuration;
using Skating.Desktop.Scoring;

namespace Skating.Desktop.Tests
{
    [Trait("Category", "Integration")]
    public sealed class ScoreEntryFormTests
    {
        [Fact]
        public Task TryFinishEdit_WhenAlertTriggersValidation_ShowsOnlyOneAlert() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            var alerts = 0;
            ScoreGrid? grid = null;
            using var form = new ScoreEntryForm(archive.Performances, archive.Registry, archive.Choreographies, 1, archive.Preferences, _ =>
            {
                alerts++;
                if (alerts == 1)
                {
                    grid?.TryFinishEdit().Should().BeFalse();
                }
            });
            grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.CurrentCell = grid.Rows[0].Cells[2];
            grid.BeginEdit(true);
            var editor = grid.EditingControl as TextBox ?? throw new InvalidOperationException();
            editor.Text = "11";

            // Act
            var result = grid.TryFinishEdit();

            // Assert
            result.Should().BeFalse();
            alerts.Should().Be(1);
            grid.IsCurrentCellInEditMode.Should().BeTrue();
            grid.CancelEdit();
        });

        [Theory]
        [InlineData(Keys.Enter)]
        [InlineData(Keys.Tab)]
        public Task Navigate_WhenEditorContainsInvalidScore_KeepsCellAndFormOpen(Keys key) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = archive.CreateForm();
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.CurrentCell = grid.Rows[0].Cells[2];
            grid.BeginEdit(true).Should().BeTrue();
            var editor = grid.EditingControl as TextBox ?? throw new InvalidOperationException();
            editor.Text = "11";
            var method = typeof(ScoreGrid).GetMethod("Navigate", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

            // Act
            var action = () => method.Invoke(grid, [key]);

            // Assert
            action.Should().NotThrow();
            grid.CurrentCell.Should().BeSameAs(grid.Rows[0].Cells[2]);
            grid.IsCurrentCellInEditMode.Should().BeTrue();
            editor.Text.Should().Be("11");
            archive.Alerts.Should().ContainSingle().Which.Should().Contain("0 a 10");
            form.IsDisposed.Should().BeFalse();
            archive.Performances.OpenPage(1).Scores.Should().BeEmpty();
            grid.CancelEdit();
        });

        [Fact]
        public Task TryFinishEdit_WhenInvalidValueCorrected_SavesCorrection() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = archive.CreateForm();
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.CurrentCell = grid.Rows[0].Cells[2];
            grid.BeginEdit(true);
            var editor = grid.EditingControl as TextBox ?? throw new InvalidOperationException();
            editor.Text = "11";
            grid.TryFinishEdit().Should().BeFalse();
            editor.Text = "7,8";

            // Act
            var completed = grid.TryFinishEdit();

            // Assert
            completed.Should().BeTrue();
            archive.Performances.OpenPage(1).Scores.Should().ContainSingle().Which.Tenths.Should().Be(78);
        });

        [Fact]
        public Task FormClosing_WhenInvalidScore_CancelsClose() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = archive.CreateForm();
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.CurrentCell = grid.Rows[0].Cells[2];
            grid.BeginEdit(true);
            var editor = grid.EditingControl as TextBox ?? throw new InvalidOperationException();
            editor.Text = "11";
            var args = new FormClosingEventArgs(CloseReason.UserClosing, false);
            var method = typeof(Form).GetMethod("OnFormClosing", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

            // Act
            method.Invoke(form, [args]);

            // Assert
            args.Cancel.Should().BeTrue();
            archive.Performances.OpenPage(1).Scores.Should().BeEmpty();
            grid.CancelEdit();
        });

        [Theory]
        [InlineData("7.8", false, 78)]
        [InlineData("0", false, 0)]
        [InlineData("", false, null)]
        [InlineData("7.88", true, null)]
        [InlineData("11", true, null)]
        public Task ValidateScore_WhenEditingCell_SavesOnlyValidInput(string text, bool canceled, int? expected) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = archive.CreateForm();
            var args = Activator.CreateInstance(typeof(DataGridViewCellValidatingEventArgs), BindingFlags.Instance | BindingFlags.NonPublic, null, [2, 0, text], null)
                as DataGridViewCellValidatingEventArgs ?? throw new InvalidOperationException();
            var method = typeof(ScoreEntryForm).GetMethod("ValidateScore", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

            // Act
            method.Invoke(form, [null, args]);

            // Assert
            args.Cancel.Should().Be(canceled);
            archive.Alerts.Should().HaveCount(canceled ? 1 : 0);
            var scores = archive.Performances.OpenPage(1).Scores;
            if (expected.HasValue)
            {
                scores.Should().ContainSingle().Which.Tenths.Should().Be(expected.Value);
            }
            else
            {
                scores.Should().BeEmpty();
            }
        });

        [Fact]
        public Task LoadPage_WhenJudgeAbsent_DisablesOnlyAbsentColumns() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();

            // Act
            using var form = archive.CreateForm();

            // Assert
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.RowCount.Should().Be(2);
            grid.ColumnCount.Should().Be(13);
            grid.Rows[0].Cells.Cast<DataGridViewCell>().Where(cell => cell.OwningColumn?.Tag is ScoreColumn).Count(cell => cell.ReadOnly).Should().Be(3);
        });

        [Theory]
        [InlineData(false, 0, 3)]
        [InlineData(true, 1, 2)]
        public Task Navigate_WhenEnterPressed_UsesConfiguredDirection(bool nextPerformance, int row, int column) => WinFormsTest.Run(() =>
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            using var form = archive.CreateForm();
            var grid = form.Controls.OfType<ScoreGrid>().Single();
            grid.EnterNextPerformance = nextPerformance;
            grid.CurrentCell = grid.Rows[0].Cells[2];
            var method = typeof(ScoreGrid).GetMethod("Navigate", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException();

            // Act
            method.Invoke(grid, [Keys.Enter]);

            // Assert
            grid.CurrentCell.Should().BeSameAs(grid.Rows[row].Cells[column]);
        });

        [Fact]
        public void Save_WhenPreferencesChanged_PersistsBothOptions()
        {
            // Arrange
            using var archive = new ScoreFormArchive();
            var preferences = new ScorePreferences(true, true);

            // Act
            archive.Preferences.Save(preferences);

            // Assert
            archive.Preferences.Load().Should().Be(preferences);
        }
    }
}

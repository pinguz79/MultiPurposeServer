using System.Data.Common;

using Skating.Core;

namespace Skating.Desktop.Choreographies
{
    internal sealed partial class ChoreographyForm : Form
    {
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _store;
        private IReadOnlyList<Choreography> _entries = [];

        public ChoreographyForm(IRegistryStore registry, IChoreographyStore store)
        {
            _registry = registry;
            _store = store;
            InitializeComponent();
        }

        private void ChoreographyFormShown(object? sender, EventArgs e) => RefreshRows();

        private void RefreshButtonClick(object? sender, EventArgs e) => RefreshRows();

        private void RefreshRows()
        {
            try
            {
                var selectedId = (entriesGrid.CurrentRow?.DataBoundItem as ChoreographyRow)?.Id;
                _entries = _store.GetChoreographies();
                var schools = _registry.GetSchools().ToDictionary(entry => entry.Id, entry => entry.Name);
                var disciplines = _registry.GetDisciplines().ToDictionary(entry => entry.Id, entry => entry.Name);
                var categories = _registry.GetCategories().ToDictionary(entry => entry.Id, entry => entry.Name);
                var levels = _registry.GetLevels().ToDictionary(entry => entry.Id, entry => entry.Name);
                var athletes = _registry.GetAthletes().ToDictionary(entry => entry.Id, entry => $"{entry.LastName} {entry.FirstName}");
                List<ChoreographyRow> rows = [.. _entries.Select(entry => new ChoreographyRow(entry.Id, entry.Name, schools[entry.SchoolId], disciplines[entry.DisciplineId],
                    categories[entry.CategoryId], levels[entry.LevelId], CompositionName(entry.Composition), $"{entry.DurationSeconds / 60}:{entry.DurationSeconds % 60:00}",
                    string.Join(", ", entry.AthleteIds.Select(id => athletes[id]))))];
                entriesGrid.DataSource = rows;
                foreach (DataGridViewRow row in entriesGrid.Rows)
                {
                    if (row.DataBoundItem is ChoreographyRow entry && entry.Id == selectedId)
                    {
                        entriesGrid.CurrentCell = row.Cells[0];
                        break;
                    }
                }

                var participants = _entries.SelectMany(entry => entry.AthleteIds).Distinct().Count();
                var multipleSchools = _entries.SelectMany(entry => entry.AthleteIds.Select(id => (Athlete: id, School: entry.SchoolId)))
                    .GroupBy(entry => entry.Athlete).Count(group => group.Select(entry => entry.School).Distinct().Count() > 1);
                summaryLabel.Text = $"{rows.Count} coreografie — {participants} atleti partecipanti distinti / {athletes.Count} in anagrafica — {multipleSchools} atleti rappresentano più scuole";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Impossibile aggiornare le coreografie.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal static string CompositionName(CompositionKind composition) => composition switch
        {
            CompositionKind.Solo => "Assolo",
            CompositionKind.Duo => "Duo",
            CompositionKind.Group => "Gruppo",
            _ => throw new ArgumentOutOfRangeException(nameof(composition)),
        };

        private void NewButtonClick(object? sender, EventArgs e) => Edit(null);

        private void EditButtonClick(object? sender, EventArgs e)
        {
            if (entriesGrid.CurrentRow?.DataBoundItem is ChoreographyRow row)
            {
                Edit(_entries.Single(entry => entry.Id == row.Id));
            }
        }

        private void EntriesGridCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                entriesGrid.CurrentCell = entriesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                EditButtonClick(sender, EventArgs.Empty);
            }
        }

        private void Edit(Choreography? entry)
        {
            using var dialog = new ChoreographyDialog(_registry, _store, entry);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                RefreshRows();
            }
        }
    }
}

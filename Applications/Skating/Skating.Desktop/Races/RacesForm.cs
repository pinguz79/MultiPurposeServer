using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Rankings;

namespace Skating.Desktop.Races
{
    internal sealed partial class RacesForm : Form
    {
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private readonly IRaceStore _store;
        private readonly Action<int>? _openScores;
        private IReadOnlyList<Race> _races = [];

        public RacesForm(IRegistryStore registry, IChoreographyStore choreographies, IRaceStore store,
            Action<int>? openScores = null)
        {
            _registry = registry;
            _choreographies = choreographies;
            _store = store;
            _openScores = openScores;
            InitializeComponent();
            var rankingButton = new Button { Text = "Classifica...", AutoSize = true };
            rankingButton.Click += (_, _) => OpenRanking();
            actionsPanel.Controls.Add(rankingButton);
        }

        private void RacesFormShown(object? sender, EventArgs e) => RefreshRows();

        private void RefreshButtonClick(object? sender, EventArgs e) => RefreshRows();

        private void RefreshRows()
        {
            try
            {
                var selected = (entriesGrid.CurrentRow?.DataBoundItem as RaceRow)?.Key;
                _races = _store.GetRaces();
                var disciplines = _registry.GetDisciplines().ToDictionary(entry => entry.Id, entry => entry.Name);
                var categories = _registry.GetCategories().ToDictionary(entry => entry.Id, entry => entry.Name);
                var levels = _registry.GetLevels().ToDictionary(entry => entry.Id);
                var criteria = _registry.GetCriteria();
                var proCategories = _races.Where(race => levels[race.Key.LevelId].IsPro).Select(race => race.Key.CategoryId).Distinct().Count();
                List<RaceRow> rows = [.. _races.Select(race => new RaceRow(race.Key, disciplines[race.Key.DisciplineId], categories[race.Key.CategoryId], levels[race.Key.LevelId].Name,
                    race.ChoreographyIds.Count, race.JudgeIds.Count, race.HasCustomJury ? "Personalizzata" : "Tutti i giudici",
                    criteria.Count == 0 ? "Mancano criteri" : race.JudgeIds.Count < 3 ? "Giuria da completare" : proCategories > 1 && levels[race.Key.LevelId].IsPro ? "Verificare categoria unica PRO" : "Configurata"))
                    .OrderBy(row => row.Discipline).ThenBy(row => row.Category).ThenBy(row => row.Level)];
                entriesGrid.DataSource = rows;
                foreach (DataGridViewRow row in entriesGrid.Rows)
                {
                    if (row.DataBoundItem is RaceRow entry && entry.Key == selected)
                    {
                        entriesGrid.CurrentCell = row.Cells[0];
                        break;
                    }
                }

                summaryLabel.Text = $"{rows.Count} gare — Criteri comuni: {string.Join(", ", criteria.Select(criterion => criterion.Name))}\r\nClassifica sempre disponibile; chiusura esplicita dopo la conferma delle esibizioni.";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Impossibile caricare le gare.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void JuryButtonClick(object? sender, EventArgs e)
        {
            if (entriesGrid.CurrentRow?.DataBoundItem is not RaceRow row)
            {
                return;
            }

            try
            {
                var race = _races.Single(entry => entry.Key == row.Key);
                var names = _choreographies.GetChoreographies().Where(entry => race.ChoreographyIds.Contains(entry.Id)).Select(entry => entry.Name);
                using var dialog = new JuryDialog(_registry, _store, race, $"{row.Discipline} / {row.Category} / {row.Level}", names);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshRows();
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Impossibile aprire la giuria.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenRanking()
        {
            if (entriesGrid.CurrentRow?.DataBoundItem is not RaceRow row)
            {
                return;
            }

            try
            {
                using var dialog = new RankingForm(_store, _registry, _choreographies, row.Key, $"{row.Discipline} / {row.Category} / {row.Level}", _openScores);
                dialog.ShowDialog(this);
                RefreshRows();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EntriesGridCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                entriesGrid.CurrentCell = entriesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                JuryButtonClick(sender, EventArgs.Empty);
            }
        }
    }
}

using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Configuration;
using Skating.Desktop.Scoring;

namespace Skating.Desktop.Scheduling
{
    internal sealed class ScheduleForm : Form
    {
        private readonly IPerformanceStore _store;
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private readonly ScorePreferencesStore _preferences;
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        };
        private readonly NumericUpDown _size = new() { Minimum = 1, Maximum = 999, Value = 20, Width = 65 };
        private readonly NumericUpDown _block = new() { Minimum = 1, Maximum = 9999, Width = 65 };
        private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 42 };

        public ScheduleForm(IPerformanceStore store, IRegistryStore registry,
            IChoreographyStore choreographies, ScorePreferencesStore preferences, IRaceStore? races = null)
        {
            _store = store;
            _registry = registry;
            _choreographies = choreographies;
            _preferences = preferences;
            Text = "Scaletta e votazioni";
            ClientSize = new Size(1100, 650);
            StartPosition = FormStartPosition.CenterParent;
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            toolbar.Controls.Add(new Label { Text = "Esibizioni per blocco", AutoSize = true });
            toolbar.Controls.Add(_size);
            AddAction(toolbar, "Prepara blocchi", () => _store.Prepare((int)_size.Value));
            AddAction(toolbar, "↑", () => _store.Move(Selected().ChoreographyId, -1));
            AddAction(toolbar, "↓", () => _store.Move(Selected().ChoreographyId, 1));
            toolbar.Controls.Add(_block);
            AddAction(toolbar, "Assegna blocco", () => _store.AssignBlock(Selected().ChoreographyId, (int)_block.Value));
            AddAction(toolbar, "Conferma blocchi", () =>
            {
                if (Ask("Confermare i blocchi? Numeri e appartenenza ai blocchi diventeranno stabili."))
                {
                    _store.ConfirmBlocks();
                }
            });
            AddAction(toolbar, "Avvia manifestazione", () =>
            {
                if (Ask("Avviare la manifestazione? Non sarà più possibile modificare i criteri o aggiungere giudici."))
                {
                    _store.Start();
                }
            });
            AddAction(toolbar, "Scheda voti…", () =>
            {
                var block = Selected().Block;
                _store.OpenPage(block);
                using var dialog = new ScoreEntryForm(_store, _registry, _choreographies, block, _preferences, races: races);
                dialog.ShowDialog(this);
            });
            Controls.Add(_grid);
            Controls.Add(_status);
            Controls.Add(toolbar);
            RefreshSchedule();
        }

        private Performance Selected() => _grid.CurrentRow?.Tag as Performance ?? throw new InvalidOperationException("Selezionare un'esibizione.");

        private bool Ask(string message) => MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private void AddAction(FlowLayoutPanel toolbar, string text, Action action)
        {
            var button = new Button { Text = text, AutoSize = true };
            button.Click += (_, _) =>
            {
                try
                {
                    action();
                    RefreshSchedule();
                }
                catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or DbException or IOException or UnauthorizedAccessException)
                {
                    MessageBox.Show(this, exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            toolbar.Controls.Add(button);
        }

        private void RefreshSchedule()
        {
            var selectedId = (_grid.CurrentRow?.Tag as Performance)?.ChoreographyId;
            var schedule = _store.GetSchedule();
            var choreographies = _choreographies.GetChoreographies().ToDictionary(entry => entry.Id);
            var disciplines = _registry.GetDisciplines().ToDictionary(entry => entry.Id, entry => entry.Name);
            var categories = _registry.GetCategories().ToDictionary(entry => entry.Id, entry => entry.Name);
            var levels = _registry.GetLevels().ToDictionary(entry => entry.Id, entry => entry.Name);
            _grid.Rows.Clear();
            if (_grid.Columns.Count == 0)
            {
                foreach (var title in new[] { "Numero", "Blocco", "Coreografia", "Gara", "Stato blocco" })
                {
                    _grid.Columns.Add(title, title);
                }
            }

            foreach (var entry in schedule.Performances)
            {
                var choreography = choreographies[entry.ChoreographyId];
                var race = $"{disciplines[choreography.DisciplineId]} / {categories[choreography.CategoryId]} / {levels[choreography.LevelId]}";
                var row = _grid.Rows[_grid.Rows.Add(entry.Number, entry.Block, choreography.Name,
                    race, schedule.ConfirmedBlocks.Contains(entry.Block) ? "Confermato" : "Da confermare")];
                row.Tag = entry;
                if (entry.ChoreographyId == selectedId)
                {
                    _grid.CurrentCell = row.Cells[0];
                }
            }

            _status.Text = schedule.IsStarted ? "Manifestazione avviata. Aprire la scheda voti del blocco selezionato."
                : "Preparare i blocchi, eventualmente riordinare/riassegnare, confermare e avviare la manifestazione.";
        }
    }
}

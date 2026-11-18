using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Choreographies;
using Skating.Desktop.Common;
using Skating.Desktop.Configuration;
using Skating.Desktop.Races;
using Skating.Desktop.Scoring;

namespace Skating.Desktop.Scheduling
{
    internal sealed class ScheduleForm : PopupForm
    {
        private readonly IPerformanceStore _store;
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private readonly ScorePreferencesStore _preferences;
        private readonly IRaceStore? _races;
        private readonly Func<string, bool> _confirm;
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        };
        private readonly NumericUpDown _size = new() { Minimum = 1, Maximum = 999, Value = 20, Width = 65 };
        private readonly NumericUpDown _block = new() { Minimum = 1, Maximum = 9999, Width = 65 };
        private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 42 };

        public ScheduleForm(IPerformanceStore store, IRegistryStore registry,
            IChoreographyStore choreographies, ScorePreferencesStore preferences, IRaceStore? races = null,
            OperatingMode mode = OperatingMode.Registry, Func<string, bool>? confirm = null)
        {
            _store = store;
            _registry = registry;
            _choreographies = choreographies;
            _preferences = preferences;
            _races = races;
            _confirm = confirm ?? (message => MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
            Text = "Scaletta e votazioni";
            ClientSize = new Size(1100, 650);
            StartPosition = FormStartPosition.CenterParent;
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            toolbar.Controls.Add(new Label { Text = "Esibizioni per blocco", AutoSize = true });
            toolbar.Controls.Add(_size);
            AddAction(toolbar, "Prepara blocchi", PrepareBlocks);
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
            Action openScores = () =>
            {
                var block = Selected().Block;
                _store.OpenPage(block);
                using var dialog = new ScoreEntryForm(_store, _registry, _choreographies, block, _preferences, races: races);
                OperatingModeAppearance.Apply(dialog, OperatingMode.Competition);
                dialog.ShowDialog(this);
            };
            AddAction(toolbar, "Scheda voti…", openScores);
            _grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return;
                }

                _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                Execute(() =>
                {
                    if (mode == OperatingMode.Competition)
                    {
                        openScores();
                    }
                    else
                    {
                        var choreography = _choreographies.GetChoreographies().Single(entry => entry.Id == Selected().ChoreographyId);
                        using var dialog = new ChoreographyDialog(_registry, _choreographies, choreography);
                        dialog.ShowDialog(this);
                    }
                });
            };
            Controls.Add(_grid);
            Controls.Add(_status);
            Controls.Add(toolbar);
            OperatingModeAppearance.Apply(this, mode);
            RefreshSchedule();
            _size.Value = Math.Clamp(_store.GetSchedule().BlockSize, (int)_size.Minimum, (int)_size.Maximum);
        }

        private void PrepareBlocks()
        {
            var schedule = _store.GetSchedule();
            if (schedule.IsStarted)
            {
                throw new InvalidOperationException("La manifestazione è avviata: non è possibile rigenerare i blocchi.");
            }

            if (schedule.Performances.Count > 0 && !Ask("Rigenerare i blocchi? Le assegnazioni manuali e le conferme dei blocchi saranno sostituite. L'ordine della scaletta sarà conservato."))
            {
                return;
            }

            _store.Prepare((int)_size.Value);
        }

        private Performance Selected() => _grid.CurrentRow?.Tag as Performance ?? throw new InvalidOperationException("Selezionare un'esibizione.");

        private bool Ask(string message) => _confirm(message);

        private void AddAction(FlowLayoutPanel toolbar, string text, Action action)
        {
            var button = new Button { Text = text, AutoSize = true };
            button.Click += (_, _) => Execute(action);
            toolbar.Controls.Add(button);
        }

        private void Execute(Action action)
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
        }

        private void RefreshSchedule()
        {
            var selectedId = (_grid.CurrentRow?.Tag as Performance)?.ChoreographyId;
            var schedule = _store.GetSchedule();
            var choreographies = _choreographies.GetChoreographies().ToDictionary(entry => entry.Id);
            var disciplines = _registry.GetDisciplines().ToDictionary(entry => entry.Id, entry => entry.Name);
            var categories = _registry.GetCategories().ToDictionary(entry => entry.Id, entry => entry.Name);
            var levels = _registry.GetLevels().ToDictionary(entry => entry.Id, entry => entry.Name);
            var colors = _races?.GetColors();
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
                if (colors is not null)
                {
                    RaceColorAppearance.Apply(row.Cells[3], new RaceKey(choreography.DisciplineId, choreography.CategoryId, choreography.LevelId), colors);
                }
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

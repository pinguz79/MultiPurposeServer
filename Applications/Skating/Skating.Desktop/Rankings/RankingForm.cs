using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Common;
using Skating.Desktop.Races;

namespace Skating.Desktop.Rankings
{
    internal sealed class RankingForm : PopupForm
    {
        private readonly IRaceStore _store;
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private RaceKey _key;
        private readonly Action<int>? _openScores;
        private readonly Label _summary = new() { Dock = DockStyle.Top, Height = 70, Padding = new Padding(8) };
        private readonly DataGridView _entries = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly RankingScoreCard _scores = new() { Dock = DockStyle.Fill };
        private readonly Button _close = new() { Text = "Chiudi gara", AutoSize = true };
        private readonly Button _reopen = new() { Text = "Riapri gara", AutoSize = true };
        private RaceRanking _ranking;
        private readonly ComboBox _view = PodiumAppearance.CreateSelector();

        public RankingForm(IRaceStore store, IRegistryStore registry,
            IChoreographyStore choreographies, RaceKey key, string title, Action<int>? openScores = null, bool selectRace = false)
        {
            _store = store;
            _registry = registry;
            _choreographies = choreographies;
            _key = key;
            _openScores = openScores;
            _ranking = store.GetRanking(key);
            Text = $"Classifica — {title}";
            ClientSize = new Size(1120, 730);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            toolbar.Controls.Add(_close);
            toolbar.Controls.Add(_reopen);
            toolbar.Controls.Add(_view);
            _view.SelectedIndexChanged += (_, _) => Run(RefreshRanking);
            RankingPrintActions.Add(toolbar, this, _entries, () => Text, () => _ranking.IsClosed ? "DEFINITIVA" : "PROVVISORIA", _view);
            _close.Click += (_, _) => Run(() => ChangeClosure(true));
            _reopen.Click += (_, _) => Run(() => ChangeClosure(false));
            AddAction(toolbar, "Scheda voti...", OpenScores);
            AddAction(toolbar, "Non presentata", () => ChangeStatus(PerformanceStatus.NotPresented));
            AddAction(toolbar, "Squalifica", () => ChangeStatus(PerformanceStatus.Disqualified));
            AddAction(toolbar, "Revoca esclusione", () => ChangeStatus(PerformanceStatus.Active));
            AddAction(toolbar, "Aggiorna", RefreshRanking);
            AddAction(toolbar, "Colori e legenda...", () =>
            {
                using var dialog = new RaceColorsForm(_store, _registry);
                dialog.ShowDialog(this);
                RefreshRanking();
            });
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Size = new Size(1100, 550), SplitterDistance = 250 };
            split.Panel1.Controls.Add(_entries);
            split.Panel2.Controls.Add(_scores);
            var detailLabel = new Label { Text = "Voti originali della riga selezionata — in grigio quelli non conteggiati in questa classifica", Dock = DockStyle.Top, Height = 28 };
            split.Panel2.Controls.Add(detailLabel);
            AddColumn(_entries, "Position", "Pos.", 55);
            AddColumn(_entries, "Number", "N.", 55);
            AddColumn(_entries, "Name", "Coreografia", 240);
            AddColumn(_entries, "School", "Scuola", 180);
            AddColumn(_entries, "Total", "Punti", 75);
            AddColumn(_entries, "Status", "Stato", 180);
            AddColumn(_entries, "Ties", "Parità", 130);
            _entries.SelectionChanged += (_, _) => ShowScores();
            _entries.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) { Run(OpenScores); } };
            Controls.Add(split);
            Controls.Add(_summary);
            Controls.Add(toolbar);
            if (selectRace)
            {
                var disciplines = registry.GetDisciplines().ToDictionary(entry => entry.Id, entry => entry.Name);
                var categories = registry.GetCategories().ToDictionary(entry => entry.Id, entry => entry.Name);
                var levels = registry.GetLevels().ToDictionary(entry => entry.Id, entry => entry.Name);
                var choices = store.GetRaces().Select(race => new RaceChoice(race.Key,
                    $"{disciplines[race.Key.DisciplineId]} / {categories[race.Key.CategoryId]} / {levels[race.Key.LevelId]}"))
                    .OrderBy(race => race.Name).ToList();
                var selector = new ComboBox { Name = "raceSelector", DropDownStyle = ComboBoxStyle.DropDownList, Width = 600, DisplayMember = nameof(RaceChoice.Name) };
                selector.Items.AddRange([.. choices]);
                selector.SelectedItem = choices.Single(race => race.Key == key);
                selector.SelectedIndexChanged += (_, _) => Run(() =>
                {
                    if (selector.SelectedItem is RaceChoice race)
                    {
                        _key = race.Key;
                        Text = $"Classifica — {race.Name}";
                        RefreshRanking();
                    }
                });
                var selectionBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
                selectionBar.Controls.Add(new Label { Text = "Gara", AutoSize = true });
                selectionBar.Controls.Add(selector);
                Controls.Add(selectionBar);
            }

            RefreshRanking();
        }

        private static void AddColumn(DataGridView grid, string name, string title, int width) => grid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = title, Width = width, SortMode = DataGridViewColumnSortMode.NotSortable });

        private void AddAction(FlowLayoutPanel panel, string text, Action action)
        {
            var button = new Button { Text = text, AutoSize = true };
            button.Click += (_, _) => Run(action);
            panel.Controls.Add(button);
        }

        private void Run(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or DbException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private RankingEntry Selected() => _entries.CurrentRow?.Tag as RankingEntry ?? throw new InvalidOperationException("Selezionare un'esibizione.");

        private bool Ask(string message) => MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private void ChangeClosure(bool close)
        {
            if (!Ask(close ? "Confermare la classifica e chiudere la gara?" : "Riaprire la gara? La classifica tornerà provvisoria; le schede confermate resteranno protette."))
            {
                return;
            }

            if (close)
            {
                _store.Close(_key);
            }
            else
            {
                _store.Reopen(_key);
            }

            RefreshRanking();
        }

        private void ChangeStatus(PerformanceStatus status)
        {
            var entry = Selected();
            if (Ask(status == PerformanceStatus.Active ? "Revocare l'esclusione? I voti saranno conservati e la scheda dovrà essere nuovamente confermata." : "Escludere l'esibizione dalla classifica? I voti saranno conservati e l'operazione sarà reversibile."))
            {
                _store.SetStatus(entry.ChoreographyId, status);
                RefreshRanking();
            }
        }

        private void OpenScores()
        {
            if (_openScores is null || Selected().Block is not int block)
            {
                throw new InvalidOperationException("Preparare e avviare la scaletta per accedere alle schede voti.");
            }

            _openScores(block);
            RefreshRanking();
        }

        private void RefreshRanking()
        {
            var selected = (_entries.CurrentRow?.Tag as RankingEntry)?.ChoreographyId;
            _ranking = _store.GetRanking(_key);
            var choreographies = _choreographies.GetChoreographies().ToDictionary(entry => entry.Id);
            var schools = _registry.GetSchools().ToDictionary(entry => entry.Id, entry => entry.Name);
            _entries.Rows.Clear();
            var colors = _store.GetColors();
            foreach (var entry in _ranking.Entries)
            {
                if (_view.SelectedIndex == 1 && !PodiumAppearance.IsOnPodium(entry))
                {
                    continue;
                }

                var choreography = choreographies[entry.ChoreographyId];
                var state = entry.Status switch
                {
                    PerformanceStatus.NotPresented => "Non presentata",
                    PerformanceStatus.Disqualified => "Squalificata",
                    _ => entry.IsConfirmed ? "Confermata" : entry.IsComplete ? "Completa, aperta" : entry.Scores.Count == 0 ? "Senza voti" : "Incompleta",
                };
                var row = _entries.Rows[_entries.Rows.Add(entry.Position?.ToString() ?? "—", entry.Number?.ToString() ?? "—", choreography.Name, schools[choreography.SchoolId],
                    entry.TotalTenths is int total ? (total / 10m).ToString("0.0") : "—", state, entry.IsTied ? entry.Position <= 3 ? "Parità sul podio" : "Parità" : "")];
                row.Tag = entry;
                RaceColorAppearance.Apply(row.Cells["Name"], _key, colors);
                row.DefaultCellStyle.BackColor = entry.IsTied ? Color.Moccasin : entry.Status != PerformanceStatus.Active ? Color.Gainsboro : !entry.IsConfirmed ? Color.AliceBlue : Color.White;
                PodiumAppearance.Apply(row, entry);
                if (entry.ChoreographyId == selected)
                {
                    _entries.CurrentCell = row.Cells[0];
                }
            }

            _close.Enabled = _ranking.CanClose;
            _reopen.Enabled = _ranking.IsClosed;
            _summary.Text = $"{(_ranking.IsClosed ? "DEFINITIVA" : "PROVVISORIA")} · {_ranking.Entries.Count} esibizioni\r\n"
                + (_ranking.HasPodiumTies ? "Risolvere le parità sul podio correggendo un voto su indicazione dei giudici."
                    : _ranking.CanClose ? "Tutte le esibizioni sono gestite: la gara può essere chiusa." : _ranking.IsClosed ? "Gara chiusa. Riaprirla prima di apportare correzioni." : "Confermare le schede complete oppure indicare le esibizioni non presentate/squalificate.");
            ShowScores();
        }

        private void ShowScores()
        {
            _scores.Clear();
            if (_entries.CurrentRow?.Tag is not RankingEntry entry)
            {
                return;
            }

            _scores.ShowEntry(entry, _choreographies.GetChoreographies().Single(choreography => choreography.Id == entry.ChoreographyId).Name, _registry);
        }
    }
}

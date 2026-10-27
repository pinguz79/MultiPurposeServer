using System.Data.Common;

using Skating.Core;

namespace Skating.Desktop.Rankings
{
    internal sealed class AbsoluteAwardForm : Form
    {
        private readonly IAbsoluteAwardStore _store;
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private readonly AbsoluteAwardKind _kind;
        private readonly AwardAggregation? _aggregation;
        private readonly Action<RaceKey> _openRace;
        private readonly Label _status = new() { Dock = DockStyle.Top, Height = 72, Padding = new Padding(8) };
        private readonly ComboBox _races = new() { Width = 410, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _validate = new() { Text = "Convalida assoluta", AutoSize = true };
        private readonly Button _reopen = new() { Text = "Riapri assoluta", AutoSize = true };
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
            RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White,
        };
        private readonly RichTextBox _details = new() { Dock = DockStyle.Bottom, Height = 170, ReadOnly = true, BackColor = Color.White };
        private AbsoluteAward _award;

        public AbsoluteAwardForm(IAbsoluteAwardStore store, IRegistryStore registry,
            IChoreographyStore choreographies, AbsoluteAwardKind kind, AwardAggregation? aggregation,
            Action<RaceKey> openRace, Action openIntermediates)
        {
            _store = store;
            _registry = registry;
            _choreographies = choreographies;
            _kind = kind;
            _aggregation = aggregation;
            _openRace = openRace;
            _award = store.GetAward(kind, aggregation);
            Text = kind == AbsoluteAwardKind.Pro ? "Premio assoluto PRO" : "Premio assoluto non PRO";
            ClientSize = new Size(1140, 740);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterParent;
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            toolbar.Controls.Add(_validate);
            toolbar.Controls.Add(_reopen);
            _validate.Click += (_, _) => Run(() => ChangeValidation(true));
            _reopen.Click += (_, _) => Run(() => ChangeValidation(false));
            if (kind == AbsoluteAwardKind.NonPro)
            {
                AddAction(toolbar, "Premi intermedi...", () => { openIntermediates(); RefreshAward(); });
            }

            AddAction(toolbar, "Aggiorna", RefreshAward);
            var sources = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            sources.Controls.Add(_races);
            AddAction(sources, "Apri gara / correggi voti...", OpenSource);
            string[] titles = ["Pos.", "Coreografia", "Scuola", "Qualificazione", "Punti", "Parità"];
            int[] widths = [50, 220, 170, 320, 90, 120];
            for (var index = 0; index < titles.Length; index++)
            {
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = titles[index], Width = widths[index], SortMode = DataGridViewColumnSortMode.NotSortable });
            }

            _grid.SelectionChanged += (_, _) => ShowDetails();
            Controls.Add(_grid);
            Controls.Add(_details);
            Controls.Add(_status);
            Controls.Add(sources);
            Controls.Add(toolbar);
            RefreshAward();
        }

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

        private string RaceName(RaceKey key) => $"{_registry.GetDisciplines().Single(entry => entry.Id == key.DisciplineId).Name} / {_registry.GetCategories().Single(entry => entry.Id == key.CategoryId).Name} / {_registry.GetLevels().Single(entry => entry.Id == key.LevelId).Name}";

        private void RefreshAward()
        {
            var selected = (_grid.CurrentRow?.Tag as RankingEntry)?.ChoreographyId;
            _award = _store.GetAward(_kind, _aggregation);
            _grid.Rows.Clear();
            _races.Items.Clear();
            foreach (var race in _award.RaceSources)
            {
                _races.Items.Add($"{RaceName(race.Key)} — {(race.IsClosed ? "definitiva" : "provvisoria")}");
            }

            _races.SelectedIndex = _races.Items.Count > 0 ? 0 : -1;
            var entries = _choreographies.GetChoreographies().ToDictionary(entry => entry.Id);
            var schools = _registry.GetSchools().ToDictionary(entry => entry.Id, entry => entry.Name);
            foreach (var entry in _award.Entries)
            {
                var choreography = entries[entry.ChoreographyId];
                var source = _award.RaceSources.Single(race => race.Entries.Any(candidate => candidate.ChoreographyId == entry.ChoreographyId));
                var qualification = _kind == AbsoluteAwardKind.Pro ? RaceName(source.Key)
                    : _award.IntermediateSources.Single(award => award.Entries.Any(candidate => candidate.ChoreographyId == entry.ChoreographyId)).Name;
                var row = _grid.Rows[_grid.Rows.Add(entry.Position?.ToString() ?? "—", choreography.Name, schools[choreography.SchoolId], qualification,
                    entry.TotalTenths is int total ? (total / 10m).ToString("0.0") : "—", entry.IsTied ? entry.Position <= 3 ? "Sul podio" : "Parità" : "")];
                row.Tag = entry;
                row.DefaultCellStyle.BackColor = entry.IsTied ? Color.Moccasin : Color.White;
                if (entry.ChoreographyId == selected)
                {
                    _grid.CurrentCell = row.Cells[0];
                }
            }

            _validate.Enabled = _award.CanValidate;
            _reopen.Enabled = _award.IsValidated;
            var sourceType = _kind == AbsoluteAwardKind.Pro ? "gare" : "premi intermedi";
            _status.Text = $"{(_award.IsValidated ? "DEFINITIVA" : "PROVVISORIA")} · {_award.Entries.Count} partecipanti · {_award.ValidatedSourceCount}/{_award.SourceCount} {sourceType} definitivi\r\n"
                + (!_award.AreSourcesValidated ? "Qualificazioni provvisorie: completare e convalidare tutte le fonti."
                    : _award.HasPodiumTies ? "Risolvere le parità sul podio correggendo i voti originali, poi convalidare nuovamente le fonti e l'assoluta."
                    : _award.IsValidated ? "Assoluta convalidata." : _award.Entries.Count == 0 ? "Nessun vincitore ammesso." : "Assoluta pronta per verifica e convalida.");
            ShowDetails();
        }

        private void ShowDetails()
        {
            _details.Clear();
            if (_grid.CurrentRow?.Tag is not RankingEntry entry)
            {
                return;
            }

            var judges = _registry.GetJudges().ToDictionary(judge => judge.Id, judge => judge.Name);
            var criteria = _registry.GetCriteria().ToDictionary(criterion => criterion.Id, criterion => criterion.Name);
            _details.SelectionColor = Color.Black;
            _details.AppendText("Voti originali — conteggio autonomo dell'assoluta.\n");
            foreach (var score in entry.Scores.OrderBy(score => judges[score.JudgeId]).ThenBy(score => criteria[score.CriterionId]))
            {
                var excluded = entry.ExcludedJudgeIds.Contains(score.JudgeId);
                _details.SelectionColor = excluded ? Color.Gray : Color.Black;
                _details.AppendText($"{judges[score.JudgeId]} · {criteria[score.CriterionId]}: {ScoreValue.Format(score.Tenths)}{(excluded ? " — NON CONTEGGIATO" : "")}\n");
            }

            _races.SelectedIndex = _award.RaceSources.ToList().FindIndex(source => source.Entries.Any(candidate => candidate.ChoreographyId == entry.ChoreographyId));
        }

        private void OpenSource()
        {
            if (_races.SelectedIndex >= 0)
            {
                _openRace(_award.RaceSources[_races.SelectedIndex].Key);
                RefreshAward();
            }
        }

        private void ChangeValidation(bool validate)
        {
            if (MessageBox.Show(this, validate ? "Convalidare la classifica assoluta dopo la verifica?" : "Rendere nuovamente provvisoria questa assoluta? Le fonti resteranno invariate.",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (validate)
            {
                _store.Validate(_kind, _aggregation);
            }
            else
            {
                _store.Reopen(_kind);
            }

            RefreshAward();
        }
    }
}

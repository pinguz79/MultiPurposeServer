using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Common;
using Skating.Desktop.Races;

namespace Skating.Desktop.Rankings
{
    internal sealed class IntermediateAwardsForm : PopupForm
    {
        private readonly IIntermediateAwardStore _store;
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private readonly AwardAggregation _aggregation;
        private readonly Action<RaceKey> _openRace;
        private readonly IRaceStore? _races;
        private readonly ComboBox _awards = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250, DisplayMember = "Name" };
        private readonly ComboBox _sources = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 380, Enabled = false };
        private readonly Label _status = new() { Dock = DockStyle.Top, Height = 65, Padding = new Padding(8) };
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
            RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White,
        };
        private readonly RankingScoreCard _details = new() { Dock = DockStyle.Bottom, Height = 260 };
        private readonly Button _validate = new() { Text = "Convalida premio", AutoSize = true };
        private readonly Button _reopen = new() { Text = "Riapri premio", AutoSize = true };
        private readonly ComboBox _view = PodiumAppearance.CreateSelector();

        private IntermediateAward? Selected => _awards.SelectedItem as IntermediateAward;

        public IntermediateAwardsForm(IIntermediateAwardStore store, IRegistryStore registry,
            IChoreographyStore choreographies, AwardAggregation aggregation, Action<RaceKey> openRace, IRaceStore? races = null)
        {
            _store = store;
            _registry = registry;
            _choreographies = choreographies;
            _aggregation = aggregation;
            _openRace = openRace;
            _races = races;
            Text = aggregation == AwardAggregation.Discipline ? "Premi intermedi per disciplina" : "Premi intermedi per categoria";
            ClientSize = new Size(1120, 720);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterParent;
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            toolbar.Controls.Add(_awards);
            toolbar.Controls.Add(_view);
            _view.SelectedIndexChanged += (_, _) => Run(ShowAward);
            RankingPrintActions.Add(toolbar, this, _grid, () => $"{Text} — {Selected?.Name}", () => Selected?.IsValidated == true ? "DEFINITIVA" : "PROVVISORIA", _view);
            toolbar.Controls.Add(_validate);
            toolbar.Controls.Add(_reopen);
            var refresh = new Button { Text = "Aggiorna", AutoSize = true };
            refresh.Click += (_, _) => Run(RefreshAwards);
            toolbar.Controls.Add(refresh);
            var sourceBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            sourceBar.Controls.Add(new Label { Text = "Gara di origine della riga selezionata", AutoSize = true });
            sourceBar.Controls.Add(_sources);
            var open = new Button { Text = "Apri gara / correggi voti...", AutoSize = true };
            open.Click += (_, _) => Run(OpenSource);
            sourceBar.Controls.Add(open);
            _validate.Click += (_, _) => Run(() => ChangeValidation(true));
            _reopen.Click += (_, _) => Run(() => ChangeValidation(false));
            _awards.SelectedIndexChanged += (_, _) => Run(ShowAward);
            _grid.Columns.Add("Position", "Pos.");
            _grid.Columns.Add("Name", "Coreografia");
            _grid.Columns.Add("School", "Scuola");
            _grid.Columns.Add("Source", "Gara di origine");
            _grid.Columns.Add("Total", "Punti");
            _grid.Columns.Add("Tie", "Parità");
            _grid.Columns[0].Width = 50;
            _grid.Columns[1].Width = 220;
            _grid.Columns[2].Width = 170;
            _grid.Columns[3].Width = 300;
            foreach (DataGridViewColumn column in _grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            _grid.SelectionChanged += (_, _) => ShowDetails();
            Controls.Add(_grid);
            Controls.Add(_details);
            Controls.Add(_status);
            Controls.Add(sourceBar);
            Controls.Add(toolbar);
            RefreshAwards();
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

        private void RefreshAwards()
        {
            var id = Selected?.GroupId;
            _awards.DataSource = _store.GetAwards(_aggregation).ToList();
            if (id is Guid selected && _awards.Items.Cast<IntermediateAward>().FirstOrDefault(award => award.GroupId == selected) is IntermediateAward previous)
            {
                _awards.SelectedItem = previous;
            }

            ShowAward();
        }

        private string RaceName(RaceKey key) => $"{_registry.GetDisciplines().Single(entry => entry.Id == key.DisciplineId).Name} / {_registry.GetCategories().Single(entry => entry.Id == key.CategoryId).Name} / {_registry.GetLevels().Single(entry => entry.Id == key.LevelId).Name}";

        private void ShowAward()
        {
            _grid.Rows.Clear();
            _sources.Items.Clear();
            var award = Selected;
            _validate.Enabled = award?.CanValidate == true;
            _reopen.Enabled = award?.IsValidated == true;
            if (award is null)
            {
                _status.Text = "Nessun premio intermedio: inserire gare non PRO. I PRO accederanno direttamente all'assoluta separata.";
                _details.Clear();
                return;
            }

            foreach (var race in award.Sources)
            {
                _sources.Items.Add($"{RaceName(race.Key)} — {(race.IsClosed ? "definitiva" : "provvisoria")}");
            }

            _sources.SelectedIndex = _sources.Items.Count > 0 ? 0 : -1;
            var choreographies = _choreographies.GetChoreographies().ToDictionary(entry => entry.Id);
            var schools = _registry.GetSchools().ToDictionary(entry => entry.Id, entry => entry.Name);
            var colors = _races?.GetColors();
            foreach (var entry in award.Entries)
            {
                if (_view.SelectedIndex == 1 && !PodiumAppearance.IsOnPodium(entry))
                {
                    continue;
                }

                var choreography = choreographies[entry.ChoreographyId];
                var source = award.Sources.Single(race => race.Entries.Any(candidate => candidate.ChoreographyId == entry.ChoreographyId));
                var row = _grid.Rows[_grid.Rows.Add(entry.Position?.ToString() ?? "—", choreography.Name, schools[choreography.SchoolId], RaceName(source.Key),
                    entry.TotalTenths is int total ? (total / 10m).ToString("0.0") : "—", entry.IsTied ? entry.Position <= 3 ? "Sul podio" : "Parità" : "")];
                row.Tag = entry;
                if (colors is not null)
                {
                    RaceColorAppearance.Apply(row.Cells[1], source.Key, colors);
                }
                row.DefaultCellStyle.BackColor = entry.IsTied ? Color.Moccasin : Color.White;
                PodiumAppearance.Apply(row, entry);
            }

            _status.Text = $"{(award.IsValidated ? "DEFINITIVA" : "PROVVISORIA")} · {award.Entries.Count} partecipanti · {award.Sources.Count(source => source.IsClosed)}/{award.Sources.Count} gare chiuse\r\n"
                + (!award.AreSourcesClosed ? "I primi provvisori possono cambiare: chiudere tutte le gare di origine."
                    : award.HasPodiumTies ? "Parità sul podio: correggere un voto originale su indicazione dei giudici, poi richiudere la gara e convalidare il premio."
                    : award.IsValidated ? "Premio convalidato." : award.Entries.Count == 0 ? "Nessun vincitore ammesso." : "Premio pronto per la verifica e convalida.");
            ShowDetails();
        }

        private void ShowDetails()
        {
            if (_grid.CurrentRow?.Tag is not RankingEntry entry)
            {
                _sources.SelectedIndex = -1;
                _details.Clear();
                return;
            }

            _details.ShowEntry(entry, _choreographies.GetChoreographies().Single(choreography => choreography.Id == entry.ChoreographyId).Name, _registry);

            if (Selected is IntermediateAward award)
            {
                _sources.SelectedIndex = award.Sources.ToList().FindIndex(source => source.Entries.Any(candidate => candidate.ChoreographyId == entry.ChoreographyId));
            }
        }

        private void OpenSource()
        {
            if (Selected is IntermediateAward award && _sources.SelectedIndex >= 0)
            {
                _openRace(award.Sources[_sources.SelectedIndex].Key);
                RefreshAwards();
            }
        }

        private void ChangeValidation(bool validate)
        {
            if (Selected is not IntermediateAward award || MessageBox.Show(this, validate ? "Convalidare il premio dopo la verifica dei risultati?" : "Riaprire il premio rendendolo provvisorio?",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (validate)
            {
                _store.Validate(_aggregation, award.GroupId);
            }
            else
            {
                _store.Reopen(_aggregation, award.GroupId);
            }

            RefreshAwards();
        }
    }
}

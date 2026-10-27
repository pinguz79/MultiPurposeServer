using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Common;
using Skating.Desktop.Registry;

namespace Skating.Desktop.Choreographies
{
    internal sealed partial class ChoreographyDialog : Form
    {
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _store;
        private readonly Choreography? _original;
        private readonly HashSet<Guid> _selectedAthletes = [];
        private IReadOnlyList<Athlete> _athletes = [];
        private Dictionary<Guid, NamedChoice> _athleteChoices = [];
        private bool _refreshingAthletes;
        private bool _loading = true;
        private bool _dirty;

        public ChoreographyDialog(IRegistryStore registry, IChoreographyStore store,
            Choreography? original = null)
        {
            _registry = registry;
            _store = store;
            _original = original;
            if (original is not null)
            {
                _selectedAthletes.UnionWith(original.AthleteIds);
            }
            InitializeComponent();
            Text = original is null ? "Nuova coreografia" : "Modifica coreografia";
            compositionCombo.Format += CompositionComboFormat;
            compositionCombo.DataSource = Enum.GetValues<CompositionKind>();
        }

        private void CompositionComboFormat(object? sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is CompositionKind composition)
            {
                e.Value = ChoreographyForm.CompositionName(composition);
            }
        }

        private void ChoreographyDialogShown(object? sender, EventArgs e)
        {
            try
            {
                PopulateCatalogs();
                if (_original is not null)
                {
                    nameTextBox.Text = _original.Name;
                    schoolCombo.SelectedValue = _original.SchoolId;
                    disciplineCombo.SelectedValue = _original.DisciplineId;
                    categoryCombo.SelectedValue = _original.CategoryId;
                    levelCombo.SelectedValue = _original.LevelId;
                    compositionCombo.SelectedItem = _original.Composition;
                    minutesInput.Value = _original.DurationSeconds / 60;
                    secondsInput.Value = _original.DurationSeconds % 60;
                }

                _loading = false;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Impossibile caricare le anagrafiche.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void PopulateCatalogs()
        {
            Bind(schoolCombo, _registry.GetSchools().Select(entry => new NamedChoice(entry.Id, entry.Name)));
            Bind(disciplineCombo, _registry.GetDisciplines().Select(entry => new NamedChoice(entry.Id, entry.Name)));
            Bind(categoryCombo, _registry.GetCategories().Select(entry => new NamedChoice(entry.Id, entry.Name)));
            Bind(levelCombo, _registry.GetLevels().Select(entry => new NamedChoice(entry.Id, entry.Name)));
            ReloadAthletes();
        }

        private void ReloadAthletes()
        {
            var entries = _store.GetChoreographies();
            var schools = _registry.GetSchools().ToDictionary(entry => entry.Id, entry => entry.Name);
            var athletes = _registry.GetAthletes();
            Dictionary<Guid, NamedChoice> choices = [];
            foreach (var athlete in athletes)
            {
                var participations = entries.Where(entry => entry.AthleteIds.Contains(athlete.Id)).ToList();
                var representedSchools = string.Join(", ", participations.Select(entry => schools[entry.SchoolId]).Distinct());
                var suffix = representedSchools.Length == 0 ? string.Empty : $" — {representedSchools}";
                var homonym = athletes.Count(other => string.Equals(other.FirstName, athlete.FirstName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(other.LastName, athlete.LastName, StringComparison.OrdinalIgnoreCase)) > 1;
                var identity = homonym ? $" [{athlete.Id.ToString()[..8]}]" : string.Empty;
                choices.Add(athlete.Id, new NamedChoice(athlete.Id, $"{athlete.LastName} {athlete.FirstName}{identity} — {participations.Count} coreografie{suffix}"));
            }

            _athletes = athletes;
            _athleteChoices = choices;
            FilterAthletes();
            hintLabel.Text = "Seleziona gli atleti (anche senza iscrizioni). Una scuola per coreografia; gli atleti possono rappresentarne altre in iscrizioni diverse.";
            saveButton.Enabled = schoolCombo.Items.Count > 0 && disciplineCombo.Items.Count > 0 && categoryCombo.Items.Count > 0 && levelCombo.Items.Count > 0 && _athletes.Count > 0;
            if (!saveButton.Enabled)
            {
                hintLabel.Text = "Prima di salvare occorrono scuole, atleti, discipline, categorie e livelli: inseriscili dal menu Anagrafiche.";
            }
        }

        private void AthleteFilterTextChanged(object? sender, EventArgs e) => FilterAthletes();

        private void FilterAthletes()
        {
            var terms = athleteFilterTextBox.Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _refreshingAthletes = true;
            athletesList.BeginUpdate();
            try
            {
                athletesList.Items.Clear();
                foreach (var athlete in _athletes.Where(athlete => terms.All(term => $"{athlete.FirstName} {athlete.LastName}".Contains(term, StringComparison.OrdinalIgnoreCase))))
                {
                    athletesList.Items.Add(_athleteChoices[athlete.Id], _selectedAthletes.Contains(athlete.Id));
                }
            }
            finally
            {
                athletesList.EndUpdate();
                _refreshingAthletes = false;
            }

            UpdateAthleteCount();
        }

        private void UpdateAthleteCount() => athleteCountLabel.Text = $"{athletesList.Items.Count} visibili / {_athletes.Count} — {_selectedAthletes.Count} selezionati (anche fuori filtro)";

        private void AddAthleteButtonClick(object? sender, EventArgs e)
        {
            using var dialog = new RegistryForm(_registry, RegistryKind.Athlete);
            dialog.ShowDialog(this);
            try
            {
                ReloadAthletes();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Impossibile aggiornare gli atleti.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Bind(ComboBox combo, IEnumerable<NamedChoice> choices)
        {
            combo.DisplayMember = nameof(NamedChoice.Name);
            combo.ValueMember = nameof(NamedChoice.Id);
            combo.DataSource = choices.ToList();
            combo.SelectedIndex = -1;
        }

        private void InputChanged(object? sender, EventArgs e)
        {
            if (!_loading)
            {
                _dirty = true;
            }
        }

        private void AthletesListItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_refreshingAthletes || athletesList.Items[e.Index] is not NamedChoice athlete)
            {
                return;
            }

            if (e.NewValue == CheckState.Checked)
            {
                _selectedAthletes.Add(athlete.Id);
            }
            else
            {
                _selectedAthletes.Remove(athlete.Id);
            }

            UpdateAthleteCount();
            InputChanged(sender, e);
        }

        private void SaveButtonClick(object? sender, EventArgs e)
        {
            try
            {
                if (schoolCombo.SelectedItem is not NamedChoice school || disciplineCombo.SelectedItem is not NamedChoice discipline
                    || categoryCombo.SelectedItem is not NamedChoice category || levelCombo.SelectedItem is not NamedChoice level || compositionCombo.SelectedItem is not CompositionKind composition)
                {
                    throw new ArgumentException("Selezionare scuola, disciplina, categoria, livello e composizione.");
                }

                var duration = checked((int)minutesInput.Value * 60 + (int)secondsInput.Value);
                var choreography = new Choreography(_original?.Id ?? Guid.NewGuid(), nameTextBox.Text, school.Id, discipline.Id, category.Id, level.Id, composition, duration,
                    _selectedAthletes);
                if (_original is null)
                {
                    _store.AddChoreography(choreography);
                }
                else
                {
                    _store.UpdateChoreography(choreography);
                }

                _dirty = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException or OverflowException)
            {
                MessageBox.Show(this, $"Salvataggio non completato.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ChoreographyDialogFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_dirty)
            {
                e.Cancel = MessageBox.Show(this, "Abbandonare le modifiche non salvate?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes;
            }
        }
    }
}

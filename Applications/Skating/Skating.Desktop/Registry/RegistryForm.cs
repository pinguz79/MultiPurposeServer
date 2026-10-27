using System.Data.Common;

using Skating.Core;

namespace Skating.Desktop.Registry
{
    internal sealed partial class RegistryForm : Form
    {
        private readonly IRegistryStore _store;
        private readonly RegistryKind _kind;
        private RegistryRow? _editingRow;

        private bool HasUnsavedChanges => _editingRow is null
            ? nameTextBox.Text.Length > 0 || lastNameTextBox.Text.Length > 0 || proCheckBox.Checked || absentCheckBox.Checked
            : nameTextBox.Text != _editingRow.Name || (_kind == RegistryKind.Athlete && lastNameTextBox.Text != _editingRow.Detail) || proCheckBox.Checked != _editingRow.IsPro || absentCheckBox.Checked != _editingRow.IsAbsent;

        public RegistryForm(IRegistryStore store, RegistryKind kind)
        {
            _store = store;
            _kind = kind;
            InitializeComponent();
            Text = kind switch
            {
                RegistryKind.Athlete => "Atleti",
                RegistryKind.Discipline => "Discipline",
                RegistryKind.Category => "Categorie",
                RegistryKind.Level => "Livelli",
                RegistryKind.School => "Scuole",
                RegistryKind.Judge => "Giudici",
                RegistryKind.Criterion => "Criteri di valutazione",
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
            lastNameLabel.Visible = lastNameTextBox.Visible = kind == RegistryKind.Athlete;
            proCheckBox.Visible = kind == RegistryKind.Level;
            absentCheckBox.Visible = kind == RegistryKind.Judge;
            detailColumn.Visible = kind is RegistryKind.Athlete or RegistryKind.Level or RegistryKind.Judge;
            detailColumn.HeaderText = kind == RegistryKind.Athlete ? "Cognome" : kind == RegistryKind.Judge ? "Presenza" : "Percorso";
            deleteButton.Visible = kind == RegistryKind.Criterion;
            hintLabel.Text = kind == RegistryKind.Athlete
                ? "Anagrafica completa, anche senza coreografie. Gli omonimi restano persone distinte."
                : "Elenco condiviso della manifestazione. I nomi non distinguono maiuscole e minuscole.";
        }

        private void RegistryFormShown(object? sender, EventArgs e) => RefreshRows();

        private void RefreshButtonClick(object? sender, EventArgs e) => RefreshRows();

        private void RefreshRows()
        {
            try
            {
                var selectedId = (entriesGrid.CurrentRow?.DataBoundItem as RegistryRow)?.Id;
                List<RegistryRow> rows = _kind switch
                {
                    RegistryKind.Athlete => [.. _store.GetAthletes().Select(athlete => new RegistryRow(athlete.Id, athlete.FirstName, athlete.LastName))],
                    RegistryKind.Discipline => [.. _store.GetDisciplines().Select(discipline => new RegistryRow(discipline.Id, discipline.Name, string.Empty))],
                    RegistryKind.Category => [.. _store.GetCategories().Select(category => new RegistryRow(category.Id, category.Name, string.Empty))],
                    RegistryKind.Level => [.. _store.GetLevels().Select(level => new RegistryRow(level.Id, level.Name, level.IsPro ? "PRO" : "Non PRO", level.IsPro))],
                    RegistryKind.School => [.. _store.GetSchools().Select(school => new RegistryRow(school.Id, school.Name, string.Empty))],
                    RegistryKind.Judge => [.. _store.GetJudges().Select(judge => new RegistryRow(judge.Id, judge.Name, judge.IsAbsent ? "Assente" : "Presente", IsAbsent: judge.IsAbsent))],
                    RegistryKind.Criterion => [.. _store.GetCriteria().Select(criterion => new RegistryRow(criterion.Id, criterion.Name, string.Empty))],
                    _ => throw new InvalidOperationException("Anagrafica non supportata."),
                };
                entriesGrid.DataSource = rows;
                if (selectedId is not null)
                {
                    foreach (DataGridViewRow row in entriesGrid.Rows)
                    {
                        if (row.DataBoundItem is RegistryRow entry && entry.Id == selectedId)
                        {
                            entriesGrid.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }

                countLabel.Text = $"{rows.Count} voci";
                UpdateEditButton();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or FormatException or ArgumentException)
            {
                MessageBox.Show(this, $"Impossibile aggiornare l'elenco.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddButtonClick(object? sender, EventArgs e)
        {
            statusLabel.Text = string.Empty;
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show(this, "Inserire il nome.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                nameTextBox.Focus();
                return;
            }

            if (_kind == RegistryKind.Athlete && string.IsNullOrWhiteSpace(lastNameTextBox.Text))
            {
                MessageBox.Show(this, "Inserire il cognome.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                lastNameTextBox.Focus();
                return;
            }

            try
            {
                var id = _editingRow?.Id ?? Guid.NewGuid();
                switch (_kind)
                {
                    case RegistryKind.Judge:
                        var judge = new Judge(id, nameTextBox.Text, absentCheckBox.Checked);
                        if (_editingRow is null)
                        {
                            _store.AddJudge(judge);
                        }
                        else
                        {
                            _store.UpdateJudge(judge);
                        }
                        break;
                    case RegistryKind.Criterion:
                        var criterion = new EvaluationCriterion(id, nameTextBox.Text);
                        if (_editingRow is null)
                        {
                            _store.AddCriterion(criterion);
                        }
                        else
                        {
                            _store.UpdateCriterion(criterion);
                        }
                        break;
                    case RegistryKind.School:
                        var school = new School(id, nameTextBox.Text);
                        if (_editingRow is null)
                        {
                            _store.AddSchool(school);
                        }
                        else
                        {
                            _store.UpdateSchool(school);
                        }
                        break;
                    case RegistryKind.Athlete:
                        var athlete = new Athlete(id, nameTextBox.Text, lastNameTextBox.Text);
                        if (_editingRow is null)
                        {
                            _store.AddAthlete(athlete);
                        }
                        else
                        {
                            _store.UpdateAthlete(athlete);
                        }
                        break;
                    case RegistryKind.Discipline:
                        var discipline = new Discipline(id, nameTextBox.Text);
                        if (_editingRow is null)
                        {
                            _store.AddDiscipline(discipline);
                        }
                        else
                        {
                            _store.UpdateDiscipline(discipline);
                        }
                        break;
                    case RegistryKind.Category:
                        var category = new Category(id, nameTextBox.Text);
                        if (_editingRow is null)
                        {
                            _store.AddCategory(category);
                        }
                        else
                        {
                            _store.UpdateCategory(category);
                        }
                        break;
                    case RegistryKind.Level:
                        var level = new Level(id, nameTextBox.Text, proCheckBox.Checked);
                        if (_editingRow is null)
                        {
                            _store.AddLevel(level);
                        }
                        else
                        {
                            _store.UpdateLevel(level);
                        }
                        break;
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Salvataggio non completato.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Il salvataggio è già riuscito: un errore di rilettura non deve invitare a inserire nuovamente la stessa voce.
            ResetEditor();
            statusLabel.Text = "Voce salvata.";
            RefreshRows();
            nameTextBox.Focus();
        }

        private void EditButtonClick(object? sender, EventArgs e)
        {
            if (_editingRow is not null || entriesGrid.CurrentRow?.DataBoundItem is not RegistryRow row)
            {
                return;
            }

            if (HasUnsavedChanges && MessageBox.Show(this, "Abbandonare i dati non salvati per modificare la voce selezionata?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _editingRow = row;
            nameTextBox.Text = row.Name;
            lastNameTextBox.Text = _kind == RegistryKind.Athlete ? row.Detail : string.Empty;
            proCheckBox.Checked = row.IsPro;
            absentCheckBox.Checked = row.IsAbsent;
            addButton.Text = "Salva modifiche";
            cancelEditButton.Visible = true;
            entriesGrid.Enabled = refreshButton.Enabled = false;
            UpdateEditButton();
            statusLabel.Text = "Modifica in corso.";
            nameTextBox.Focus();
            nameTextBox.SelectAll();
        }

        private void EntriesGridCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && _editingRow is null)
            {
                entriesGrid.CurrentCell = entriesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                EditButtonClick(sender, EventArgs.Empty);
            }
        }

        private void EntriesGridSelectionChanged(object? sender, EventArgs e) => UpdateEditButton();

        private void UpdateEditButton()
        {
            editButton.Enabled = _editingRow is null && entriesGrid.CurrentRow?.DataBoundItem is RegistryRow;
            deleteButton.Enabled = editButton.Enabled && !HasUnsavedChanges;
        }

        private void DeleteButtonClick(object? sender, EventArgs e)
        {
            if (_kind != RegistryKind.Criterion || _editingRow is not null || HasUnsavedChanges || entriesGrid.CurrentRow?.DataBoundItem is not RegistryRow row)
            {
                return;
            }

            if (MessageBox.Show(this, $"Eliminare il criterio «{row.Name}» da tutte le gare?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _store.DeleteCriterion(row.Id);
                RefreshRows();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Eliminazione non completata.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelEditButtonClick(object? sender, EventArgs e)
        {
            ResetEditor();
            statusLabel.Text = "Modifica annullata.";
            nameTextBox.Focus();
        }

        private void ResetEditor()
        {
            _editingRow = null;
            nameTextBox.Clear();
            lastNameTextBox.Clear();
            proCheckBox.Checked = false;
            absentCheckBox.Checked = false;
            addButton.Text = "Aggiungi";
            cancelEditButton.Visible = false;
            entriesGrid.Enabled = refreshButton.Enabled = true;
            UpdateEditButton();
        }

        private void RegistryFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (HasUnsavedChanges)
            {
                e.Cancel = MessageBox.Show(this, "Chiudere senza salvare i dati inseriti?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes;
            }
        }
    }
}

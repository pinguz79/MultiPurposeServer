using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Configuration;

namespace Skating.Desktop.Scoring
{
    internal sealed class ScoreEntryForm : Form
    {
        private const string ScoreHelp = "Voti 0–10 con un decimale · Salvataggio automatico · Esc annulla · Tasto destro su un voto: presenze e sblocco giudice · Totale scheda, non classifica.";
        private readonly IPerformanceStore _store;
        private readonly IRegistryStore _registry;
        private readonly IChoreographyStore _choreographies;
        private readonly IRaceStore? _races;
        private readonly ScorePreferencesStore _preferencesStore;
        private readonly Action<string> _showValidationAlert;
        private bool _showingValidationAlert;
        private ScorePreferences _preferences;
        private readonly ScoreGrid _grid = new()
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, EditMode = DataGridViewEditMode.EditOnEnter,
            RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        };
        private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 44 };
        private readonly Label _pageLabel = new() { AutoSize = true, Padding = new Padding(8, 10, 8, 0) };
        private readonly Label _summary = new() { AutoSize = true, Padding = new Padding(8, 12, 16, 0) };
        private readonly ContextMenuStrip _judgeMenu = new();
        private readonly HashSet<Guid> _unlockedJudges = [];
        private readonly HashSet<(Guid Performance, Guid Judge)> _unlockedRows = [];
        private ScorePage _page = new([], []);
        private int _block;
        private bool _loading;

        public ScoreEntryForm(IPerformanceStore store, IRegistryStore registry,
            IChoreographyStore choreographies, int block, ScorePreferencesStore preferencesStore,
            Action<string>? showValidationAlert = null, IRaceStore? races = null)
        {
            _store = store;
            _registry = registry;
            _choreographies = choreographies;
            _races = races;
            _preferencesStore = preferencesStore;
            _showValidationAlert = showValidationAlert ?? ShowValidationAlert;
            _preferences = preferencesStore.Load();
            _block = block;
            Text = "Scheda voti";
            ClientSize = new Size(1400, 720);
            MinimumSize = new Size(1000, 560);
            BackColor = Color.FromArgb(244, 246, 249);
            Font = new Font("Segoe UI", 10);
            Padding = new Padding(20);
            StartPosition = FormStartPosition.CenterParent;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            var heading = new Label { Text = "Inserimento voti", Dock = DockStyle.Top, Height = 52, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(30, 43, 57) };
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 12) };
            AddAction(toolbar, "← Blocco", () => ChangeBlock(-1));
            toolbar.Controls.Add(_pageLabel);
            AddAction(toolbar, "Blocco →", () => ChangeBlock(1));
            var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 12, 0, 12) };
            footer.Controls.Add(_summary);
            AddAction(footer, "Conferma esibizioni complete della pagina", () => Confirm(null));
            var confirmButton = footer.Controls.OfType<Button>().Single();
            confirmButton.BackColor = Color.FromArgb(42, 96, 166);
            confirmButton.ForeColor = Color.White;
            _judgeMenu.Items.Add("Abilita giudice sulla riga", null, (_, _) => RunAction(() => Unlock(false)));
            _judgeMenu.Items.Add("Abilita colonna giudice", null, (_, _) => RunAction(() => Unlock(true)));
            _judgeMenu.Items.Add("Assente su questa riga", null, (_, _) => RunAction(MarkAbsent));
            _grid.CellMouseDown += ShowJudgeMenu;
            _grid.CellClick += RowActionClick;
            var enterMode = new CheckBox { AutoSize = true, Text = "Invio: prossima esibizione", Checked = _preferences.EnterNextPerformance };
            _grid.EnterNextPerformance = enterMode.Checked;
            enterMode.CheckedChanged += (_, _) =>
            {
                _grid.EnterNextPerformance = enterMode.Checked;
                SavePreferences(_preferences with { EnterNextPerformance = enterMode.Checked });
            };
            toolbar.Controls.Add(enterMode);
            var colorMode = new CheckBox { AutoSize = true, Text = "Colore disciplina sulla prima colonna", Checked = _preferences.ColorDiscipline };
            colorMode.CheckedChanged += (_, _) =>
            {
                SavePreferences(_preferences with { ColorDiscipline = colorMode.Checked });
                ApplyColors();
            };
            toolbar.Controls.Add(colorMode);
            _grid.CellValidating += ValidateScore;
            _grid.ValidatePendingEdit = ValidatePendingEdit;
            _grid.CellEndEdit += (_, e) =>
            {
                _grid.Rows[e.RowIndex].ErrorText = string.Empty;
                _status.Text = ScoreHelp;
            };
            _grid.EditingControlShowing += (_, e) =>
            {
                if (e.Control is TextBox textBox)
                {
                    textBox.SelectAll();
                }
            };
            FormClosing += (_, e) => e.Cancel = !_grid.TryFinishEdit();
            Controls.Add(_grid);
            Controls.Add(_status);
            Controls.Add(footer);
            Controls.Add(toolbar);
            Controls.Add(heading);
            LoadPage();
        }

        private Performance Selected() => _grid.CurrentRow?.Tag as Performance ?? throw new InvalidOperationException("Selezionare un'esibizione.");

        private ScoreColumn SelectedJudge() => _grid.CurrentCell?.OwningColumn?.Tag as ScoreColumn ?? throw new InvalidOperationException("Selezionare una cella voto del giudice.");

        private void SavePreferences(ScorePreferences preferences)
        {
            _preferences = preferences;
            try
            {
                _preferencesStore.Save(preferences);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                _status.Text = $"Preferenza applicata solo per questa sessione: {exception.Message}";
            }
        }

        private void ApplyColors()
        {
            _grid.ColorDiscipline = _preferences.ColorDiscipline;
            _grid.Invalidate();
        }

        private bool Ask(string text) => MessageBox.Show(this, text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private void AddAction(FlowLayoutPanel toolbar, string text, Action action)
        {
            var button = new Button { AutoSize = true, Text = text, FlatStyle = FlatStyle.Flat, BackColor = Color.White, Padding = new Padding(8, 5, 8, 5) };
            button.FlatAppearance.BorderColor = Color.FromArgb(198, 208, 220);
            button.Click += (_, _) => RunAction(action);
            toolbar.Controls.Add(button);
        }

        private void RunAction(Action action)
        {
            if (!_grid.TryFinishEdit())
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or DbException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowJudgeMenu(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Tag is not ScoreColumn || !_grid.TryFinishEdit())
            {
                return;
            }

            _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            _judgeMenu.Show(Cursor.Position);
        }

        private void RowActionClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Action" || _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag is not true
                || _grid.Rows[e.RowIndex].Tag is not Performance performance)
            {
                return;
            }

            RunAction(() =>
            {
                if (performance.IsConfirmed)
                {
                    if (Ask("Riaprire l'esibizione per correggere i voti?"))
                    {
                        _store.Reopen(performance.ChoreographyId);
                        LoadPage();
                    }
                }
                else if (ScoreCompletion.IsComplete(performance.ChoreographyId, _page, [.. _registry.GetCriteria().Select(entry => entry.Id)]))
                {
                    Confirm(performance.ChoreographyId);
                }
            });
        }

        private void ChangeBlock(int direction)
        {
            var blocks = _store.GetSchedule().ConfirmedBlocks.Order().ToList();
            var index = blocks.IndexOf(_block) + direction;
            if (index >= 0 && index < blocks.Count)
            {
                _block = blocks[index];
                _unlockedJudges.Clear();
                _unlockedRows.Clear();
                LoadPage();
            }
        }

        private void Confirm(Guid? choreographyId)
        {
            if (Ask("Confermare i voti completi? Sarà salvata una copia preventiva dell'archivio. Le righe incomplete resteranno aperte."))
            {
                var count = _store.ConfirmComplete(_block, choreographyId);
                LoadPage();
                _status.Text = $"{count} esibizioni confermate. La conferma non chiude la gara.";
            }
        }

        private void Unlock(bool wholeColumn)
        {
            var judge = SelectedJudge().JudgeId;
            if (wholeColumn)
            {
                _unlockedJudges.Add(judge);
            }
            else
            {
                _unlockedRows.Add((Selected().ChoreographyId, judge));
            }

            RefreshReadOnly();
        }

        private void MarkAbsent()
        {
            var id = Selected().ChoreographyId;
            var judge = SelectedJudge().JudgeId;
            _store.SetJudgeRequired(id, judge, false);
            _unlockedRows.Remove((id, judge));
            _unlockedJudges.Remove(judge);
            LoadPage();
        }

        private void LoadPage()
        {
            _loading = true;
            try
            {
                _page = _store.OpenPage(_block);
                var choreographies = _choreographies.GetChoreographies().ToDictionary(entry => entry.Id);
                var disciplines = _registry.GetDisciplines().ToDictionary(entry => entry.Id, entry => entry.Name);
                var categories = _registry.GetCategories().ToDictionary(entry => entry.Id, entry => entry.Name);
                var levels = _registry.GetLevels().ToDictionary(entry => entry.Id, entry => entry.Name);
                var criteria = _registry.GetCriteria();
                var schools = _registry.GetSchools().ToDictionary(entry => entry.Id, entry => entry.Name);
                var judges = _registry.GetJudges().Where(judge => _page.Judges.Any(entry => entry.JudgeId == judge.Id));
                _grid.Rows.Clear();
                _grid.Columns.Clear();
                _grid.JudgeNames.Clear();
                _grid.RowVisuals.Clear();
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Performance", HeaderText = "N.      Esibizione / gara", Width = 300, ReadOnly = true, Frozen = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None, SortMode = DataGridViewColumnSortMode.NotSortable,
                });
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Status", HeaderText = "Scheda", Width = 95, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                });
                foreach (var judge in judges)
                {
                    _grid.JudgeNames[judge.Id] = judge.Name;
                    foreach (var criterion in criteria)
                    {
                        _grid.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            HeaderText = criterion.Name, ToolTipText = criterion.Name, MinimumWidth = 52, SortMode = DataGridViewColumnSortMode.NotSortable, Tag = new ScoreColumn(judge.Id, criterion.Id),
                        });
                    }
                }

                _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Totale", Width = 70, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
                _grid.Columns[1].DisplayIndex = _grid.ColumnCount - 1;
                _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "", Width = 90, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
                _grid.ColumnHeadersHeight = 86;
                Color[] colors = [Color.FromArgb(230, 233, 252), Color.FromArgb(222, 242, 239), Color.FromArgb(252, 232, 224), Color.FromArgb(231, 240, 219)];
                foreach (var performance in _store.GetSchedule().Performances.Where(entry => entry.Block == _block))
                {
                    var choreography = choreographies[performance.ChoreographyId];
                    var label = $"{performance.Number}. {choreography.Name}\n{disciplines[choreography.DisciplineId]} / {categories[choreography.CategoryId]} / {levels[choreography.LevelId]}";
                    var row = _grid.Rows[_grid.Rows.Add(label)];
                    row.Height = 124;
                    var composition = choreography.Composition switch { CompositionKind.Solo => "Assolo", CompositionKind.Duo => "Duo", _ => "Gruppo" };
                    _grid.RowVisuals[performance.ChoreographyId] = new ScoreRowVisual(performance.Number, choreography.Name, $"{schools[choreography.SchoolId]} · {composition}",
                        disciplines[choreography.DisciplineId], categories[choreography.CategoryId], levels[choreography.LevelId], colors[Array.IndexOf(disciplines.Keys.ToArray(), choreography.DisciplineId) % colors.Length]);
                    row.Cells[0].Style.WrapMode = DataGridViewTriState.True;
                    row.Cells[0].ToolTipText = label;
                    row.Tag = performance;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.OwningColumn?.Tag is ScoreColumn column)
                        {
                            var score = _page.Scores.SingleOrDefault(score => score.ChoreographyId == performance.ChoreographyId && score.JudgeId == column.JudgeId && score.CriterionId == column.CriterionId);
                            cell.Value = ScoreValue.Format(score?.Tenths);
                            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            cell.Style.Padding = new Padding(10, 43, 10, 43);
                        }
                    }
                }

                RefreshReadOnly();
                ApplyColors();
                Text = $"Scheda voti — Blocco {_block}";
                var blocks = _store.GetSchedule().ConfirmedBlocks.Order().ToList();
                _pageLabel.Text = $"Blocco {blocks.IndexOf(_block) + 1} di {blocks.Count}";
                _status.Text = ScoreHelp;
            }
            finally
            {
                _loading = false;
            }
        }

        private void RefreshReadOnly()
        {
            var criteria = _registry.GetCriteria().Select(entry => entry.Id).ToArray();
            var keys = _choreographies.GetChoreographies().Where(entry => _grid.RowVisuals.ContainsKey(entry.Id))
                .Select(entry => new RaceKey(entry.DisciplineId, entry.CategoryId, entry.LevelId)).Distinct();
            var results = _races is null ? new Dictionary<Guid, RankingEntry>() : keys.SelectMany(key => _races.GetRanking(key).Entries).ToDictionary(entry => entry.ChoreographyId);
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is not Performance performance)
                {
                    continue;
                }

                var complete = ScoreCompletion.IsComplete(performance.ChoreographyId, _page, criteria);
                row.Cells[1].Value = performance.Status == PerformanceStatus.NotPresented ? "Non presentata" : performance.Status == PerformanceStatus.Disqualified ? "Squalificata"
                    : performance.IsConfirmed ? "✓ Confermata" : complete ? "Da confermare" : "Incompleta";
                row.Cells["Total"].Value = complete ? (_page.Scores.Where(score => score.ChoreographyId == performance.ChoreographyId).Sum(score => score.Tenths) / 10m).ToString("0.0") : "—";
                row.Cells["Action"].Value = performance.IsConfirmed ? "Riapri" : "Conferma";
                row.Cells["Action"].Tag = !performance.IsRaceClosed && performance.Status == PerformanceStatus.Active && (performance.IsConfirmed || complete);
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn?.Tag is not ScoreColumn column)
                    {
                        continue;
                    }

                    var attendance = _page.Judges.SingleOrDefault(entry => entry.ChoreographyId == performance.ChoreographyId && entry.JudgeId == column.JudgeId);
                    var unlocked = _unlockedJudges.Contains(column.JudgeId) || _unlockedRows.Contains((performance.ChoreographyId, column.JudgeId));
                    cell.ReadOnly = performance.IsConfirmed || performance.IsRaceClosed || performance.Status != PerformanceStatus.Active || attendance is null || (!attendance.IsRequired && !unlocked);
                    cell.Style.BackColor = cell.ReadOnly ? Color.Gainsboro : Color.White;
                    var excluded = results.TryGetValue(performance.ChoreographyId, out var result) && result.ExcludedJudgeIds.Contains(column.JudgeId);
                    cell.Tag = excluded;
                    cell.ToolTipText = excluded ? "Voto conservato ma non conteggiato nella classifica della gara: giurie di numerosità diversa." : string.Empty;
                }
            }

            var rows = _grid.Rows.Cast<DataGridViewRow>().ToList();
            _summary.Text = $"{rows.Count(row => (row.Tag as Performance)?.IsConfirmed == true)} confermate · {rows.Count(row => Convert.ToString(row.Cells[1].Value) == "Da confermare")} da confermare · {rows.Count(row => Convert.ToString(row.Cells[1].Value) == "Incompleta")} incomplete";
            _grid.Invalidate();
        }

        private void ValidateScore(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            e.Cancel = !TrySaveScore(e.RowIndex, e.ColumnIndex, e.FormattedValue);
        }

        private bool ValidatePendingEdit()
        {
            var cell = _grid.CurrentCell;
            return cell is null || TrySaveScore(cell.RowIndex, cell.ColumnIndex, cell.EditedFormattedValue);
        }

        private bool TrySaveScore(int rowIndex, int columnIndex, object? formattedValue)
        {
            // Il dialogo modale può provocare una nuova validazione durante il cambio di focus.
            if (_showingValidationAlert)
            {
                return false;
            }

            if (_loading || _grid.Columns[columnIndex].Tag is not ScoreColumn column || _grid.Rows[rowIndex].Tag is not Performance performance || _grid.Rows[rowIndex].Cells[columnIndex].ReadOnly)
            {
                return true;
            }

            try
            {
                var value = ScoreValue.Parse(Convert.ToString(formattedValue));
                var unlocked = _unlockedJudges.Contains(column.JudgeId) || _unlockedRows.Contains((performance.ChoreographyId, column.JudgeId));
                var previous = _page.Scores.SingleOrDefault(score => score.ChoreographyId == performance.ChoreographyId && score.JudgeId == column.JudgeId && score.CriterionId == column.CriterionId)?.Tenths;
                if (previous != value)
                {
                    _store.SaveScore(performance.ChoreographyId, column.JudgeId, column.CriterionId, value, unlocked);
                    _page = _store.OpenPage(_block);
                    RefreshReadOnly();
                }

                _grid.Rows[rowIndex].ErrorText = string.Empty;
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException or FormatException or InvalidOperationException or DbException or IOException)
            {
                _grid.Rows[rowIndex].ErrorText = exception.Message;
                _status.Text = exception.Message;
                _showingValidationAlert = true;
                try
                {
                    _showValidationAlert(exception.Message);
                    if (_grid.EditingControl is TextBox textBox)
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    }
                }
                finally
                {
                    _showingValidationAlert = false;
                }

                return false;
            }
        }

        private void ShowValidationAlert(string message) => MessageBox.Show(this, $"Il voto non è stato salvato.\r\n\r\n{message}\r\n\r\nCorreggi il dato oppure premi Esc per annullare la modifica.", "Verifica il voto inserito", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _judgeMenu.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

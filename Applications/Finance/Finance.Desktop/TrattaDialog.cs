using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class TrattaDialog : Form
    {
        private readonly FinanceApiClient _client;
        private readonly Guid _first;
        private readonly Guid _second;
        private readonly List<SaveTariffaDefinition> _definitions;
        private bool _saving;

        public TrattaDialog(FinanceApiClient client, string title, IReadOnlyList<TariffaTratta> definitions)
        {
            _client = client;
            _first = definitions[0].CaselloAId;
            _second = definitions[0].CaselloBId;
            _definitions = [.. definitions.OrderBy(item => item.Index)
                .Select(item => new SaveTariffaDefinition(decimal.Parse(item.Formula, CultureInfo.InvariantCulture), item.ValidFrom, item.ValidTo))];
            InitializeComponent();
            Text = title;
            grid.Columns.Add("Kind", "Tipo / priorità");
            grid.Columns.Add("Value", "Tariffa");
            grid.Columns.Add("From", "Dal (incluso)");
            grid.Columns.Add("To", "Al (incluso)");
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            RefreshRows(0);
        }

        private void RefreshRows(int selected)
        {
            grid.Rows.Clear();
            for (int index = 0; index < _definitions.Count; index++)
            {
                SaveTariffaDefinition item = _definitions[index];
                grid.Rows.Add(index == _definitions.Count - 1 ? "Base" : $"Eccezione {index + 1}",
                    item.Value == 0 ? "Non conosciuta (0 €)" : item.Value.ToString("C2", CultureInfo.GetCultureInfo("it-IT")),
                    item.ValidFrom?.ToString("dd/MM/yyyy") ?? "Senza inizio", item.ValidTo?.ToString("dd/MM/yyyy") ?? "Senza scadenza");
            }

            grid.CurrentCell = grid.Rows[Math.Clamp(selected, 0, _definitions.Count - 1)].Cells[0];
            SelectionChanged(this, EventArgs.Empty);
        }

        private void SelectionChanged(object? sender, EventArgs e)
        {
            int index = grid.CurrentRow?.Index ?? -1;
            editButton.Enabled = index >= 0;
            deleteButton.Enabled = index >= 0 && index < _definitions.Count - 1;
            upButton.Enabled = index > 0 && index < _definitions.Count - 1;
            downButton.Enabled = index >= 0 && index < _definitions.Count - 2;
        }

        private void EditClick(object? sender, EventArgs e)
        {
            int index = grid.CurrentRow?.Index ?? -1;
            if (index < 0)
            {
                return;
            }

            using var dialog = new TariffaDefinitionDialog(_definitions[index], index == _definitions.Count - 1);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _definitions[index] = dialog.Definition;
                RefreshRows(index);
            }
        }

        private void GridDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditClick(sender, EventArgs.Empty);
            }
        }

        private void AddClick(object? sender, EventArgs e)
        {
            using var dialog = new TariffaDefinitionDialog(new SaveTariffaDefinition(0m, DateOnly.FromDateTime(DateTime.Today), null), false);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _definitions.Insert(0, dialog.Definition);
                RefreshRows(0);
            }
        }

        private void DeleteClick(object? sender, EventArgs e)
        {
            int index = grid.CurrentRow?.Index ?? -1;
            if (index >= 0 && index < _definitions.Count - 1 && MessageBox.Show(this, "Rimuovere questa eccezione? La modifica sarà applicata solo con Salva.",
                "Finance", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _definitions.RemoveAt(index);
                RefreshRows(index);
            }
        }

        private void UpClick(object? sender, EventArgs e) => MoveDefinition(-1);

        private void DownClick(object? sender, EventArgs e) => MoveDefinition(1);

        private void MoveDefinition(int offset)
        {
            int index = grid.CurrentRow?.Index ?? -1;
            int target = index + offset;
            if (index < 0 || index >= _definitions.Count - 1 || target < 0 || target >= _definitions.Count - 1)
            {
                return;
            }

            (_definitions[index], _definitions[target]) = (_definitions[target], _definitions[index]);
            RefreshRows(target);
        }

        private async void SaveClick(object? sender, EventArgs e)
        {
            if (_saving)
            {
                return;
            }

            _saving = true;
            Enabled = false;
            try
            {
                await _client.UpdateTratta(_first, _second, _definitions);
                _saving = false;
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _saving = false;
                Enabled = true;
            }
        }

        private void DialogClosing(object? sender, FormClosingEventArgs e) => e.Cancel = _saving;
    }
}

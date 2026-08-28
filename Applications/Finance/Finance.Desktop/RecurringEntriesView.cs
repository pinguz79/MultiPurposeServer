using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class RecurringEntriesView : UserControl
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private readonly FinanceApiClient _client;
        private IReadOnlyList<VoceRicorrente> _items = [];

        public RecurringEntriesView(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
            ConfigureGrids();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await RefreshItems();
        }

        private async void AddButtonClick(object? sender, EventArgs e)
        {
            using var dialog = new VoceRicorrenteDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await Execute(async () => await _client.CreateVoceRicorrente(dialog.Value));
            }
        }

        private void ConfigureGrids()
        {
            masterGrid.Columns.Add("Name", "Nome");
            masterGrid.Columns.Add("DisplayName", "Nome visualizzato");
            masterGrid.Columns.Add("Period", "Validità");
            masterGrid.Columns.Add("Value", "Valore attuale");
            masterGrid.Columns.Add(CreateActionColumn("Plan", "Crea pianificazione", GridActionIcons.CalendarAdd));
            masterGrid.Columns.Add(CreateActionColumn("Edit", "Modifica voce", GridActionIcons.Edit));
            masterGrid.Columns.Add(CreateActionColumn("Delete", "Elimina voce", GridActionIcons.Delete));
            detailGrid.Columns.Add("DisplayName", "Nome visualizzato");
            detailGrid.Columns.Add("Value", "Valore");
            detailGrid.Columns.Add("ValidFrom", "Dal");
            detailGrid.Columns.Add("ValidTo", "Al");
            detailGrid.Columns.Add(CreateActionColumn("Up", "Aumenta priorità", GridActionIcons.ArrowUp));
            detailGrid.Columns.Add(CreateActionColumn("Down", "Riduci priorità", GridActionIcons.ArrowDown));
            detailGrid.Columns.Add(CreateActionColumn("Edit", "Modifica intervallo", GridActionIcons.Edit));
            detailGrid.Columns.Add(CreateActionColumn("Delete", "Elimina intervallo", GridActionIcons.Delete));
            detailGrid.CellContentClick += DetailGridCellContentClick;
        }

        private async void DetailGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            VoceRicorrente? item = SelectedItem();
            if (item is null || e.RowIndex < 0)
            {
                return;
            }

            var definitions = item.Definitions.ToList();
            switch (detailGrid.Columns[e.ColumnIndex].Name)
            {
                case "Up" when e.RowIndex > 0:
                    (definitions[e.RowIndex], definitions[e.RowIndex - 1]) = (definitions[e.RowIndex - 1], definitions[e.RowIndex]);
                    break;
                case "Down" when e.RowIndex < definitions.Count - 1:
                    (definitions[e.RowIndex], definitions[e.RowIndex + 1]) = (definitions[e.RowIndex + 1], definitions[e.RowIndex]);
                    break;
                case "Edit":
                    using (var dialog = new VoceRicorrenteDefinitionDialog(definitions[e.RowIndex]))
                    {
                        if (dialog.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }

                        definitions[e.RowIndex] = dialog.Definition;
                    }
                    break;
                case "Delete" when definitions.Count > 1:
                    definitions.RemoveAt(e.RowIndex);
                    break;
                default:
                    return;
            }

            await Execute(async () => await _client.UpdateVoceRicorrente(item.Name, ToRequest(item.Name, definitions)));
        }

        private async Task Execute(Func<Task> operation)
        {
            try
            {
                UseWaitCursor = true;
                await operation();
                await RefreshItems();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async void MasterGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            VoceRicorrente item = _items[e.RowIndex];
            switch (masterGrid.Columns[e.ColumnIndex].Name)
            {
                case "Plan":
                    return;
                case "Edit":
                    using (var dialog = new VoceRicorrenteDialog(item))
                    {
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            await Execute(async () => await _client.UpdateVoceRicorrente(item.Name, dialog.Value));
                        }
                    }
                    return;
                case "Delete" when MessageBox.Show(this, $"Eliminare la voce '{item.DisplayName}'?", "Finance", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes:
                    await Execute(async () => await _client.DeleteVoceRicorrente(item.Name));
                    return;
            }
        }

        private void MasterGridSelectionChanged(object? sender, EventArgs e) => RenderDetail(SelectedItem());

        private async Task RefreshItems()
        {
            _items = await _client.GetVociRicorrenti();
            masterGrid.Rows.Clear();

            foreach (VoceRicorrente item in _items)
            {
                masterGrid.Rows.Add(item.Name, item.DisplayName, FormatPeriod(item), item.CurrentValue is null ? "-" : item.CurrentValue.Value.ToString("N2", ItalianCulture) + " €");
            }

            RenderDetail(SelectedItem());
        }

        private void RenderDetail(VoceRicorrente? item)
        {
            detailGrid.Rows.Clear();
            detailCaption.Text = item is null ? "Definizioni" : $"Definizioni — {item.Name}";

            if (item is null)
            {
                coveragePanel.SetDefinitions([]);
                return;
            }

            foreach (VoceRicorrenteDefinition definition in item.Definitions)
            {
                int rowIndex = detailGrid.Rows.Add(
                    definition.DisplayName,
                    definition.Value.ToString("N2", ItalianCulture) + " €",
                    definition.ValidFrom?.ToString("dd/MM/yy", ItalianCulture) ?? "-∞",
                    definition.ValidTo?.ToString("dd/MM/yy", ItalianCulture) ?? "+∞");
                DataGridViewRow row = detailGrid.Rows[rowIndex];
                ConfigureActionCell(row, "Up", rowIndex > 0, GridActionIcons.ArrowUp, GridActionIcons.ArrowUpDisabled);
                ConfigureActionCell(row, "Down", rowIndex < item.Definitions.Count - 1, GridActionIcons.ArrowDown, GridActionIcons.ArrowDownDisabled);
                ConfigureActionCell(row, "Delete", item.Definitions.Count > 1, GridActionIcons.Delete, GridActionIcons.DeleteDisabled);
            }

            coveragePanel.SetDefinitions(item.Definitions);
        }

        private static void ConfigureActionCell(DataGridViewRow row, string columnName, bool enabled, Image enabledIcon, Image disabledIcon)
        {
            DataGridViewCell cell = row.Cells[columnName];
            cell.Value = enabled ? enabledIcon : disabledIcon;
            cell.Style.BackColor = enabled ? Color.Empty : SystemColors.Control;
        }

        private static DataGridViewImageColumn CreateActionColumn(string name, string toolTipText, Image icon) => new()
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            HeaderText = string.Empty,
            Image = icon,
            ImageLayout = DataGridViewImageCellLayout.Normal,
            Name = name,
            ToolTipText = toolTipText,
            Width = 42,
        };

        private VoceRicorrente? SelectedItem()
            => masterGrid.CurrentRow is null || masterGrid.CurrentRow.Index >= _items.Count ? null : _items[masterGrid.CurrentRow.Index];

        private static string FormatPeriod(VoceRicorrente item) => (item.ValidFrom, item.ValidTo) switch
        {
            (null, null) => "Sempre",
            (null, DateOnly to) => $"Fino al {to:dd/MM/yy}",
            (DateOnly from, null) => $"Dal {from:dd/MM/yy}",
            (DateOnly from, DateOnly to) => $"{from:dd/MM/yy} – {to:dd/MM/yy}",
        };

        private static SaveVoceRicorrente ToRequest(string name, IReadOnlyList<VoceRicorrenteDefinition> definitions)
            => new(name, [.. definitions.Select(definition => new SaveVoceRicorrenteDefinition(
                definition.Id,
                definition.DisplayName,
                definition.Value,
                definition.ValidFrom,
                definition.ValidTo))]);
    }
}

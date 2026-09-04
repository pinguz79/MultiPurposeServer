using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class AccountParametersView : UserControl
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private readonly FinanceApiClient _client;
        private IReadOnlyList<Conto> _accounts = [];
        private IReadOnlyList<ParametroConto> _items = [];

        public AccountParametersView(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
            ConfigureGrids();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _accounts = await _client.GetConti();
            accountComboBox.Items.AddRange([.. _accounts.Cast<object>()]);
            accountComboBox.DisplayMember = nameof(Conto.DisplayName);
            accountComboBox.SelectedIndex = accountComboBox.Items.Count == 0 ? -1 : 0;
            SetCommandsEnabled();
        }

        private async void AccountComboBoxSelectedIndexChanged(object? sender, EventArgs e) => await RefreshItems();

        private async void AddButtonClick(object? sender, EventArgs e)
        {
            Conto? account = SelectedAccount();
            if (account is null)
            {
                return;
            }

            using var dialog = new ParametroContoDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await Execute(async () => await _client.CreateParametroConto(account.Name,
                    new CreateParametroConto(dialog.NameValue, dialog.TypeValue, dialog.Definitions)));
            }
        }

        private async void ConfigureCardButtonClick(object? sender, EventArgs e)
        {
            Conto? account = SelectedAccount();
            if (account is null)
            {
                return;
            }

            using var dialog = new CartaASaldoDialog(account, _accounts);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await Execute(async () => await _client.ConfigureCartaASaldo(account.Name, dialog.Value));
            }
        }

        private void ConfigureGrids()
        {
            masterGrid.Columns.Add("Name", "Nome");
            masterGrid.Columns.Add("DisplayName", "Nome visualizzato");
            masterGrid.Columns.Add("Type", "Tipo");
            masterGrid.Columns.Add("Value", "Valore attuale");
            masterGrid.Columns.Add("Period", "Validità");
            masterGrid.Columns.Add(CreateActionColumn("Edit", "Modifica parametro", GridActionIcons.Edit));
            masterGrid.Columns.Add(CreateActionColumn("Delete", "Elimina parametro", GridActionIcons.Delete));
            detailGrid.Columns.Add("DisplayName", "Nome visualizzato");
            detailGrid.Columns.Add("Value", "Valore");
            detailGrid.Columns.Add("ValidFrom", "Dal");
            detailGrid.Columns.Add("ValidTo", "Al");
            detailGrid.Columns.Add(CreateActionColumn("Up", "Aumenta priorità", GridActionIcons.ArrowUp));
            detailGrid.Columns.Add(CreateActionColumn("Down", "Riduci priorità", GridActionIcons.ArrowDown));
            detailGrid.Columns.Add(CreateActionColumn("Edit", "Modifica intervallo", GridActionIcons.Edit));
            detailGrid.Columns.Add(CreateActionColumn("Delete", "Elimina intervallo", GridActionIcons.Delete));
        }

        private async void DetailGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            ParametroConto? item = SelectedItem();
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
                    using (var dialog = new ParametroContoDefinitionDialog(definitions[e.RowIndex], item.Type))
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

            await Update(item, definitions);
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
            Conto? account = SelectedAccount();
            if (account is null || e.RowIndex < 0)
            {
                return;
            }

            ParametroConto item = _items[e.RowIndex];
            if (masterGrid.Columns[e.ColumnIndex].Name == "Edit")
            {
                using var dialog = new ParametroContoDialog(item);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await Execute(async () => await _client.UpdateParametroConto(account.Name, item.Name,
                        new UpdateParametroConto(dialog.Definitions)));
                }
            }
            else if (masterGrid.Columns[e.ColumnIndex].Name == "Delete"
                && MessageBox.Show(this, $"Eliminare il parametro '{item.DisplayName}'?", "Finance", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                await Execute(async () => await _client.DeleteParametroConto(account.Name, item.Name));
            }
        }

        private void MasterGridSelectionChanged(object? sender, EventArgs e) => RenderDetail(SelectedItem());

        private async Task RefreshItems()
        {
            Conto? account = SelectedAccount();
            _items = account is null ? [] : await _client.GetParametriConto(account.Name);
            masterGrid.Rows.Clear();

            foreach (ParametroConto item in _items)
            {
                masterGrid.Rows.Add(item.Name, item.DisplayName, item.Type, FormatValue(item.CurrentValue, item.Type), FormatPeriod(item));
            }

            RenderDetail(SelectedItem());
            SetCommandsEnabled();
        }

        private void RenderDetail(ParametroConto? item)
        {
            detailGrid.Rows.Clear();
            detailCaption.Text = item is null ? "Definizioni" : $"Definizioni — {item.Name}";
            if (item is null)
            {
                coveragePanel.SetDefinitions([]);
                return;
            }

            foreach (ParametroContoDefinition definition in item.Definitions)
            {
                int rowIndex = detailGrid.Rows.Add(definition.DisplayName, FormatValue(definition.Value, item.Type),
                    definition.ValidFrom?.ToString("dd/MM/yy", ItalianCulture) ?? "-∞",
                    definition.ValidTo?.ToString("dd/MM/yy", ItalianCulture) ?? "+∞");
                DataGridViewRow row = detailGrid.Rows[rowIndex];
                ConfigureActionCell(row, "Up", rowIndex > 0, GridActionIcons.ArrowUp, GridActionIcons.ArrowUpDisabled);
                ConfigureActionCell(row, "Down", rowIndex < item.Definitions.Count - 1, GridActionIcons.ArrowDown, GridActionIcons.ArrowDownDisabled);
                ConfigureActionCell(row, "Delete", item.Definitions.Count > 1, GridActionIcons.Delete, GridActionIcons.DeleteDisabled);
            }

            coveragePanel.SetDefinitions(item.Definitions);
        }

        private async Task Update(ParametroConto item, IReadOnlyList<ParametroContoDefinition> definitions)
        {
            Conto account = SelectedAccount()!;
            var request = new UpdateParametroConto([.. definitions.Select(definition
                => new SaveParametroContoDefinition(definition.Id, definition.DisplayName, definition.Value, definition.ValidFrom, definition.ValidTo))]);
            await Execute(async () => await _client.UpdateParametroConto(account.Name, item.Name, request));
        }

        private void SetCommandsEnabled()
        {
            bool hasAccount = SelectedAccount() is not null;
            addButton.Enabled = hasAccount;
            configureCardButton.Enabled = hasAccount && _accounts.Count > 1;
        }

        private Conto? SelectedAccount() => accountComboBox.SelectedItem as Conto;

        private ParametroConto? SelectedItem()
            => masterGrid.CurrentRow is null || masterGrid.CurrentRow.Index >= _items.Count ? null : _items[masterGrid.CurrentRow.Index];

        private static string FormatValue(decimal? value, TipoParametroConto type) => value is null ? "-" : type switch
        {
            TipoParametroConto.Importo => value.Value.ToString("N2", ItalianCulture) + " €",
            TipoParametroConto.Percentuale => (value.Value * 100m).ToString("N2", ItalianCulture) + " %",
            TipoParametroConto.Intero => value.Value.ToString("N0", ItalianCulture),
            _ => value.Value.ToString("N6", ItalianCulture),
        };

        private static string FormatPeriod(ParametroConto item) => (item.ValidFrom, item.ValidTo) switch
        {
            (null, null) => "Sempre",
            (null, DateOnly to) => $"Fino al {to:dd/MM/yy}",
            (DateOnly from, null) => $"Dal {from:dd/MM/yy}",
            (DateOnly from, DateOnly to) => $"{from:dd/MM/yy} – {to:dd/MM/yy}",
        };

        private static void ConfigureActionCell(DataGridViewRow row, string name, bool enabled, Image icon, Image disabledIcon)
        {
            row.Cells[name].Value = enabled ? icon : disabledIcon;
            row.Cells[name].Style.BackColor = enabled ? Color.Empty : SystemColors.Control;
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
    }
}

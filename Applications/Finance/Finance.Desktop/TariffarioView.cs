using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class TariffarioView : UserControl
    {
        private readonly FinanceApiClient _client;
        private IReadOnlyList<Casello> _stations = [];
        private IReadOnlyList<TariffaTratta> _tariffs = [];
        private bool _busy;

        public TariffarioView(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadData();
        }

        public Task LoadData() => Execute(RefreshData);

        private async Task RefreshData()
        {
            IReadOnlyList<Casello> stations = await _client.GetCaselli();
            IReadOnlyList<TariffaTratta> tariffs = await _client.GetTariffe();
            if (IsDisposed)
            {
                return;
            }

            _stations = stations;
            _tariffs = tariffs;
            RenderRows();
        }

        private void FilterChanged(object? sender, EventArgs e) => RenderRows();

        private void RenderRows()
        {
            grid.Columns.Clear();
            grid.Columns.Add("First", stationsCheckBox.Checked ? "Stazione" : "Stazione A");
            if (!stationsCheckBox.Checked)
            {
                grid.Columns.Add("Second", "Stazione B");
                grid.Columns.Add("Value", "Tariffa di oggi");
                grid.Columns.Add("Expiry", "Scadenza tariffa");
            }

            grid.Columns.Add(ActionColumn("Edit", "Modifica", GridActionIcons.Edit));
            grid.Columns.Add(ActionColumn("Delete", "Elimina", GridActionIcons.Delete));
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            string filter = filterInput.Text.Trim();
            if (stationsCheckBox.Checked)
            {
                foreach (Casello station in _stations.OrderBy(item => item.Name).Where(item => Matches(item.Name, filter)))
                {
                    int row = grid.Rows.Add(station.Name);
                    grid.Rows[row].Tag = station;
                }
            }
            else
            {
                var names = _stations.ToDictionary(item => item.Id, item => item.Name);
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                foreach (var group in _tariffs.GroupBy(item => (item.CaselloAId, item.CaselloBId))
                    .OrderBy(item => names[item.Key.CaselloAId]).ThenBy(item => names[item.Key.CaselloBId]))
                {
                    string first = names[group.Key.CaselloAId];
                    string second = names[group.Key.CaselloBId];
                    if (!Matches(first, filter) && !Matches(second, filter))
                    {
                        continue;
                    }

                    TariffaTratta? current = group.OrderBy(item => item.Index)
                        .FirstOrDefault(item => (item.ValidFrom is null || item.ValidFrom <= today) && (item.ValidTo is null || item.ValidTo >= today));
                    decimal value = current is null ? 0m : decimal.Parse(current.Formula, CultureInfo.InvariantCulture);
                    int row = grid.Rows.Add(first, second, value == 0m ? "Non conosciuta" : value.ToString("C2", CultureInfo.GetCultureInfo("it-IT")),
                        current?.ValidTo?.ToString("dd/MM/yyyy") ?? "Senza scadenza");
                    grid.Rows[row].Tag = group.ToArray();
                    if (value == 0m)
                    {
                        grid.Rows[row].DefaultCellStyle.ForeColor = Color.DarkOrange;
                    }
                }
            }

            statusLabel.Text = $"{grid.Rows.Count} {(stationsCheckBox.Checked ? "stazioni" : "tratte bidirezionali")} — 0 = tariffa sconosciuta";
        }

        private static bool Matches(string name, string filter) => CultureInfo.GetCultureInfo("it-IT").CompareInfo.IndexOf(name, filter, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

        private async void GridClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _busy)
            {
                return;
            }

            object? item = grid.Rows[e.RowIndex].Tag;
            if (grid.Columns[e.ColumnIndex].Name == "Edit")
            {
                await Edit(item);
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Delete")
            {
                await Delete(item);
            }
        }

        private async void GridDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex] is not DataGridViewImageColumn && !_busy)
            {
                await Edit(grid.Rows[e.RowIndex].Tag);
            }
        }

        private async Task Edit(object? item)
        {
            if (item is Casello station)
            {
                using var dialog = new CaselloDialog(station.Name);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await Execute(async () =>
                    {
                        await _client.UpdateCasello(station.Id, dialog.StationName);
                        await RefreshData();
                    });
                }
            }
            else if (item is TariffaTratta[] definitions)
            {
                string first = _stations.Single(station => station.Id == definitions[0].CaselloAId).Name;
                string second = _stations.Single(station => station.Id == definitions[0].CaselloBId).Name;
                using var dialog = new TrattaDialog(_client, $"{first} ↔ {second}", definitions);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await Execute(RefreshData);
                }
            }
        }

        private async Task Delete(object? item)
        {
            string message = item is Casello station ? $"Eliminare la stazione {station.Name} e tutte le sue tratte?"
                : "Eliminare la tratta selezionata e tutte le sue tariffe?";
            if (MessageBox.Show(this, message + "\r\nL'operazione verrà rifiutata se esistono riferimenti in uso.", "Finance", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            await Execute(async () =>
            {
                if (item is Casello selected)
                {
                    await _client.DeleteCasello(selected.Id);
                }
                else if (item is TariffaTratta[] definitions)
                {
                    await _client.DeleteTratta(definitions[0].CaselloAId, definitions[0].CaselloBId);
                }

                await RefreshData();
            });
        }

        private async void AddStationClick(object? sender, EventArgs e)
        {
            using var dialog = new CaselloDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await Execute(async () =>
                {
                    await _client.CreateCasello(dialog.StationName);
                    await RefreshData();
                });
            }
        }

        private async void RefreshClick(object? sender, EventArgs e) => await Execute(RefreshData);

        private async Task Execute(Func<Task> operation)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            Enabled = false;
            UseWaitCursor = true;
            try
            {
                await operation();
            }
            catch (Exception exception)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _busy = false;
                if (!IsDisposed)
                {
                    Enabled = true;
                    UseWaitCursor = false;
                }
            }
        }

        private static DataGridViewImageColumn ActionColumn(string name, string tooltip, Image image) => new()
        {
            Name = name, HeaderText = string.Empty, ToolTipText = tooltip, Image = image,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 42,
        };
    }
}

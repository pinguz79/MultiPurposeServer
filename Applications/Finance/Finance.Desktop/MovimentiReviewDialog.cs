using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class MovimentiReviewDialog : Form
    {
        private readonly FinanceApiClient _client;
        private readonly IReadOnlyList<Conto> _accounts;
        private readonly Conto? _conto;
        private bool _busy;
        private readonly ContextMenuStrip _movementMenu;
        private readonly ToolStripItem _confirmMenuItem;
        private MovimentoEdit? _contextMovement;

        public MovimentiReviewDialog(FinanceApiClient client, IReadOnlyList<Conto> accounts, Conto? conto = null)
        {
            _client = client;
            _accounts = accounts;
            _conto = conto;
            InitializeComponent();
            Text = conto is null ? "Movimenti da confermare" : $"Gestione movimenti — {conto.DisplayName}";
            monthInput.Value = DateTime.Today;
            monthInput.Visible = conto is not null;
            newButton.Visible = conto is not null;
            grid.AutoGenerateColumns = false;
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Selected", HeaderText = "", Width = 35 });
            AddColumn("Conto", nameof(MovimentoEdit.ContoName), 100);
            AddColumn("Data", nameof(MovimentoEdit.Date), 90);
            AddColumn("Descrizione", nameof(MovimentoEdit.Description), 230);
            AddColumn("Importo", nameof(MovimentoEdit.Amount), 110);
            grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Confermato", DataPropertyName = nameof(MovimentoEdit.IsConfirmed), ReadOnly = true, Width = 85 });
            AddColumn("Avvisi", nameof(MovimentoEdit.ReviewWarning), 200);
            grid.Columns[2].DefaultCellStyle.Format = "dd/MM/yy";
            grid.Columns[4].DefaultCellStyle.Format = "C2";
            _movementMenu = new ContextMenuStrip(components ??= new System.ComponentModel.Container());
            _movementMenu.Items.Add("Modifica…", null, async (_, _) => await RunMovementAction(_contextMovement, "edit"));
            _movementMenu.Items.Add("Elimina…", null, async (_, _) => await RunMovementAction(_contextMovement, "delete"));
            _confirmMenuItem = _movementMenu.Items.Add("Conferma", null, async (_, _) => await RunMovementAction(_contextMovement, "confirm"));
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await Run(() => Task.CompletedTask);
        }

        private void AddColumn(string title, string property, int width)
            => grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = property, ReadOnly = true, Width = width });

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = _busy;
            base.OnFormClosing(e);
        }

        internal async Task RefreshItems()
        {
            grid.EndEdit();
            HashSet<Guid> selected = [.. grid.Rows.Cast<DataGridViewRow>().Where(row => row.Cells[0].Value is true).Select(row => row.DataBoundItem).OfType<MovimentoEdit>().Select(item => item.Id)];
            Guid? current = (grid.CurrentRow?.DataBoundItem as MovimentoEdit)?.Id;
            DateOnly from = new(monthInput.Value.Year, monthInput.Value.Month, 1);
            IReadOnlyList<MovimentoEdit> items = await _client.GetMovimentiForReview(_conto?.Name, from, from.AddMonths(1).AddDays(-1));
            grid.DataSource = items.ToList();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.DataBoundItem is MovimentoEdit movement)
                {
                    row.Cells[0].ReadOnly = !movement.CanConfirm;
                    row.Cells[0].Value = movement.CanConfirm && selected.Contains(movement.Id);
                    if (movement.Id == current)
                    {
                        grid.CurrentCell = row.Cells[1];
                    }
                }
            }
            statusLabel.Text = $"{items.Count} movimenti. Chiudi per rimandare; Modifica per correggere o spostare una singola occorrenza.";
        }

        private async Task Run(Func<Task> action)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            actionsPanel.Enabled = false;
            grid.Enabled = false;
            refreshButton.Enabled = false;
            monthInput.Enabled = false;
            try
            {
                await action();
                await RefreshItems();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _busy = false;
                actionsPanel.Enabled = true;
                grid.Enabled = true;
                refreshButton.Enabled = true;
                monthInput.Enabled = true;
            }
        }

        private async void RefreshButtonClick(object? sender, EventArgs e) => await Run(() => Task.CompletedTask);

        private async void NewButtonClick(object? sender, EventArgs e) => await Run(() => Edit(null));

        private async void EditButtonClick(object? sender, EventArgs e)
        {
            await RunMovementAction(grid.CurrentRow?.DataBoundItem as MovimentoEdit, "edit");
        }

        private async void GridCellMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                await RunMovementAction(grid.Rows[e.RowIndex].DataBoundItem as MovimentoEdit, "edit");
            }
        }

        private async void GridCellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (_busy || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            var movement = grid.Rows[e.RowIndex].DataBoundItem as MovimentoEdit;
            if (e.Button == MouseButtons.Middle)
            {
                await RunMovementAction(movement, "confirm");
            }
            else if (e.Button == MouseButtons.Right && movement is not null)
            {
                grid.EndEdit();
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[1];
                _contextMovement = movement;
                _confirmMenuItem.Visible = !movement.IsConfirmed;
                _confirmMenuItem.Enabled = movement.CanConfirm;
                _confirmMenuItem.ToolTipText = movement.ReviewWarning;
                _movementMenu.Show(grid, grid.PointToClient(Cursor.Position));
            }
        }

        internal async Task RunMovementAction(MovimentoEdit? movement, string action)
        {
            if (_busy || movement is null)
            {
                return;
            }
            if (action == "edit")
            {
                await Run(() => Edit(movement));
            }
            else if (action == "confirm" && !movement.IsConfirmed && movement.CanConfirm)
            {
                await Run(() => _client.ConfirmMovimenti([movement.Id]));
            }
            else if (action == "delete"
                && MessageBox.Show(this, $"Eliminare '{movement.Description}' del {movement.Date:dd/MM/yy}? La pianificazione non verrà eliminata.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                await Run(() => _client.DeleteMovimento(movement.Id));
            }
        }

        private Task Edit(MovimentoEdit? movement)
        {
            Conto conto = _conto ?? _accounts.Single(item => item.Name == movement!.ContoName);
            if (movement?.Pedaggio is not null)
            {
                using var pedaggioDialog = new PedaggioDialog(_client, conto, movement);
                pedaggioDialog.ShowDialog(this);
                return Task.CompletedTask;
            }
            using var dialog = new MovimentoDialog(_client, conto, movement);
            dialog.ShowDialog(this);
            return Task.CompletedTask;
        }

        private async void DeleteButtonClick(object? sender, EventArgs e)
        {
            await RunMovementAction(grid.CurrentRow?.DataBoundItem as MovimentoEdit, "delete");
        }

        private async void ConfirmButtonClick(object? sender, EventArgs e)
        {
            grid.EndEdit();
            Guid[] ids = [.. grid.Rows.Cast<DataGridViewRow>().Where(row => row.Cells[0].Value is true).Select(row => row.DataBoundItem).OfType<MovimentoEdit>().Where(movement => movement.CanConfirm).Select(movement => movement.Id)];
            if (ids.Length > 0 && MessageBox.Show(this, $"Confermare {ids.Length} movimenti? Gli importi diventeranno definitivi.", "Conferma movimenti", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await Run(() => _client.ConfirmMovimenti(ids));
            }
        }
    }
}

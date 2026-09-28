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
            AddColumn("Errore", nameof(MovimentoEdit.EvaluationError), 200);
            grid.Columns[2].DefaultCellStyle.Format = "dd/MM/yy";
            grid.Columns[4].DefaultCellStyle.Format = "C2";
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

        private async Task RefreshItems()
        {
            DateOnly from = new(monthInput.Value.Year, monthInput.Value.Month, 1);
            IReadOnlyList<MovimentoEdit> items = await _client.GetMovimentiForReview(_conto?.Name, from, from.AddMonths(1).AddDays(-1));
            grid.DataSource = items.ToList();
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
            }
        }

        private async void RefreshButtonClick(object? sender, EventArgs e) => await Run(() => Task.CompletedTask);

        private async void NewButtonClick(object? sender, EventArgs e) => await Run(() => Edit(null));

        private async void EditButtonClick(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is MovimentoEdit movement)
            {
                await Run(() => Edit(movement));
            }
        }

        private Task Edit(MovimentoEdit? movement)
        {
            Conto conto = _conto ?? _accounts.Single(item => item.Name == movement!.ContoName);
            using var dialog = new MovimentoDialog(_client, conto, movement);
            dialog.ShowDialog(this);
            return Task.CompletedTask;
        }

        private async void DeleteButtonClick(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is MovimentoEdit movement
                && MessageBox.Show(this, $"Eliminare '{movement.Description}' del {movement.Date:dd/MM/yy}? La pianificazione non verrà eliminata.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                await Run(() => _client.DeleteMovimento(movement.Id));
            }
        }

        private async void ConfirmButtonClick(object? sender, EventArgs e)
        {
            grid.EndEdit();
            Guid[] ids = [.. grid.Rows.Cast<DataGridViewRow>().Where(row => row.Cells[0].Value is true).Select(row => row.DataBoundItem).OfType<MovimentoEdit>().Select(movement => movement.Id)];
            if (ids.Length > 0 && MessageBox.Show(this, $"Confermare {ids.Length} movimenti? Gli importi diventeranno definitivi.", "Conferma movimenti", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await Run(() => _client.ConfirmMovimenti(ids));
            }
        }
    }
}

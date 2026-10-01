using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class FinanziamentoView : UserControl
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private readonly FinanceApiClient _client;
        private readonly string _name;
        private PianoFinanziamento? _plan;
        private bool _busy;

        public FinanziamentoView(FinanceApiClient client, string name)
        {
            _client = client;
            _name = name;
            InitializeComponent();
            AddColumn("Rata", nameof(RataFinanziamento.Number), "0", 45);
            AddColumn("Scadenza", nameof(RataFinanziamento.DueDate), "dd/MM/yy", 80);
            AddColumn("Capitale", nameof(RataFinanziamento.Principal), "C2", 90);
            AddColumn("Interessi", nameof(RataFinanziamento.Interest), "C2", 85);
            AddColumn("Assicurazione", nameof(RataFinanziamento.Insurance), "C2", 95);
            AddColumn("Spese", nameof(RataFinanziamento.Fees), "C2", 75);
            AddColumn("Totale rata", nameof(RataFinanziamento.Total), "C2", 95);
            AddColumn("Residuo", nameof(RataFinanziamento.RemainingPrincipal), "C2", 110);
            AddColumn("Scostamento", nameof(RataFinanziamento.Discrepancy), "C2", 95);
            AddColumn("Verificato", nameof(RataFinanziamento.VerifiedPrincipal), "C2", 110);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await Execute(() => Task.CompletedTask);
        }

        private void AddColumn(string caption, string property, string format, int width)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = caption, DataPropertyName = property, Width = width, SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle { Format = format, FormatProvider = ItalianCulture, Alignment = DataGridViewContentAlignment.MiddleRight },
            });
        }

        internal void Render(PianoFinanziamento plan)
        {
            _plan = plan;
            Finanziamento loan = plan.Finanziamento;
            titleLabel.Text = $"{loan.DisplayName} — {loan.Lender}";
            summaryLabel.Text = $"Capitale residuo stimato: {Money(plan.RemainingPrincipal)}\n"
                + $"Rata: {Money(loan.Installment + loan.Insurance + loan.Fees)}   •   Rate residue: {plan.RemainingInstallments}   •   Totale: {Money(plan.RemainingTotal)}\n"
                + $"Ultima scadenza: {plan.LastDueDate:dd/MM/yy}   •   Riferimento: {plan.ReferenceDate:dd/MM/yy} (rate precedenti considerate pagate)\n"
                + (plan.LastAlignment is { } alignment ? $"Residuo verificato: {Money(alignment.VerifiedPrincipal!.Value)} dopo la rata {alignment.Number} del {alignment.DueDate:dd/MM/yy}"
                    : "Nessun residuo verificato: stima dai dati contrattuali.");
            warningLabel.Text = plan.FinalPrincipal != 0 || plan.TotalDiscrepancy != 0
                ? $"Scostamento del piano: capitale finale {Money(plan.FinalPrincipal)}, eccedenza rate {Money(plan.TotalDiscrepancy)}. Rate contrattuali invariate."
                : "Il residuo stimato non è un conteggio ufficiale di estinzione anticipata.";
            ApplyFilter();
        }

        private static string Money(decimal value) => value.ToString("C2", ItalianCulture);

        private void ApplyFilter()
        {
            int? selected = (grid.CurrentRow?.DataBoundItem as RataFinanziamento)?.Number;
            grid.DataSource = _plan?.Installments.Where(row => showPastCheckBox.Checked || !row.IsPast).ToList();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.DataBoundItem is RataFinanziamento installment)
                {
                    if (installment.VerifiedPrincipal is not null)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(219, 238, 252);
                    }
                    if (installment.Number == selected)
                    {
                        grid.CurrentCell = row.Cells[0];
                    }
                }
            }
            UpdateActions();
        }

        private void UpdateActions()
        {
            RataFinanziamento? row = grid.CurrentRow?.DataBoundItem as RataFinanziamento;
            alignButton.Enabled = !_busy && row?.IsPast == true;
            deleteButton.Enabled = !_busy && row?.IsPast == true && row.VerifiedPrincipal is not null;
        }

        private async Task Execute(Func<Task> action)
        {
            if (_busy)
            {
                return;
            }
            _busy = true;
            grid.Enabled = false;
            refreshButton.Enabled = false;
            UpdateActions();
            try
            {
                await action();
                Render(await _client.GetPianoFinanziamento(_name));
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _busy = false;
                grid.Enabled = true;
                refreshButton.Enabled = true;
                UpdateActions();
            }
        }

        private void ShowPastCheckBoxCheckedChanged(object? sender, EventArgs e) => ApplyFilter();

        private void GridSelectionChanged(object? sender, EventArgs e) => UpdateActions();

        private async void RefreshButtonClick(object? sender, EventArgs e) => await Execute(() => Task.CompletedTask);

        private async void AlignButtonClick(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is not RataFinanziamento row || !row.IsPast || _busy)
            {
                return;
            }
            using var dialog = new RiallineamentoDialog(row.Number, row.DueDate, row.VerifiedPrincipal ?? row.RemainingPrincipal);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await Execute(() => _client.SaveRiallineamento(_name, row.Number, dialog.Principal, row.VerifiedPrincipal is null));
            }
        }

        private async void DeleteButtonClick(object? sender, EventArgs e)
        {
            if (grid.CurrentRow?.DataBoundItem is RataFinanziamento row && row.IsPast && row.VerifiedPrincipal is not null
                && MessageBox.Show(this, $"Eliminare il residuo verificato dopo la rata {row.Number}? Il piano verrà ricalcolato.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                await Execute(() => _client.DeleteRiallineamento(_name, row.Number));
            }
        }
    }
}

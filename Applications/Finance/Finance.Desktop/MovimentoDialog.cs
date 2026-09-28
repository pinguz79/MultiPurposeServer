using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class MovimentoDialog : Form
    {
        private readonly FinanceApiClient _client;
        private readonly Conto _conto;
        private readonly MovimentoEdit? _movement;
        private bool _amountChanged;
        private bool _busy;

        public MovimentoDialog(FinanceApiClient client, Conto conto, MovimentoEdit? movement = null)
        {
            _client = client;
            _conto = conto;
            _movement = movement;
            InitializeComponent();
            Text = $"{(movement is null ? "Nuovo movimento" : "Modifica movimento")} — {conto.DisplayName}";
            dateInput.Value = movement?.Date.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
            descriptionInput.Text = movement?.Description ?? string.Empty;
            amountInput.Value = Math.Abs(movement?.Amount ?? 0m);
            expenseCheck.Checked = movement is null || (conto.HasCycles ? movement.Amount >= 0 : movement.Amount <= 0);
            confirmedCheck.Checked = movement?.IsConfirmed ?? dateInput.Value.Date <= DateTime.Today;
            formulaLabel.Text = movement is null ? string.Empty : $"Formula: {movement.Formula}";
            _amountChanged = false;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            saveButton.Enabled = false;
            try
            {
                categoryInput.Items.Add(new Categoria(string.Empty, "Nessuna", 0));
                foreach (Categoria category in await _client.GetCategorie())
                {
                    categoryInput.Items.Add(category);
                }

                categoryInput.DisplayMember = nameof(Categoria.DisplayName);
                categoryInput.SelectedItem = categoryInput.Items.Cast<Categoria>().FirstOrDefault(item => item.Name == _movement?.Category?.Name) ?? categoryInput.Items[0];
                saveButton.Enabled = true;
                descriptionInput.Focus();
            }
            catch (Exception exception)
            {
                errorLabel.Text = exception.Message;
            }
        }

        private void DateInputValueChanged(object? sender, EventArgs e)
        {
            if (_movement is null)
            {
                confirmedCheck.Checked = dateInput.Value.Date <= DateTime.Today;
            }
        }

        private void AmountChanged(object? sender, EventArgs e) => _amountChanged = true;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = _busy;
            base.OnFormClosing(e);
        }

        private async void SaveButtonClick(object? sender, EventArgs e) => await Save();

        public async Task Save()
        {
            if (_busy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(descriptionInput.Text))
            {
                errorLabel.Text = "Inserire una descrizione.";
                descriptionInput.Focus();
                return;
            }

            _busy = true;
            saveButton.Enabled = false;
            try
            {
                decimal amount = amountInput.Value * (expenseCheck.Checked == _conto.HasCycles ? 1m : -1m);
                string formula = _movement is not null && !_amountChanged ? _movement.Formula : amount.ToString("0.00", CultureInfo.InvariantCulture);
                string? category = (categoryInput.SelectedItem as Categoria)?.Name;
                await _client.SaveMovimento(_movement?.Id, new SaveMovimento(_conto.Name, DateOnly.FromDateTime(dateInput.Value), descriptionInput.Text.Trim(), formula,
                    confirmedCheck.Checked, string.IsNullOrEmpty(category) ? null : category));
                _busy = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                errorLabel.Text = exception.Message;
            }
            finally
            {
                _busy = false;
                saveButton.Enabled = true;
            }
        }
    }
}

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class CartaRevolvingDialog : Form
    {
        private readonly FinanceApiClient _client;
        private readonly Conto _carta;
        private bool _saving;

        public CartaRevolvingDialog(FinanceApiClient client, Conto carta, IReadOnlyList<Conto> conti)
        {
            _client = client;
            _carta = carta;
            InitializeComponent();
            Text = $"Carta revolving — {carta.DisplayName}";
            contoAddebitoComboBox.DisplayMember = nameof(Conto.DisplayName);
            contoAddebitoComboBox.Items.AddRange([.. conti.Where(conto => conto.Id != carta.Id).Cast<object>()]);
            contoAddebitoComboBox.SelectedIndex = contoAddebitoComboBox.Items.Count == 0 ? -1 : 0;
            validFromInput.Value = DateTime.Today;
            validToInput.Value = new DateTime(DateTime.Today.Year + 10, 12, 31);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            plafondInput.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel |= _saving;
            base.OnFormClosing(e);
        }

        private ConfigureCartaRevolving CreateRequest() => new(plafondInput.Value, percentualeScopertoInput.Value / 100m, quotaRataInput.Value / 100m, rataMinimaInput.Value, tanInput.Value / 100m, bolloInput.Value, sogliaBolloInput.Value, decimal.ToInt32(chiusuraCicloInput.Value), decimal.ToInt32(addebitoInput.Value), (contoAddebitoComboBox.SelectedItem as Conto)?.Name ?? string.Empty, DateOnly.FromDateTime(validFromInput.Value), DateOnly.FromDateTime(validToInput.Value));

        private void InputValueChanged(object? sender, EventArgs e) => ValidateInput(false);

        private bool ValidateInput(bool focusError)
        {
            Control? invalidControl = plafondInput.Value <= 0m ? plafondInput
                : contoAddebitoComboBox.SelectedItem is not Conto ? contoAddebitoComboBox
                : validFromInput.Value.Date > validToInput.Value.Date ? validToInput
                : addebitoInput.Value <= chiusuraCicloInput.Value ? addebitoInput : null;
            errorLabel.Text = invalidControl == plafondInput ? "Il plafond deve essere maggiore di zero."
                : invalidControl == contoAddebitoComboBox ? "Selezionare un conto di addebito diverso dalla carta."
                : invalidControl == validToInput ? "La data finale non può precedere quella iniziale."
                : invalidControl == addebitoInput ? "L'addebito deve seguire la chiusura nello stesso mese." : string.Empty;

            if (focusError)
            {
                invalidControl?.Focus();
            }
            return invalidControl is null;
        }

        private async void SaveButtonClick(object? sender, EventArgs e) => await Save();

        internal async Task Save()
        {
            if (_saving || !ValidateInput(true))
            {
                return;
            }

            bool succeeded = false;
            try
            {
                SetBusy(true);
                await _client.ConfigureCartaRevolving(_carta.Name, CreateRequest());
                succeeded = true;
            }
            catch (Exception exception)
            {
                errorLabel.Text = exception.Message;
            }
            finally
            {
                SetBusy(false);
            }

            if (succeeded)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void SetBusy(bool busy)
        {
            _saving = busy;
            UseWaitCursor = busy;
            inputPanel.Enabled = !busy;
            saveButton.Enabled = !busy;
            cancelButton.Enabled = !busy;
        }
    }
}

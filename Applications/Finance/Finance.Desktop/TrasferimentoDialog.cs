using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Presentation;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class TrasferimentoDialog : Form
    {
        private readonly FinanceApiClient _client;
        private readonly IReadOnlyList<Conto> _accounts;
        private readonly Dictionary<Guid, IReadOnlyList<ParametroConto>> _parameters = [];
        private bool _ready;
        private bool _saving;
        private bool _uncertain;
        private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");

        public TrasferimentoDialog(FinanceApiClient client, IReadOnlyList<Conto> accounts, Guid? destinationId = null)
        {
            _client = client;
            _accounts = accounts;
            InitializeComponent();
            originInput.DisplayMember = destinationInput.DisplayMember = nameof(Conto.DisplayName);
            originInput.Items.AddRange(accounts.Cast<object>().ToArray());
            destinationInput.Items.AddRange(accounts.Cast<object>().ToArray());
            destinationInput.SelectedItem = accounts.FirstOrDefault(account => account.Id == destinationId);
            dateInput.Value = DateTime.Today;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadParameters();
        }

        internal async Task LoadParameters()
        {
            try
            {
                errorLabel.Text = "Caricamento dei parametri dei conti…";
                var results = await Task.WhenAll(_accounts.Select(async account => (account.Id, Parameters: await _client.GetParametriConto(account.Name))));
                if (IsDisposed)
                {
                    return;
                }
                foreach (var result in results)
                {
                    _parameters[result.Id] = result.Parameters;
                }
                _ready = true;
                errorLabel.Text = "";
                UpdatePreview();
            }
            catch (Exception exception)
            {
                if (!IsDisposed)
                {
                    errorLabel.Text = exception.Message;
                }
            }
        }

        private void InputChanged(object? sender, EventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            saveButton.Enabled = false;
            if (!_ready || _saving || _uncertain)
            {
                return;
            }
            if (originInput.SelectedItem is not Conto origin || destinationInput.SelectedItem is not Conto destination)
            {
                previewLabel.Text = "Seleziona il conto origine e il conto destinazione.";
                return;
            }
            if (origin.Id == destination.Id)
            {
                previewLabel.Text = "Origine e destinazione devono essere diverse.";
                return;
            }
            DateOnly date = DateOnly.FromDateTime(dateInput.Value);
            decimal originAmount = TransferPreview.IsDebtAccount(_parameters[origin.Id], date) ? amountInput.Value : -amountInput.Value;
            decimal destinationAmount = TransferPreview.IsDebtAccount(_parameters[destination.Id], date) ? -amountInput.Value : amountInput.Value;
            string Format(decimal value) => value.ToString("+0.00;-0.00;0.00", Italian) + " €";
            previewLabel.Text = $"Movimenti del {date:dd/MM/yyyy}, senza categoria:\n\n{origin.DisplayName}: {Format(originAmount)}\n{destination.DisplayName}: {Format(destinationAmount)}\n\n"
                + (confirmedCheck.Checked ? "Entrambi confermati, senza collegamento." : "Entrambi da confermare e collegati. Confermandone uno verrà confermato anche l’altro.");
            saveButton.Enabled = amountInput.Value > 0 && !string.IsNullOrWhiteSpace(descriptionInput.Text);
        }

        private async void SaveButtonClick(object? sender, EventArgs e) => await Save();

        internal async Task Save()
        {
            if (_saving || !saveButton.Enabled || originInput.SelectedItem is not Conto origin || destinationInput.SelectedItem is not Conto destination)
            {
                return;
            }
            _saving = true;
            fieldsPanel.Enabled = saveButton.Enabled = cancelButton.Enabled = false;
            errorLabel.Text = "Salvataggio…";
            try
            {
                await _client.CreateTrasferimento(new SaveTrasferimento(origin.Name, destination.Name, amountInput.Value,
                    DateOnly.FromDateTime(dateInput.Value), descriptionInput.Text.Trim(), confirmedCheck.Checked));
                _saving = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                _uncertain = exception is TimeoutException or HttpRequestException
                    || exception is FinanceApiException apiException && (int)apiException.StatusCode >= 500;
                errorLabel.Text = exception.Message + (_uncertain ? " Verifica i movimenti prima di riprovare: il trasferimento potrebbe essere già stato registrato." : "");
            }
            finally
            {
                _saving = false;
                if (!IsDisposed)
                {
                    fieldsPanel.Enabled = cancelButton.Enabled = true;
                    UpdatePreview();
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = _saving;
            base.OnFormClosing(e);
        }

        private void AmountEnter(object? sender, EventArgs e)
        {
            BeginInvoke(() =>
            {
                if (!IsDisposed && amountInput.ContainsFocus)
                {
                    amountInput.Select(0, amountInput.Text.Length);
                }
            });
        }

        private void AmountKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
            {
                e.KeyChar = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            }
        }
    }
}

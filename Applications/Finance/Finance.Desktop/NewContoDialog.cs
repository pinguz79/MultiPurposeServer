using System.Globalization;
using System.Text;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class NewContoDialog : Form
    {
        private readonly FinanceApiClient _client;
        private bool _displayNameEdited;
        private bool _updatingDisplayName;

        public NewContoDialog(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
            initialBalanceInput.Minimum = decimal.MinValue;
            initialBalanceInput.Maximum = decimal.MaxValue;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            nameInput.Focus();
        }

        private void NameInputTextChanged(object? sender, EventArgs e)
        {
            var normalizedName = TryNormalizeName(nameInput.Text, out var error);
            namePreviewValue.Text = normalizedName;
            nameErrorLabel.Text = error;

            if (_displayNameEdited)
            {
                return;
            }

            _updatingDisplayName = true;
            displayNameInput.Text = nameInput.Text;
            _updatingDisplayName = false;
        }

        private void DisplayNameInputTextChanged(object? sender, EventArgs e)
        {
            _displayNameEdited |= !_updatingDisplayName;
            ValidateDisplayName();
        }

        private void InitialBalanceInputValueChanged(object? sender, EventArgs e) => initialBalanceErrorLabel.Text = string.Empty;

        private async void CreateButtonClick(object? sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            try
            {
                SetBusy(true);
                var request = new CreateConto(nameInput.Text, displayNameInput.Text, initialBalanceInput.Value);
                await _client.CreateConto(request);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (FinanceApiException exception) when (string.Equals(exception.Field, "Name", StringComparison.OrdinalIgnoreCase))
            {
                nameErrorLabel.Text = exception.Message;
                nameInput.Focus();
            }
            catch (Exception exception)
            {
                generalErrorLabel.Text = exception.Message;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private bool ValidateForm()
        {
            _ = TryNormalizeName(nameInput.Text, out var nameError);
            nameErrorLabel.Text = nameError;
            var displayNameValid = ValidateDisplayName();
            initialBalanceErrorLabel.Text = string.Empty;
            generalErrorLabel.Text = string.Empty;

            if (!string.IsNullOrEmpty(nameError))
            {
                nameInput.Focus();
                return false;
            }

            if (!displayNameValid)
            {
                displayNameInput.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateDisplayName()
        {
            displayNameErrorLabel.Text = string.IsNullOrWhiteSpace(displayNameInput.Text) ? "Il nome visualizzato è obbligatorio." : string.Empty;

            return displayNameErrorLabel.Text.Length == 0;
        }

        private static string TryNormalizeName(string value, out string error)
        {
            var tokens = SplitTokens(value);

            if (tokens.Count == 0)
            {
                error = "Il nome è obbligatorio.";
                return string.Empty;
            }

            if (!char.IsLetter(tokens[0][0]))
            {
                error = "Il nome deve iniziare con una lettera.";
                return string.Empty;
            }

            error = string.Empty;
            return string.Concat(tokens.Select(token => char.ToUpper(token[0], CultureInfo.InvariantCulture) + token[1..].ToLowerInvariant()));
        }

        private static List<string> SplitTokens(string value)
        {
            var tokens = new List<string>();
            var token = new StringBuilder();

            foreach (var character in value.Trim())
            {
                if (char.IsLetterOrDigit(character))
                {
                    if (char.IsUpper(character) && token.Length > 0 && char.IsLower(token[^1]))
                    {
                        tokens.Add(token.ToString());
                        token.Clear();
                    }

                    token.Append(character);
                }
                else if (token.Length > 0)
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                }
            }

            if (token.Length > 0)
            {
                tokens.Add(token.ToString());
            }

            return tokens;
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            createButton.Enabled = !busy;
            cancelButton.Enabled = !busy;
        }
    }
}

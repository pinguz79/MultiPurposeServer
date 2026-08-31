using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class PianificazioneDialog : Form
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private readonly FinanceApiClient _client;
        private readonly VoceRicorrente _voce;
        private System.Windows.Forms.Timer? _previewTimer;

        public PianificazioneDialog(FinanceApiClient client, VoceRicorrente voce)
        {
            _client = client;
            _voce = voce;
            InitializeComponent();
            ConfigurePreview();
            InitializeValues();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadAccounts();
            await LoadCategories();
            SchedulePreview();
        }

        private async void CreateButtonClick(object? sender, EventArgs e)
        {
            try
            {
                createButton.Enabled = false;
                UseWaitCursor = true;
                Pianificazione result = await _client.CreatePianificazione(CreateRequest());
                MessageBox.Show(this, $"Pianificazione creata: {result.OccurrenceCount} movimenti.", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                createButton.Enabled = true;
            }
        }

        private void EndOfMonthCheckedChanged(object? sender, EventArgs e)
        {
            dayInput.Enabled = !endOfMonthCheckBox.Checked;
            SchedulePreview();
        }

        private void InputValueChanged(object? sender, EventArgs e) => SchedulePreview();

        private void ConfigurePreview()
        {
            previewGrid.Columns.Add("Date", "Data");
            previewGrid.Columns.Add("Value", "Importo");
            previewGrid.Columns.Add("Category", "Categoria");
            previewGrid.Columns.Add("Status", "Stato");
            previewGrid.Columns.Add("Messages", "Note");
        }

        private void InitializeValues()
        {
            DateTime today = DateTime.Today;
            DateTime end = new(today.Month <= 6 ? today.Year : today.Year + 1, 12, 31);
            descriptionTextBox.Text = _voce.DisplayName;
            movimentoDescriptionTextBox.Text = _voce.DisplayName;
            formulaTextBox.Text = $"[{_voce.Name}]";
            validFromInput.Value = today;
            validToInput.Value = end;
            dayInput.Value = today.Day;
            components ??= new System.ComponentModel.Container();
            _previewTimer = new System.Windows.Forms.Timer(components) { Interval = 350 };
            _previewTimer.Tick += PreviewTimerTick;
        }

        private async Task LoadAccounts()
        {
            IReadOnlyList<Conto> accounts = await _client.GetConti();
            contoComboBox.DataSource = accounts.ToList();
            contoComboBox.DisplayMember = nameof(Conto.DisplayName);
            contoComboBox.ValueMember = nameof(Conto.Name);
        }

        private async Task LoadCategories()
        {
            IReadOnlyList<Categoria> categories = await _client.GetCategorie();
            categoryComboBox.Items.Add(new PianificazioneCategoriaOption(ModalitaCategoria.Ereditata, null, "Ereditata dalla voce ricorrente"));
            categoryComboBox.Items.Add(new PianificazioneCategoriaOption(ModalitaCategoria.Nessuna, null, "Nessuna categoria"));
            categoryComboBox.Items.AddRange([.. categories.Select(category => new PianificazioneCategoriaOption(
                ModalitaCategoria.Esplicita,
                category.Name,
                category.DisplayName))]);
            categoryComboBox.SelectedIndex = 0;
        }

        private void SchedulePreview()
        {
            if (_previewTimer is null)
            {
                return;
            }

            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private async void PreviewTimerTick(object? sender, EventArgs e)
        {
            _previewTimer?.Stop();

            if (contoComboBox.SelectedValue is null)
            {
                return;
            }

            try
            {
                PianificazionePreview preview = await _client.PreviewPianificazione(CreateRequest());
                RenderPreview(preview);
            }
            catch (Exception exception)
            {
                errorLabel.Text = exception.Message;
                createButton.Enabled = false;
            }
        }

        private void RenderPreview(PianificazionePreview preview)
        {
            previewGrid.Rows.Clear();

            foreach (PianificazioneOccurrence occurrence in preview.Occurrences)
            {
                previewGrid.Rows.Add(
                    occurrence.Date.ToString("dd/MM/yy", ItalianCulture),
                    occurrence.Value?.ToString("N2", ItalianCulture) ?? "-",
                    occurrence.Category?.DisplayName ?? "-",
                    occurrence.Status,
                    string.Join(" ", occurrence.Messages));
            }

            errorLabel.Text = preview.Errors.Count == 0 ? string.Empty : string.Join(" ", preview.Errors);
            summaryLabel.Text = $"{preview.OccurrenceCount} occorrenze";
            createButton.Enabled = preview.IsValid;
        }

        private CreatePianificazione CreateRequest()
        {
            var category = (PianificazioneCategoriaOption?)categoryComboBox.SelectedItem
                ?? new PianificazioneCategoriaOption(ModalitaCategoria.Ereditata, null, "Ereditata dalla voce ricorrente");

            return new CreatePianificazione(
                contoComboBox.SelectedValue?.ToString() ?? string.Empty,
                descriptionTextBox.Text,
                movimentoDescriptionTextBox.Text,
                formulaTextBox.Text,
                DateOnly.FromDateTime(validFromInput.Value),
                DateOnly.FromDateTime(validToInput.Value),
                1,
                endOfMonthCheckBox.Checked ? null : (int)dayInput.Value,
                endOfMonthCheckBox.Checked,
                category.Mode,
                category.Name,
                category.Mode == ModalitaCategoria.Ereditata ? _voce.Name : null);
        }

    }
}

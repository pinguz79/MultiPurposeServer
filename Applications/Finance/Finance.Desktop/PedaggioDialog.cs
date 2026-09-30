using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class PedaggioDialog : Form
    {
        private static readonly Casello NewStation = new(Guid.Empty, "Aggiungi nuova stazione…");
        private readonly FinanceApiClient _client;
        private readonly Conto _conto;
        private readonly MovimentoEdit? _movement;
        private List<Casello> _stations = [];
        private IReadOnlyList<TariffaTratta> _tariffs = [];
        private bool _rendering;
        private bool _busy;
        private string _generatedDescription = string.Empty;
        private decimal _tariff;

        public PedaggioDialog(FinanceApiClient client, Conto conto, MovimentoEdit? movement = null)
        {
            _client = client;
            _conto = conto;
            _movement = movement;
            InitializeComponent();
            Text = $"{(movement is null ? "Nuovo pedaggio" : "Modifica pedaggio")} — {conto.DisplayName}";
            dateInput.Value = movement?.Date.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
            descriptionInput.Text = movement?.Description ?? string.Empty;
            confirmedCheck.Checked = movement?.IsConfirmed ?? false;
            editAmountButton.Visible = movement?.IsConfirmed == true;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadData();
        }

        internal async Task LoadData()
        {
            _busy = true;
            try
            {
                _stations = [.. await _client.GetCaselli()];
                _tariffs = await _client.GetTariffe();
                categoryInput.DisplayMember = nameof(Categoria.DisplayName);
                categoryInput.Items.Add(new Categoria(string.Empty, "Nessuna", 0));
                foreach (Categoria category in await _client.GetCategorie())
                {
                    categoryInput.Items.Add(category);
                }

                categoryInput.SelectedItem = categoryInput.Items.Cast<Categoria>().FirstOrDefault(item => item.Name == _movement?.Category?.Name) ?? categoryInput.Items[0];
                ReloadEntrata(_movement?.Pedaggio?.EntrataId);
                if (_movement?.Pedaggio is Pedaggio pedaggio)
                {
                    uscitaInput.SelectedItem = uscitaInput.Items.Cast<Casello>().FirstOrDefault(item => item.Id == pedaggio.UscitaId);
                }

                RefreshPrice();
                entrataInput.Focus();
            }
            catch (Exception exception)
            {
                errorLabel.Text = exception.Message;
            }
            finally
            {
                _busy = false;
            }
        }

        private void ReloadEntrata(Guid? selected)
        {
            _rendering = true;
            entrataInput.DisplayMember = nameof(Casello.Name);
            entrataInput.DataSource = _stations.OrderBy(item => item.Name).Append(NewStation).ToList();
            entrataInput.SelectedItem = _stations.FirstOrDefault(item => item.Id == selected);
            _rendering = false;
            ReloadUscita(true);
        }

        private void ReloadUscita(bool entranceChanged)
        {
            if (_rendering)
            {
                return;
            }

            _rendering = true;
            Guid? entrata = (entrataInput.SelectedItem as Casello)?.Id;
            HashSet<Guid> known = [.. _tariffs.Where(item => item.CaselloAId == entrata || item.CaselloBId == entrata)
                .Select(item => item.CaselloAId == entrata ? item.CaselloBId : item.CaselloAId)];
            if (entranceChanged)
            {
                otherStationsCheck.Checked = known.Count == 0;
            }

            List<Casello> choices = entrata is null || entrata == Guid.Empty ? []
                : [.. _stations.Where(item => item.Id != entrata && (otherStationsCheck.Checked ? !known.Contains(item.Id) : known.Contains(item.Id))).OrderBy(item => item.Name)];
            if (otherStationsCheck.Checked && entrata is not null && entrata != Guid.Empty)
            {
                choices.Add(NewStation);
            }

            uscitaInput.DisplayMember = nameof(Casello.Name);
            uscitaInput.DataSource = choices;
            uscitaInput.SelectedIndex = -1;
            _rendering = false;
            RefreshPrice();
        }

        private async void EntrataChanged(object? sender, EventArgs e)
        {
            if (_rendering)
            {
                return;
            }

            if (entrataInput.SelectedItem is Casello { Id: var id } && id == Guid.Empty)
            {
                Casello? created = await AddStation();
                ReloadEntrata(created?.Id);
                return;
            }

            ReloadUscita(true);
        }

        private async void UscitaChanged(object? sender, EventArgs e)
        {
            if (_rendering)
            {
                return;
            }

            if (uscitaInput.SelectedItem is Casello { Id: var id } && id == Guid.Empty)
            {
                Casello? created = await AddStation();
                Casello? entrance = entrataInput.SelectedItem as Casello;
                _rendering = true;
                entrataInput.DataSource = _stations.OrderBy(item => item.Name).Append(NewStation).ToList();
                entrataInput.SelectedItem = entrance;
                _rendering = false;
                ReloadUscita(false);
                if (created is not null)
                {
                    uscitaInput.SelectedItem = created;
                }
            }

            RefreshPrice();
        }

        private async Task<Casello?> AddStation()
        {
            if (_busy)
            {
                return null;
            }

            using var dialog = new CaselloDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return null;
            }

            _busy = true;
            entrataInput.Enabled = false;
            uscitaInput.Enabled = false;
            try
            {
                Casello station = await _client.CreateCasello(dialog.StationName);
                _stations.Add(station);
                return station;
            }
            catch (Exception exception)
            {
                errorLabel.Text = exception.Message;
                MessageBox.Show(this, exception.Message, "Nuova stazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            finally
            {
                _busy = false;
                entrataInput.Enabled = true;
                uscitaInput.Enabled = true;
            }
        }

        private void OtherStationsChanged(object? sender, EventArgs e) => ReloadUscita(false);

        private void DateChanged(object? sender, EventArgs e) => RefreshPrice();

        private void RefreshPrice()
        {
            if (_rendering)
            {
                return;
            }

            Casello? entrata = entrataInput.SelectedItem as Casello;
            Casello? uscita = uscitaInput.SelectedItem as Casello;
            bool selected = entrata is not null && uscita is not null && entrata.Id != Guid.Empty && uscita.Id != Guid.Empty;
            DateOnly date = DateOnly.FromDateTime(dateInput.Value);
            TariffaTratta? tariffa = _tariffs.Where(item => ((item.CaselloAId == entrata?.Id && item.CaselloBId == uscita?.Id)
                || (item.CaselloBId == entrata?.Id && item.CaselloAId == uscita?.Id)) && (item.ValidFrom is null || item.ValidFrom <= date)
                && (item.ValidTo is null || item.ValidTo >= date)).OrderBy(item => item.Index).FirstOrDefault();
            _tariff = tariffa is null ? 0m : decimal.Parse(tariffa.Formula, CultureInfo.InvariantCulture);
            bool frozen = _movement?.IsConfirmed == true && confirmedCheck.Checked;
            amountLabel.Text = (frozen ? _movement!.Amount ?? 0m : _tariff).ToString("C2", CultureInfo.GetCultureInfo("it-IT"));
            confirmedCheck.Enabled = frozen || selected && _tariff > 0;
            if (!confirmedCheck.Enabled)
            {
                confirmedCheck.Checked = false;
            }

            errorLabel.Text = selected && _tariff == 0 && !frozen ? "Tariffa non disponibile: il pedaggio resterà da confermare." : string.Empty;
            saveButton.Enabled = selected;
            if (selected)
            {
                string description = $"{entrata!.Name} → {uscita!.Name}";
                if (descriptionInput.Text.Length == 0 || descriptionInput.Text == _generatedDescription)
                {
                    descriptionInput.Text = description;
                }

                _generatedDescription = description;
            }
        }

        private async void SaveClick(object? sender, EventArgs e)
        {
            if (_busy || entrataInput.SelectedItem is not Casello entrata || uscitaInput.SelectedItem is not Casello uscita)
            {
                return;
            }

            _busy = true;
            saveButton.Enabled = false;
            try
            {
                string? category = (categoryInput.SelectedItem as Categoria)?.Name;
                await _client.SavePedaggio(_movement?.Id, new SavePedaggio(_conto.Name, DateOnly.FromDateTime(dateInput.Value), entrata.Id, uscita.Id,
                    descriptionInput.Text, string.IsNullOrEmpty(category) ? null : category, confirmedCheck.Checked));
                _busy = false;
                DialogResult = DialogResult.OK;
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

        private void EditAmountClick(object? sender, EventArgs e)
        {
            using var dialog = new MovimentoDialog(_client, _conto, _movement);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = _busy;
            base.OnFormClosing(e);
        }
    }
}

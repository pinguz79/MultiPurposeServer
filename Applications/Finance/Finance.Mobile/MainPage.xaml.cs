using System.Net;

using Finance.Client;

using MultiPurposeServer.Client;

namespace Finance.Mobile
{
    public partial class MainPage : ContentPage
    {
        private const string ServerPreferenceKey = "server";
        private const string ApiKeyStorageKey = "finance-api-key";

        private readonly FinanceApiClientFactory _clientFactory;
        private readonly IPreferences _preferences;
        private readonly ISecureStorage _secureStorage;
        private bool _isLoading;
        private CancellationTokenSource? _request;

        public MainPage(FinanceApiClientFactory clientFactory, IPreferences preferences,
            ISecureStorage secureStorage)
        {
            _clientFactory = clientFactory;
            _preferences = preferences;
            _secureStorage = secureStorage;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Load();
        }

        protected override void OnDisappearing()
        {
            _request?.Cancel();
            base.OnDisappearing();
        }

        private async void Refresh(object? sender, EventArgs e) => await Load();

        private async void Configure(object? sender, EventArgs e)
        {
            string? address = await DisplayPromptAsync("Connessione MPS", "Indirizzo HTTPS del server", initialValue: _preferences.Get(ServerPreferenceKey, ""));
            if (address is null)
            {
                return;
            }

            Uri server;
            try
            {
                server = ApiClient.ValidateServer(address);
            }
            catch (ArgumentException exception)
            {
                await DisplayAlertAsync("Indirizzo non valido", exception.Message, "OK");
                return;
            }

            // Non precompilare né mostrare la chiave già memorizzata.
            var page = new KeyPage();
            await Navigation.PushModalAsync(page);
            string? key = await page.Result;
            if (key is null)
            {
                return;
            }

            try
            {
                await _secureStorage.SetAsync(ApiKeyStorageKey, key.Trim());
                _preferences.Set(ServerPreferenceKey, server.AbsoluteUri);
                await Load();
            }
            catch (Exception)
            {
                await DisplayAlertAsync("Configurazione", "Impossibile salvare la configurazione protetta sul dispositivo.", "OK");
            }
        }

        private async Task Load()
        {
            if (_isLoading)
            {
                return;
            }

            string address = _preferences.Get(ServerPreferenceKey, "");
            if (string.IsNullOrWhiteSpace(address))
            {
                return;
            }

            _isLoading = true;
            RefreshButton.IsEnabled = SettingsButton.IsEnabled = false;
            BusyIndicator.IsVisible = BusyIndicator.IsRunning = true;
            Accounts.ItemsSource = null;
            StatusLabel.Text = "Caricamento conti...";
            using var cancellation = new CancellationTokenSource();
            _request = cancellation;
            try
            {
                string? key = await _secureStorage.GetAsync(ApiKeyStorageKey);
                FinanceApiClient client = _clientFactory.Create(ApiClient.ValidateServer(address), key);
                IReadOnlyList<Conto> accounts = await client.GetConti(cancellation.Token);
                Accounts.ItemsSource = accounts.Select(account => new AccountCard(account)).ToList();
                StatusLabel.Text = accounts.Count == 0 ? "Nessun conto presente." : $"Aggiornato alle {DateTime.Now:HH:mm}";
            }
            catch (ApiException exception)
            {
                StatusLabel.Text = exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                    ? "Accesso non autorizzato. Controlla la chiave nelle impostazioni di connessione." : exception.Message;
            }
            catch (OperationCanceledException)
            {
                StatusLabel.Text = "Caricamento interrotto o tempo di attesa scaduto. Premi Aggiorna per una nuova lettura.";
            }
            catch (HttpRequestException)
            {
                StatusLabel.Text = "Server non raggiungibile. Controlla la connessione e premi Aggiorna.";
            }
            catch (Exception)
            {
                StatusLabel.Text = "Impossibile caricare i conti. Verifica la configurazione della connessione.";
            }
            finally
            {
                _request = null;
                _isLoading = false;
                RefreshButton.IsEnabled = SettingsButton.IsEnabled = true;
                BusyIndicator.IsVisible = BusyIndicator.IsRunning = false;
            }
        }
    }
}

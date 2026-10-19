using Finance.Client;

namespace Finance.Mobile
{
    public static class MauiProgram
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder().UseMauiApp<App>();
            builder.Services.AddSingleton(_ => new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = RequestTimeout });
            builder.Services.AddSingleton(Preferences.Default);
            builder.Services.AddSingleton(SecureStorage.Default);
            builder.Services.AddSingleton<FinanceApiClientFactory>();
            builder.Services.AddTransient<MainPage>();
            return builder.Build();
        }
    }
}

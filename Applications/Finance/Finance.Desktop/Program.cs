using Finance.Desktop.Configuration;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            var configuration = AppConfiguration.Load();
            Application.Run(new MainForm(new FinanceApiClient(configuration.Api)));
        }
    }
}

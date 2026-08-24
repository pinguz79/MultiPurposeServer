using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class MainForm : Form
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");

        private readonly FinanceApiClient _client;

        public MainForm(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await RefreshConti();
        }

        private async void NewContoMenuItemClick(object? sender, EventArgs e)
        {
            using var dialog = new NewContoDialog(_client);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await RefreshConti();
            }
        }

        private async Task RefreshConti()
        {
            try
            {
                UseWaitCursor = true;
                var conti = await _client.GetConti();
                RenderConti(conti);
            }
            catch (Exception exception)
            {
                accountsPanel.Controls.Clear();
                accountsPanel.Controls.Add(CreateMessageLabel($"Impossibile caricare i conti. {exception.Message}"));
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void RenderConti(IReadOnlyList<Conto> conti)
        {
            accountsPanel.SuspendLayout();
            accountsPanel.Controls.Clear();

            for (var index = 0; index < conti.Count; index++)
            {
                accountsPanel.Controls.Add(CreateContoCard(conti[index], index == 0));
            }

            accountsPanel.ResumeLayout();
        }

        private static Control CreateContoCard(Conto conto, bool highlighted)
        {
            var card = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(12),
                Padding = new Padding(18),
                Size = highlighted ? new Size(430, 140) : new Size(280, 105),
            };
            var nameLabel = new Label
            {
                AutoEllipsis = true,
                AutoSize = false,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", highlighted ? 16F : 12F, FontStyle.Bold),
                Height = highlighted ? 42 : 32,
                Text = conto.DisplayName,
            };
            var balanceLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", highlighted ? 24F : 17F, FontStyle.Regular),
                ForeColor = conto.Balance < 0 ? Color.Firebrick : SystemColors.ControlText,
                Text = conto.Balance.ToString("N2", ItalianCulture) + " €",
                TextAlign = ContentAlignment.MiddleLeft,
            };

            card.Controls.Add(balanceLabel);
            card.Controls.Add(nameLabel);

            return card;
        }

        private static Label CreateMessageLabel(string message) => new()
        {
            AutoSize = true,
            ForeColor = Color.Firebrick,
            Margin = new Padding(16),
            Text = message,
        };
    }
}

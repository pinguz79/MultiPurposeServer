using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class MainForm : Form
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private static readonly Color PastMovementBackground = Color.FromArgb(238, 246, 252);
        private static readonly Color TodayMovementBackground = Color.FromArgb(183, 218, 247);

        private readonly FinanceApiClient _client;
        private Panel? _selectedCard;

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

        #region Conti

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
                _selectedCard = null;
                accountsPanel.Controls.Clear();
                RenderContiMenu([]);
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
            _selectedCard = null;
            accountsPanel.Controls.Clear();
            accountsPanel.FlowDirection = FlowDirection.LeftToRight;
            accountsPanel.WrapContents = true;

            for (var index = 0; index < conti.Count; index++)
            {
                accountsPanel.Controls.Add(CreateContoCard(conti[index], index == 0));
            }

            RenderContiMenu(conti);
            accountsPanel.ResumeLayout();
        }

        private void RecurringEntriesMenuItemClick(object? sender, EventArgs e)
        {
            accountsPanel.Controls.Clear();
            accountsPanel.FlowDirection = FlowDirection.LeftToRight;
            accountsPanel.WrapContents = false;
            accountsPanel.Controls.Add(new RecurringEntriesView(_client) { Dock = DockStyle.Fill, Size = accountsPanel.ClientSize });
        }

        private void RenderContiMenu(IReadOnlyList<Conto> conti)
        {
            while (contiMenuItem.DropDownItems.Count > 2)
            {
                contiMenuItem.DropDownItems.RemoveAt(2);
            }

            contiMenuSeparator.Visible = conti.Count > 0;
            foreach (Conto conto in conti)
            {
                var contoMenuItem = new ToolStripMenuItem(conto.DisplayName.Replace("&", "&&"));
                contoMenuItem.DropDownItems.Add("&Movimenti", null, async (_, _) => await ShowMovimenti(conto, DateTime.Today.Month, DateTime.Today.Year));
                contiMenuItem.DropDownItems.Add(contoMenuItem);
            }
        }

        private Control CreateContoCard(Conto conto, bool highlighted)
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
            card.Tag = conto;
            card.Cursor = Cursors.Hand;
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Apri movimenti", null, async (_, _) => await ShowMovimenti(conto, DateTime.Today.Month, DateTime.Today.Year));
            card.ContextMenuStrip = contextMenu;
            AttachCardEvents(card, conto, card);

            return card;
        }

        private void AttachCardEvents(Control control, Conto conto, Panel card)
        {
            control.Click += (_, _) => SelectCard(card);
            control.DoubleClick += async (_, _) => await ShowMovimenti(conto, DateTime.Today.Month, DateTime.Today.Year);

            foreach (Control child in control.Controls)
            {
                AttachCardEvents(child, conto, card);
            }
        }

        private void SelectCard(Panel card)
        {
            if (_selectedCard is not null)
            {
                _selectedCard.BackColor = Color.White;
            }

            _selectedCard = card;
            card.BackColor = Color.FromArgb(224, 238, 252);
        }

        #endregion

        #region Movimenti

        private async Task ShowMovimenti(Conto conto, int month, int year)
        {
            try
            {
                UseWaitCursor = true;
                ContoMovimenti timeline = await _client.GetMovimenti(conto.Name, month, year);
                RenderMovimenti(timeline);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Impossibile caricare i movimenti. {exception.Message}", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void RenderMovimenti(ContoMovimenti timeline)
        {
            accountsPanel.SuspendLayout();
            _selectedCard = null;
            accountsPanel.Controls.Clear();
            accountsPanel.FlowDirection = FlowDirection.TopDown;
            accountsPanel.WrapContents = false;

            var backButton = new Button { AutoSize = true, Text = "← Conti", Margin = new Padding(12) };
            backButton.Click += async (_, _) => await RefreshConti();
            accountsPanel.Controls.Add(backButton);
            accountsPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Margin = new Padding(12),
                Text = timeline.Conto.DisplayName,
            });

            accountsPanel.Controls.Add(CreatePeriodSelector(timeline));
            accountsPanel.Controls.Add(CreateOpeningBalanceRow(timeline));

            DateOnly renderedPeriod = new(timeline.From.Year, timeline.From.Month, 1);
            DateOnly finalPeriod = new(timeline.To.Year, timeline.To.Month, 1);
            while (renderedPeriod <= finalPeriod)
            {
                Movimento[] monthItems = [.. timeline.Items.Where(movimento => movimento.Date.Year == renderedPeriod.Year && movimento.Date.Month == renderedPeriod.Month)];
                bool selectedMonth = renderedPeriod.Month == timeline.SelectedMonth && renderedPeriod.Year == timeline.SelectedYear;
                accountsPanel.Controls.Add(CreateMonthSection(renderedPeriod, monthItems, selectedMonth));

                renderedPeriod = renderedPeriod.AddMonths(1);
            }

            accountsPanel.ResumeLayout();
        }

        private Control CreatePeriodSelector(ContoMovimenti timeline)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(12), WrapContents = false };
            var month = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };
            month.Items.AddRange([.. ItalianCulture.DateTimeFormat.MonthNames.Take(12).Select(value => ItalianCulture.TextInfo.ToTitleCase(value))]);
            month.SelectedIndex = timeline.SelectedMonth - 1;
            var year = new NumericUpDown { Maximum = 9999, Minimum = 1, Value = timeline.SelectedYear, Width = 80 };
            var previous = new Button { AutoSize = true, Text = "← Mese precedente" };
            var next = new Button { AutoSize = true, Text = "Mese successivo →" };
            var show = new Button { AutoSize = true, Text = "Visualizza" };

            previous.Click += async (_, _) => await ShowMovimenti(timeline.Conto, new DateOnly(timeline.SelectedYear, timeline.SelectedMonth, 1).AddMonths(-1));
            next.Click += async (_, _) => await ShowMovimenti(timeline.Conto, new DateOnly(timeline.SelectedYear, timeline.SelectedMonth, 1).AddMonths(1));
            show.Click += async (_, _) => await ShowMovimenti(timeline.Conto, month.SelectedIndex + 1, decimal.ToInt32(year.Value));
            panel.Controls.AddRange([previous, month, year, show, next]);

            return panel;
        }

        private Task ShowMovimenti(Conto conto, DateOnly period) => ShowMovimenti(conto, period.Month, period.Year);

        #endregion

        #region Controlli grafici

        private static Control CreateMonthSection(DateOnly date, IReadOnlyCollection<Movimento> movements, bool selectedMonth)
        {
            string title = date.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", ItalianCulture);
            var section = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(12, 18, 12, 3),
                WrapContents = false,
            };
            var header = new Button
            {
                BackColor = Color.FromArgb(52, 58, 64),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Margin = Padding.Empty,
                Size = new Size(880, 34),
                Text = $"▼  {title}",
                TextAlign = ContentAlignment.MiddleLeft,
                UseVisualStyleBackColor = false,
            };
            header.FlatAppearance.BorderSize = 0;
            var content = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(0, 3, 0, 0),
                WrapContents = false,
            };

            foreach (Movimento movement in movements)
            {
                content.Controls.Add(CreateMovimentoRow(movement));
            }

            if (movements.Count == 0 && selectedMonth)
            {
                content.Controls.Add(CreateEmptyMonthLabel());
            }

            header.Click += (_, _) =>
            {
                content.Visible = !content.Visible;
                header.Text = $"{(content.Visible ? "▼" : "▶")}  {title}";
            };
            section.Controls.Add(header);
            section.Controls.Add(content);

            return section;
        }

        private static Panel CreateMovimentoRow(Movimento movimento)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            Color background = movimento.Date == today ? TodayMovementBackground
                : movimento.Date < today ? PastMovementBackground : Color.White;
            var row = new Panel { BackColor = background, Margin = new Padding(0, 1, 0, 1), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Location = new Point(10, 11), Size = new Size(80, 22), Text = movimento.Date.ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoEllipsis = true, AutoSize = false, Location = new Point(100, 11), Size = new Size(430, 22), Text = movimento.Description });
            row.Controls.Add(new Label
            {
                AutoSize = false,
                ForeColor = movimento.Amount < 0 ? Color.Firebrick : SystemColors.ControlText,
                Location = new Point(545, 11),
                Size = new Size(135, 22),
                Text = movimento.Amount == 0 ? "-" : movimento.Amount.ToString("N2", ItalianCulture) + " €",
                TextAlign = ContentAlignment.TopRight,
            });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(695, 11), Size = new Size(165, 22), Text = movimento.BalanceAfter.ToString("N2", ItalianCulture) + " €", TextAlign = ContentAlignment.TopRight });

            return row;
        }

        private static Panel CreateOpeningBalanceRow(ContoMovimenti timeline)
        {
            var row = new Panel { BackColor = Color.FromArgb(225, 230, 235), Margin = new Padding(12, 1, 12, 1), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(10, 11), Size = new Size(80, 22), Text = timeline.From.AddDays(-1).ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(100, 11), Size = new Size(580, 22), Text = "Saldo precedente" });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(695, 11), Size = new Size(165, 22), Text = timeline.OpeningBalance.ToString("N2", ItalianCulture) + " €", TextAlign = ContentAlignment.TopRight });

            return row;
        }

        private static Label CreateEmptyMonthLabel() => new()
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(22, 10, 12, 10),
            Text = "Nessun movimento nel mese",
        };

        private static Label CreateMessageLabel(string message) => new()
        {
            AutoSize = true,
            ForeColor = Color.Firebrick,
            Margin = new Padding(16),
            Text = message,
        };

        #endregion
    }
}

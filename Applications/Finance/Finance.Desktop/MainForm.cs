using System.Globalization;

using Finance.Desktop.Models;
using Finance.Desktop.Presentation;
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
        private bool _showingConfiguration;

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
                _showingConfiguration = false;
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
                RenderContiMenu(conti);

                if (!_showingConfiguration)
                {
                    RenderConti(conti);
                }
            }
            catch (Exception exception)
            {
                RenderContiMenu([]);

                if (!_showingConfiguration)
                {
                    _selectedCard = null;
                    accountsPanel.Controls.Clear();
                    accountsPanel.Controls.Add(CreateMessageLabel($"Impossibile caricare i conti. {exception.Message}"));
                }
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
            ShowConfigurationView(new RecurringEntriesView(_client));
        }

        private void CategoriesMenuItemClick(object? sender, EventArgs e)
        {
            ShowConfigurationView(new CategoriesView(_client));
        }

        private void AccountParametersMenuItemClick(object? sender, EventArgs e)
        {
            ShowConfigurationView(new AccountParametersView(_client));
        }

        private void ShowConfigurationView(Control view)
        {
            _showingConfiguration = true;
            accountsPanel.SuspendLayout();
            accountsPanel.Controls.Clear();
            accountsPanel.FlowDirection = FlowDirection.LeftToRight;
            accountsPanel.WrapContents = false;
            view.Margin = Padding.Empty;
            view.Size = new Size(
                accountsPanel.ClientSize.Width - accountsPanel.Padding.Horizontal,
                accountsPanel.ClientSize.Height - accountsPanel.Padding.Vertical);
            accountsPanel.Controls.Add(view);
            accountsPanel.ResumeLayout();
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
                contoMenuItem.DropDownItems.Add("&Movimenti", null, async (_, _) => await OpenTimeline(conto));
                contiMenuItem.DropDownItems.Add(contoMenuItem);
            }
        }

        private Control CreateContoCard(Conto conto, bool highlighted)
        {
            bool isCard = conto.CycleIndicators is not null;
            var card = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(12),
                Padding = new Padding(18),
                Size = isCard
                    ? highlighted ? new Size(460, 230) : new Size(330, 205)
                    : highlighted ? new Size(430, 140) : new Size(280, 105),
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
            Control content = isCard
                ? CreateCycleIndicators(conto.CycleIndicators!, highlighted)
                : CreateBalance(conto, highlighted);

            card.Controls.Add(content);
            card.Controls.Add(nameLabel);
            card.Tag = conto;
            card.Cursor = Cursors.Hand;
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Apri movimenti", null, async (_, _) => await OpenTimeline(conto));
            card.ContextMenuStrip = contextMenu;
            AttachCardEvents(card, conto, card);

            return card;
        }

        private static Control CreateBalance(Conto conto, bool highlighted)
        {
            var balanceLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", highlighted ? 24F : 17F, FontStyle.Regular),
                ForeColor = conto.Balance < 0 ? Color.Firebrick : SystemColors.ControlText,
                Text = conto.Balance.ToString("N2", ItalianCulture) + " €",
                TextAlign = ContentAlignment.MiddleLeft,
            };
            var balancePanel = new Panel { Dock = DockStyle.Fill };

            balancePanel.Controls.Add(balanceLabel);

            if (conto.FirstNegativeBalanceDate is DateOnly firstNegativeBalanceDate && conto.FirstNegativeBalance is decimal firstNegativeBalance)
            {
                var forecastLabel = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Bottom,
                    Font = new Font("Segoe UI", highlighted ? 10F : 8.5F, FontStyle.Regular),
                    ForeColor = Color.Firebrick,
                    Height = highlighted ? 24 : 20,
                    Text = $"{firstNegativeBalanceDate:dd/MM/yyyy}  {FormatCurrency(firstNegativeBalance)}",
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                balancePanel.Controls.Add(forecastLabel);
            }

            return balancePanel;
        }

        private static Control CreateCycleIndicators(CycleIndicators indicators, bool highlighted)
        {
            var panel = new FlowLayoutPanel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0, 4, 0, 0),
                WrapContents = false,
            };
            panel.Controls.Add(CreateIndicatorLabel(
                $"Speso nel ciclo: {FormatCurrency(indicators.CurrentCycleSpent)}",
                highlighted ? 16F : 12F,
                FontStyle.Bold,
                SystemColors.ControlText));
            Color remainingColor = indicators.RemainingIncludingOverdraft < 0 ? Color.Firebrick
                : indicators.RemainingPlafond < 0 ? Color.DarkOrange : SystemColors.ControlText;
            panel.Controls.Add(CreateIndicatorLabel(
                $"Plafond residuo: {FormatCurrency(indicators.RemainingPlafond)} ({FormatCurrency(indicators.RemainingIncludingOverdraft)})",
                highlighted ? 10F : 9F,
                FontStyle.Regular,
                remainingColor));

            if (indicators.PendingDebit is decimal pendingDebit)
            {
                panel.Controls.Add(CreateIndicatorLabel($"In addebito: {FormatCurrency(pendingDebit)}", highlighted ? 10F : 9F,
                    FontStyle.Regular, SystemColors.ControlText));
            }

            if (indicators.FirstPlafondExceeded is CycleThreshold plafond)
            {
                panel.Controls.Add(CreateIndicatorLabel(
                    $"Prima eccedenza: {plafond.Date:dd/MM/yy}  {FormatCurrency(plafond.Balance)} (+{FormatCurrency(plafond.Excess)})",
                    highlighted ? 9.5F : 8.5F,
                    FontStyle.Regular,
                    Color.DarkOrange));
            }

            if (indicators.FirstTotalLimitExceeded is CycleThreshold total)
            {
                panel.Controls.Add(CreateIndicatorLabel(
                    $"Prima criticità: {total.Date:dd/MM/yy}  {FormatCurrency(total.Balance)} (+{FormatCurrency(total.Excess)})",
                    highlighted ? 9.5F : 8.5F,
                    FontStyle.Regular,
                    Color.Firebrick));
            }

            return panel;
        }

        private static Label CreateIndicatorLabel(string text, float size, FontStyle style, Color color) => new()
        {
            AutoEllipsis = true,
            AutoSize = false,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            Height = size >= 12F ? 34 : 25,
            Margin = Padding.Empty,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 410,
        };

        private void AttachCardEvents(Control control, Conto conto, Panel card)
        {
            control.Click += (_, _) => SelectCard(card);
            control.DoubleClick += async (_, _) => await OpenTimeline(conto);

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

        private async Task OpenTimeline(Conto conto)
        {
            if (conto.CycleIndicators is null)
            {
                await ShowMovimenti(conto, DateTime.Today.Month, DateTime.Today.Year);
                return;
            }

            await ShowCicli(conto, conto.CycleIndicators.To.Month, conto.CycleIndicators.To.Year);
        }

        private async Task ShowCicli(Conto conto, int month, int year)
        {
            try
            {
                _showingConfiguration = false;
                UseWaitCursor = true;
                ContoCicli timeline = await _client.GetCicli(conto.Name, month, year);
                RenderCicli(timeline);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Impossibile caricare i cicli. {exception.Message}", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task ShowMovimenti(Conto conto, int month, int year)
        {
            try
            {
                _showingConfiguration = false;
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
                MonthSummary summary = MovimentiTimelineCalculator.CalculateMonth(timeline, renderedPeriod, DateOnly.FromDateTime(DateTime.Today));
                bool selectedMonth = renderedPeriod.Month == timeline.SelectedMonth && renderedPeriod.Year == timeline.SelectedYear;
                accountsPanel.Controls.Add(CreateMonthSection(renderedPeriod, summary, selectedMonth));

                renderedPeriod = renderedPeriod.AddMonths(1);
            }

            accountsPanel.Controls.Add(CreateClosingBalanceRow(timeline));
            accountsPanel.ResumeLayout();
        }

        private void RenderCicli(ContoCicli timeline)
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
            accountsPanel.Controls.Add(CreateCyclePeriodSelector(timeline));
            accountsPanel.Controls.Add(CreateOpeningBalanceRow(timeline.From, timeline.OpeningBalance));

            foreach (Ciclo cycle in timeline.Cycles)
            {
                bool selected = cycle.To.Month == timeline.SelectedMonth && cycle.To.Year == timeline.SelectedYear;
                accountsPanel.Controls.Add(CreateCycleSection(cycle, selected));
            }

            accountsPanel.Controls.Add(CreateClosingBalanceRow(timeline.To, timeline.ClosingBalance));
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

        private Control CreateCyclePeriodSelector(ContoCicli timeline)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(12), WrapContents = false };
            var month = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };
            month.Items.AddRange([.. ItalianCulture.DateTimeFormat.MonthNames.Take(12).Select(value => ItalianCulture.TextInfo.ToTitleCase(value))]);
            month.SelectedIndex = timeline.SelectedMonth - 1;
            var year = new NumericUpDown { Maximum = 9999, Minimum = 1, Value = timeline.SelectedYear, Width = 80 };
            var previous = new Button { AutoSize = true, Text = "← Ciclo precedente" };
            var next = new Button { AutoSize = true, Text = "Ciclo successivo →" };
            var show = new Button { AutoSize = true, Text = "Visualizza" };
            DateOnly selected = new(timeline.SelectedYear, timeline.SelectedMonth, 1);

            previous.Click += async (_, _) => await ShowCicli(timeline.Conto, selected.AddMonths(-1).Month, selected.AddMonths(-1).Year);
            next.Click += async (_, _) => await ShowCicli(timeline.Conto, selected.AddMonths(1).Month, selected.AddMonths(1).Year);
            show.Click += async (_, _) => await ShowCicli(timeline.Conto, month.SelectedIndex + 1, decimal.ToInt32(year.Value));
            panel.Controls.AddRange([previous, month, year, show, next]);

            return panel;
        }

        #endregion

        #region Controlli grafici

        private static Control CreateCycleSection(Ciclo cycle, bool selected)
        {
            string headerText = $"{cycle.From:dd/MM/yy} – {cycle.To:dd/MM/yy}    Speso: {FormatSignedCurrency(cycle.Total)}";
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
                Text = $"{(selected ? "▼" : "▶")}  {headerText}",
                TextAlign = ContentAlignment.MiddleLeft,
                UseVisualStyleBackColor = false,
            };
            header.FlatAppearance.BorderSize = 0;
            var content = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(0, 3, 0, 0),
                Visible = selected,
                WrapContents = false,
            };

            foreach (MovimentoCiclo movement in cycle.Items)
            {
                content.Controls.Add(CreateCycleMovementRow(movement));
            }

            if (cycle.Items.Count == 0)
            {
                content.Controls.Add(CreateEmptyCycleLabel());
            }

            header.Click += (_, _) =>
            {
                content.Visible = !content.Visible;
                header.Text = $"{(content.Visible ? "▼" : "▶")}  {headerText}";
            };
            section.Controls.Add(header);
            section.Controls.Add(content);

            return section;
        }

        private static Panel CreateCycleMovementRow(MovimentoCiclo movimento)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            Color background = movimento.Date == today ? TodayMovementBackground
                : movimento.Date < today ? PastMovementBackground : Color.White;
            var row = new Panel { BackColor = background, Margin = new Padding(0, 1, 0, 1), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Location = new Point(10, 11), Size = new Size(80, 22), Text = movimento.Date.ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoEllipsis = true, AutoSize = false, Location = new Point(100, 11), Size = new Size(370, 22), Text = movimento.Description });
            row.Controls.Add(new Label
            {
                AutoSize = false,
                ForeColor = movimento.Amount < 0 ? Color.Firebrick : SystemColors.ControlText,
                Location = new Point(480, 11),
                Size = new Size(120, 22),
                Text = movimento.Amount == 0 ? "-" : FormatCurrency(movimento.Amount),
                TextAlign = ContentAlignment.TopRight,
            });
            row.Controls.Add(new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(610, 11),
                Size = new Size(120, 22),
                Text = FormatCurrency(movimento.CycleBalanceAfter),
                TextAlign = ContentAlignment.TopRight,
            });
            row.Controls.Add(new Label { AutoSize = false, Location = new Point(740, 11), Size = new Size(120, 22), Text = FormatCurrency(movimento.BalanceAfter), TextAlign = ContentAlignment.TopRight });

            return row;
        }

        private static Control CreateMonthSection(DateOnly date, MonthSummary summary, bool selectedMonth)
        {
            string title = date.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", ItalianCulture);
            string headerText = CreateMonthHeaderText(title, summary);
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
                Text = $"{(selectedMonth ? "▼" : "▶")}  {headerText}",
                TextAlign = ContentAlignment.MiddleLeft,
                UseVisualStyleBackColor = false,
            };
            header.FlatAppearance.BorderSize = 0;
            var content = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(0, 3, 0, 0),
                Visible = selectedMonth,
                WrapContents = false,
            };

            foreach (Movimento movement in summary.Movements)
            {
                content.Controls.Add(CreateMovimentoRow(movement));
            }

            if (summary.Movements.Count == 0)
            {
                content.Controls.Add(CreateEmptyMonthLabel());
            }

            header.Click += (_, _) =>
            {
                content.Visible = !content.Visible;
                header.Text = $"{(content.Visible ? "▼" : "▶")}  {headerText}";
            };
            section.Controls.Add(header);
            section.Controls.Add(content);

            return section;
        }

        private static string CreateMonthHeaderText(string title, MonthSummary summary)
        {
            string result = $"{title}    Δ mese: {FormatSignedCurrency(summary.Delta)}";
            return summary.CurrentBalance is decimal currentBalance
                ? $"{result}    Saldo attuale: {FormatCurrency(currentBalance)}"
                : result;
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

        private static Panel CreateOpeningBalanceRow(DateOnly from, decimal balance)
        {
            var row = new Panel { BackColor = Color.FromArgb(225, 230, 235), Margin = new Padding(12, 1, 12, 1), Size = new Size(880, 42) };
            row.Controls.Add(new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                Location = new Point(10, 11),
                Size = new Size(80, 22),
                Text = from.AddDays(-1).ToString("dd/MM/yy", ItalianCulture),
            });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(100, 11), Size = new Size(580, 22), Text = "Saldo precedente" });
            row.Controls.Add(new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(695, 11),
                Size = new Size(165, 22),
                Text = FormatCurrency(balance),
                TextAlign = ContentAlignment.TopRight,
            });

            return row;
        }

        private static Panel CreateClosingBalanceRow(ContoMovimenti timeline)
        {
            var row = new Panel { BackColor = Color.FromArgb(214, 226, 238), Margin = new Padding(12, 10, 12, 12), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(10, 11), Size = new Size(80, 22), Text = timeline.To.ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(100, 11), Size = new Size(580, 22), Text = "Saldo previsto" });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(695, 11), Size = new Size(165, 22), Text = FormatCurrency(timeline.ClosingBalance), TextAlign = ContentAlignment.TopRight });

            return row;
        }

        private static Panel CreateClosingBalanceRow(DateOnly to, decimal balance)
        {
            var row = new Panel { BackColor = Color.FromArgb(214, 226, 238), Margin = new Padding(12, 10, 12, 12), Size = new Size(880, 42) };
            row.Controls.Add(new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                Location = new Point(10, 11),
                Size = new Size(80, 22),
                Text = to.ToString("dd/MM/yy", ItalianCulture),
            });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(100, 11), Size = new Size(580, 22), Text = "Saldo previsto" });
            row.Controls.Add(new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(695, 11),
                Size = new Size(165, 22),
                Text = FormatCurrency(balance),
                TextAlign = ContentAlignment.TopRight,
            });

            return row;
        }

        private static string FormatCurrency(decimal value) => value.ToString("N2", ItalianCulture) + " €";

        private static string FormatSignedCurrency(decimal value) => value > 0 ? "+" + FormatCurrency(value) : FormatCurrency(value);

        private static Label CreateEmptyMonthLabel() => new()
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(22, 10, 12, 10),
            Text = "Nessun movimento nel mese",
        };

        private static Label CreateEmptyCycleLabel() => new()
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Italic),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(22, 10, 12, 10),
            Text = "Nessun movimento nel ciclo",
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

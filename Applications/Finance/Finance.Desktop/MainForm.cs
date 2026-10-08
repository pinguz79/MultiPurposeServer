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
        private Func<Task>? _refreshTimeline;
        private bool _initializing;
        private bool _movementActionBusy;
        private bool _loadingFinanziamenti;
        private bool _transferDialogOpen;

        public MainForm(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
            accountsPanel.SizeChanged += (_, _) => ResizeTimelineLayout();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await Initialize();
        }

        private async Task Initialize()
        {
            if (_initializing)
            {
                return;
            }

            _initializing = true;
            menuStrip.Enabled = false;
            UseWaitCursor = true;
            accountsPanel.Controls.Clear();
            accountsPanel.Controls.Add(CreateMessageLabel("Caricamento dei conti in corso…"));
            try
            {
                IReadOnlyList<Conto> conti = await _client.Initialize();
                RenderConti(conti);
                await RefreshFinanziamentiMenu();
                menuStrip.Enabled = true;
                await CheckPendingMovements(conti);
            }
            catch (Exception exception)
            {
                accountsPanel.Controls.Clear();
                Label errorLabel = CreateMessageLabel($"Impossibile completare l'avvio. {exception.Message}");
                errorLabel.MaximumSize = new Size(650, 0);
                accountsPanel.Controls.Add(errorLabel);
                var retryButton = new Button { AutoSize = true, Text = "&Riprova", Margin = new Padding(16) };
                retryButton.Click += async (_, _) => await Initialize();
                accountsPanel.Controls.Add(retryButton);
            }
            finally
            {
                UseWaitCursor = false;
                _initializing = false;
            }
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

        private async Task CheckPendingMovements(IReadOnlyList<Conto> accounts)
        {
            try
            {
                IReadOnlyList<MovimentoEdit> pending = await _client.GetMovimentiForReview();
                UseWaitCursor = false;
                if (pending.Count > 0 && MessageBox.Show(this, $"Ci sono {pending.Count} movimenti passati da confermare. Vuoi esaminarli ora?", "Movimenti da confermare", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    using var dialog = new MovimentiReviewDialog(_client, accounts);
                    dialog.ShowDialog(this);
                    await RefreshConti();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Controllo dei movimenti da confermare non disponibile. Puoi riprovare dal menu. {exception.Message}", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task ManageMovements(Conto? conto = null)
        {
            try
            {
                IReadOnlyList<Conto> accounts = await _client.GetConti();
                using var dialog = new MovimentiReviewDialog(_client, accounts, conto is null ? null : accounts.Single(item => item.Id == conto.Id));
                dialog.ShowDialog(this);
                await RefreshMovementView();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task NewTransfer(Conto? destination = null)
        {
            if (_transferDialogOpen)
            {
                return;
            }
            _transferDialogOpen = true;
            try
            {
                IReadOnlyList<Conto> accounts = await _client.GetConti();
                if (accounts.Count < 2)
                {
                    MessageBox.Show(this, "Servono almeno due conti per registrare un trasferimento.", "Trasferimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var dialog = new TrasferimentoDialog(_client, accounts, destination?.Id);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await RefreshMovementView();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Trasferimento", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _transferDialogOpen = false;
            }
        }

        private async Task NewMovement(Conto conto)
        {
            try
            {
                Conto current = (await _client.GetConti()).Single(item => item.Id == conto.Id);
                using var dialog = new MovimentoDialog(_client, current);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await RefreshMovementView();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshMovementView()
        {
            FlowLayoutPanel list = GetMovementList();
            Point scroll = list.AutoScrollPosition;
            Dictionary<string, bool> expanded = list.Controls.Cast<Control>()
                .Where(control => control.Tag is DateOnly)
                .ToDictionary(control => ((DateOnly)control.Tag!).ToString("yyyy-MM-dd"), control => control.Controls[0].Text.StartsWith("▼"));
            await (_refreshTimeline is null ? RefreshConti() : _refreshTimeline());
            list = GetMovementList();
            foreach (Control section in list.Controls)
            {
                if (section.Tag is DateOnly date && expanded.TryGetValue(date.ToString("yyyy-MM-dd"), out bool visible)
                    && section.Controls[0] is Button header && header.Text.StartsWith("▼") != visible)
                {
                    header.PerformClick();
                }
            }
            list.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
        }

        private FlowLayoutPanel GetMovementList() => accountsPanel.Controls.Find("movementList", true).OfType<FlowLayoutPanel>().FirstOrDefault() ?? accountsPanel;

        private void ResizeTimelineLayout()
        {
            if (accountsPanel.Controls.OfType<FinanziamentoView>().FirstOrDefault() is FinanziamentoView view)
            {
                view.Size = new Size(Math.Max(0, accountsPanel.ClientSize.Width - accountsPanel.Padding.Horizontal),
                    Math.Max(0, accountsPanel.ClientSize.Height - accountsPanel.Padding.Vertical));
            }
            if (accountsPanel.Controls["timelineLayout"] is Control layout)
            {
                layout.Size = new Size(Math.Max(0, accountsPanel.ClientSize.Width - accountsPanel.Padding.Horizontal),
                    Math.Max(0, accountsPanel.ClientSize.Height - accountsPanel.Padding.Vertical));
            }
        }

        private void CreateTimelineLayout()
        {
            Control[] controls = [.. accountsPanel.Controls.Cast<Control>()];
            accountsPanel.Controls.Clear();
            accountsPanel.AutoScroll = false;
            var layout = new TableLayoutPanel { Name = "timelineLayout", ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var toolbar = new FlowLayoutPanel
            {
                Name = "movementToolbar", AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Margin = Padding.Empty,
            };
            var list = new FlowLayoutPanel
            {
                Name = "movementList", AutoScroll = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Margin = Padding.Empty,
            };
            toolbar.Controls.AddRange(controls.Take(4).ToArray());
            list.Controls.AddRange(controls.Skip(4).ToArray());
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(list, 0, 1);
            accountsPanel.Controls.Add(layout);
            ResizeTimelineLayout();
        }

        private void AttachMovementActions(Control control, Conto conto)
        {
            (Guid Id, DateOnly Date, bool Confirmed)? target = control.Tag switch
            {
                Movimento movement => (movement.Id, movement.Date, movement.IsConfirmed),
                MovimentoCiclo movement => (movement.Id, movement.Date, movement.IsConfirmed),
                _ => null,
            };
            if (target is { } item)
            {
                var menu = new ContextMenuStrip();
                menu.Items.Add("Modifica…", null, async (_, _) => await RunMovementAction(conto, item.Id, item.Date, "edit"));
                menu.Items.Add("Elimina…", null, async (_, _) => await RunMovementAction(conto, item.Id, item.Date, "delete"));
                if (!item.Confirmed)
                {
                    menu.Items.Add("Conferma", null, async (_, _) => await RunMovementAction(conto, item.Id, item.Date, "confirm"));
                }
                control.Disposed += (_, _) => menu.Dispose();
                foreach (Control surface in control.Controls.Cast<Control>().Prepend(control))
                {
                    surface.ContextMenuStrip = menu;
                    surface.MouseDoubleClick += async (_, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            await RunMovementAction(conto, item.Id, item.Date, "edit");
                        }
                    };
                    surface.MouseClick += async (_, e) =>
                    {
                        if (e.Button == MouseButtons.Middle && !item.Confirmed)
                        {
                            await RunMovementAction(conto, item.Id, item.Date, "confirm");
                        }
                    };
                }
                return;
            }
            foreach (Control child in control.Controls)
            {
                AttachMovementActions(child, conto);
            }
        }

        private async Task RunMovementAction(Conto conto, Guid id, DateOnly date, string action)
        {
            if (_movementActionBusy)
            {
                return;
            }
            _movementActionBusy = true;
            menuStrip.Enabled = false;
            accountsPanel.Enabled = false;
            try
            {
                if (action == "confirm")
                {
                    // La conferma è validata atomicamente dal server, comprese tariffe e correlazioni.
                    // Non serve valutare preventivamente tutti i movimenti della stessa giornata.
                    await _client.ConfirmMovimenti([id]);
                    await RefreshMovementView();
                    return;
                }
                MovimentoEdit movement = (await _client.GetMovimentiForReview(conto.Name, date, date)).Single(item => item.Id == id);
                if (action == "edit")
                {
                    using Form dialog = movement.Pedaggio is null
                        ? new MovimentoDialog(_client, conto, movement)
                        : new PedaggioDialog(_client, conto, movement);
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                }
                else if (action == "delete")
                {
                    if (MessageBox.Show(this, $"Eliminare '{movement.Description}' del {movement.Date:dd/MM/yy}? La pianificazione non verrà eliminata.", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return;
                    }
                    await _client.DeleteMovimento(id);
                }
                await RefreshMovementView();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _movementActionBusy = false;
                menuStrip.Enabled = true;
                accountsPanel.Enabled = true;
            }
        }

        private async Task NewPedaggio(Conto conto)
        {
            try
            {
                using var dialog = new PedaggioDialog(_client, conto);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await RefreshMovementView();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            accountsPanel.AutoScroll = true;
            _refreshTimeline = null;
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

        private async void FinanziamentiMenuItemDropDownOpening(object? sender, EventArgs e) => await RefreshFinanziamentiMenu();

        private Action? _pendingFinanziamentiMenuUpdate;

        private void FinanziamentiMenuItemDropDownClosed(object? sender, EventArgs e)
        {
            Action? update = _pendingFinanziamentiMenuUpdate;
            _pendingFinanziamentiMenuUpdate = null;
            update?.Invoke();
        }

        private void UpdateFinanziamentiMenu(Action update)
        {
            if (finanziamentiMenuItem.DropDown.Visible)
            {
                _pendingFinanziamentiMenuUpdate = update;
                return;
            }
            update();
        }

        private async Task RefreshFinanziamentiMenu()
        {
            if (_loadingFinanziamenti)
            {
                return;
            }
            _loadingFinanziamenti = true;
            try
            {
                IReadOnlyList<Finanziamento> loans = await _client.GetFinanziamenti();
                if (IsDisposed)
                {
                    return;
                }
                UpdateFinanziamentiMenu(() =>
                {
                    finanziamentiMenuItem.DropDownItems.Clear();
                    foreach (Finanziamento loan in loans.Where(item => !item.IsClosed))
                    {
                        finanziamentiMenuItem.DropDownItems.Add(loan.DisplayName, null, (_, _) =>
                        {
                            accountsPanel.AutoScroll = false;
                            ShowConfigurationView(new FinanziamentoView(_client, loan.Name));
                        });
                    }
                    if (finanziamentiMenuItem.DropDownItems.Count == 0)
                    {
                        finanziamentiMenuItem.DropDownItems.Add(new ToolStripMenuItem("Nessun finanziamento aperto") { Enabled = false });
                    }
                });
            }
            catch (Exception exception)
            {
                if (!IsDisposed)
                {
                    UpdateFinanziamentiMenu(() =>
                    {
                        finanziamentiMenuItem.DropDownItems.Clear();
                        finanziamentiMenuItem.DropDownItems.Add(new ToolStripMenuItem("Caricamento non riuscito: riapri il menu") { Enabled = false, ToolTipText = exception.Message });
                    });
                }
            }
            finally
            {
                _loadingFinanziamenti = false;
            }
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
            _refreshTimeline = null;
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
            while (contiMenuItem.DropDownItems.Count > 1)
            {
                contiMenuItem.DropDownItems.RemoveAt(1);
            }

            contiMenuSeparator.Visible = conti.Count > 0;
            contiMenuItem.DropDownItems.Add("Da &confermare…", null, async (_, _) => await ManageMovements());
            contiMenuItem.DropDownItems.Add("&Trasferimento…", null, async (_, _) => await NewTransfer());
            contiMenuItem.DropDownItems.Add(contiMenuSeparator);
            foreach (Conto conto in conti)
            {
                var contoMenuItem = new ToolStripMenuItem(conto.DisplayName.Replace("&", "&&"));
                contoMenuItem.DropDownItems.Add("&Movimenti", null, async (_, _) => await OpenTimeline(conto));
                contoMenuItem.DropDownItems.Add("&Nuovo movimento…", null, async (_, _) => await NewMovement(conto));
                if (conto.AbilitaPedaggi)
                {
                    contoMenuItem.DropDownItems.Add("Nuovo &pedaggio…", null, async (_, _) => await NewPedaggio(conto));
                }
                contoMenuItem.DropDownItems.Add("&Gestisci movimenti…", null, async (_, _) => await ManageMovements(conto));
                contiMenuItem.DropDownItems.Add(contoMenuItem);
            }
        }

        private Control CreateContoCard(Conto conto, bool highlighted)
        {
            bool isCard = conto.CycleIndicators is not null || conto.RevolvingIndicators is not null;
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
            Control content = conto.RevolvingIndicators is RevolvingIndicators revolving ? CreateRevolvingIndicators(conto, revolving, highlighted)
                : conto.CycleIndicators is not null ? CreateCycleIndicators(conto.CycleIndicators, highlighted)
                : CreateBalance(conto, highlighted);

            card.Controls.Add(content);
            card.Controls.Add(nameLabel);
            card.Height = Math.Max(card.Height, card.Padding.Vertical + nameLabel.Height + content.MinimumSize.Height + 2);
            card.Tag = conto;
            card.Cursor = Cursors.Hand;
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Apri movimenti", null, async (_, _) => await OpenTimeline(conto));
            contextMenu.Items.Add("Trasferimento…", null, async (_, _) => await NewTransfer(conto));
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
            var balancePanel = new Panel
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(0, balanceLabel.PreferredHeight),
            };

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
                balancePanel.MinimumSize = new Size(0, balanceLabel.PreferredHeight + Math.Max(forecastLabel.Height, forecastLabel.PreferredHeight));
                forecastLabel.Height = Math.Max(forecastLabel.Height, forecastLabel.PreferredHeight);
            }

            return balancePanel;
        }

        private static Control CreateRevolvingIndicators(Conto conto, RevolvingIndicators indicators, bool highlighted)
        {
            Color color = RevolvingCardPresentation.GetBalanceColor(indicators);
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
            };
            panel.Controls.Add(CreateIndicatorLabel($"Debito attuale: {FormatCurrency(conto.Balance)}", highlighted ? 16F : 12F, FontStyle.Bold, color));
            panel.Controls.Add(CreateIndicatorLabel($"Plafond residuo: {FormatCurrency(indicators.RemainingPlafond)}", highlighted ? 10F : 9F, FontStyle.Regular, color));
            if (string.Equals(conto.Name, "AmEx", StringComparison.OrdinalIgnoreCase))
            {
                panel.Controls.Add(CreateIndicatorLabel($"Disponibile AmEx: {FormatCurrency(decimal.Floor(Math.Max(0m, indicators.RemainingPlafond)))}", highlighted ? 10F : 9F, FontStyle.Regular, color));
            }
            panel.Controls.Add(CreateIndicatorLabel($"Con scoperto: {FormatCurrency(indicators.RemainingIncludingOverdraft)}", highlighted ? 10F : 9F, FontStyle.Regular, color));

            foreach (Control control in panel.Controls)
            {
                control.Width = highlighted ? 410 : 280;
            }
            return panel;
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
            control.ContextMenuStrip = card.ContextMenuStrip;
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
            if (!conto.UsesCycleTimeline)
            {
                await ShowMovimenti(conto, DateTime.Today.Month, DateTime.Today.Year);
                return;
            }

            await ShowCicli(conto);
        }

        private async Task ShowCicli(Conto conto, int? month = null, int? year = null)
        {
            try
            {
                _showingConfiguration = false;
                UseWaitCursor = true;
                ContoCicli timeline = await _client.GetCicli(conto.Name, month, year);
                IReadOnlyList<ParametroConto> parameters = string.Equals(conto.Name, "AmEx", StringComparison.OrdinalIgnoreCase) ? await _client.GetParametriConto(conto.Name) : [];
                RenderCicli(timeline, parameters);
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
            _refreshTimeline = () => ShowMovimenti(timeline.Conto, timeline.SelectedMonth, timeline.SelectedYear);
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
            accountsPanel.Controls.Add(CreateMovementActions(timeline.Conto));
            accountsPanel.Controls.Add(CreateOpeningBalanceRow(timeline));

            DateOnly renderedPeriod = new(timeline.From.Year, timeline.From.Month, 1);
            DateOnly finalPeriod = new(timeline.To.Year, timeline.To.Month, 1);
            while (renderedPeriod <= finalPeriod)
            {
                MonthSummary summary = MovimentiTimelineCalculator.CalculateMonth(timeline, renderedPeriod, DateOnly.FromDateTime(DateTime.Today));
                bool selectedMonth = renderedPeriod.Month == timeline.SelectedMonth && renderedPeriod.Year == timeline.SelectedYear;
                Control section = CreateMonthSection(renderedPeriod, summary, selectedMonth);
                section.Tag = renderedPeriod;
                AttachMovementActions(section, timeline.Conto);
                accountsPanel.Controls.Add(section);

                renderedPeriod = renderedPeriod.AddMonths(1);
            }

            accountsPanel.Controls.Add(CreateClosingBalanceRow(timeline));
            CreateTimelineLayout();
            accountsPanel.ResumeLayout();
        }

        private void RenderCicli(ContoCicli timeline, IReadOnlyList<ParametroConto> parameters)
        {
            _refreshTimeline = () => ShowCicli(timeline.Conto, timeline.SelectedMonth, timeline.SelectedYear);
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
            accountsPanel.Controls.Add(CreateMovementActions(timeline.Conto));
            accountsPanel.Controls.Add(CreateOpeningBalanceRow(timeline.From, timeline.OpeningBalance));

            decimal openingBalance = timeline.OpeningBalance;
            bool isAmEx = string.Equals(timeline.Conto.Name, "AmEx", StringComparison.OrdinalIgnoreCase);
            ParametroConto? plafond = isAmEx ? parameters.FirstOrDefault(parameter => string.Equals(parameter.Name, "Plafond", StringComparison.OrdinalIgnoreCase)) : null;
            foreach (Ciclo cycle in timeline.Cycles)
            {
                bool selected = cycle.To.Month == timeline.SelectedMonth && cycle.To.Year == timeline.SelectedYear;
                var summary = new CycleSummary(cycle, openingBalance, DateOnly.FromDateTime(DateTime.Today), plafond);
                Control section = CreateCycleSection(cycle, selected, summary);
                section.Tag = cycle.To;
                AttachMovementActions(section, timeline.Conto);
                accountsPanel.Controls.Add(section);
                openingBalance = summary.ClosingBalance;
            }

            accountsPanel.Controls.Add(CreateClosingBalanceRow(timeline.To, timeline.ClosingBalance));
            CreateTimelineLayout();
            accountsPanel.ResumeLayout();
        }

        private Control CreateMovementActions(Conto conto)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(12), WrapContents = false };
            var create = new Button { AutoSize = true, Text = conto.AbilitaPedaggi ? "Nuovo pedaggio…" : "Nuovo movimento…" };
            var manage = new Button { AutoSize = true, Text = "Modifica / elimina movimenti…" };
            create.Click += async (_, _) =>
            {
                if (conto.AbilitaPedaggi)
                {
                    await NewPedaggio(conto);
                }
                else
                {
                    await NewMovement(conto);
                }
            };
            manage.Click += async (_, _) => await ManageMovements(conto);
            panel.Controls.Add(create);
            panel.Controls.Add(manage);
            var transfer = new Button { AutoSize = true, Text = "Trasferimento…" };
            transfer.Click += async (_, _) => await NewTransfer(conto);
            panel.Controls.Add(transfer);
            return panel;
        }

        private Control CreatePeriodSelector(ContoMovimenti timeline)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(12), WrapContents = false };
            var month = new Controls.SearchComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };
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
            var month = new Controls.SearchComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };
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

        private static Control CreateCycleSection(Ciclo cycle, bool selected, CycleSummary summary)
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

            content.Controls.Add(CreateCycleSummaryRow(cycle.From.AddDays(-1), "Saldo iniziale", summary.OpeningBalance));
            foreach (MovimentoCiclo movement in cycle.Items)
            {
                content.Controls.Add(CreateCycleMovementRow(movement));
            }

            if (cycle.Items.Count == 0)
            {
                content.Controls.Add(CreateEmptyCycleLabel());
            }

            content.Controls.Add(CreateCycleSummaryRow(summary.ReferenceDate, summary.IsOpen ? "Saldo ad oggi" : summary.IsFuture ? "Saldo previsto a chiusura" : "Saldo finale", summary.ReferenceBalance));
            if (summary.IsOpen && cycle.Items.Any(item => item.Date > summary.ReferenceDate))
            {
                content.Controls.Add(CreateCycleSummaryRow(cycle.To, "Saldo previsto a chiusura", summary.ClosingBalance));
            }
            if (summary.RemainingPlafond is decimal remaining && summary.StatementRemainingPlafond is decimal statementRemaining)
            {
                content.Controls.Add(new Label
                {
                    AutoSize = true,
                    Margin = new Padding(10, 6, 10, 6),
                    Text = $"Plafond residuo al {summary.ReferenceDate:dd/MM/yy}: {FormatCurrency(remaining)} (AmEx: {FormatCurrency(statementRemaining)})",
                });
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

        private static Panel CreateCycleSummaryRow(DateOnly date, string caption, decimal balance)
        {
            Panel row = CreateClosingBalanceRow(date, balance);
            row.Margin = new Padding(0, 1, 0, 1);
            row.Controls[1].Text = caption;
            return row;
        }

        private static Panel CreateCycleMovementRow(MovimentoCiclo movimento)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            Color background = movimento.Date == today ? TodayMovementBackground
                : movimento.Date < today ? PastMovementBackground : Color.White;
            var row = new Panel { BackColor = background, Margin = new Padding(0, 1, 0, 1), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Location = new Point(10, 11), Size = new Size(80, 22), Text = movimento.Date.ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoEllipsis = true, AutoSize = false, Location = new Point(100, 11), Size = new Size(370, 22), Text = movimento.IsConfirmed ? movimento.Description : $"[Da confermare] {movimento.Description}" });
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
            HighlightNegativeBalance(row.Controls[3], movimento.CycleBalanceAfter);
            HighlightNegativeBalance(row.Controls[4], movimento.BalanceAfter);
            row.Tag = movimento;

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
            row.Controls.Add(new Label { AutoEllipsis = true, AutoSize = false, Location = new Point(100, 11), Size = new Size(430, 22), Text = movimento.IsConfirmed ? movimento.Description : $"[Da confermare] {movimento.Description}" });
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
            HighlightNegativeBalance(row.Controls[3], movimento.BalanceAfter);
            row.Tag = movimento;

            return row;
        }

        private static Panel CreateOpeningBalanceRow(ContoMovimenti timeline)
        {
            var row = new Panel { BackColor = Color.FromArgb(225, 230, 235), Margin = new Padding(12, 1, 12, 1), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(10, 11), Size = new Size(80, 22), Text = timeline.From.AddDays(-1).ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(100, 11), Size = new Size(580, 22), Text = "Saldo precedente" });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(695, 11), Size = new Size(165, 22), Text = timeline.OpeningBalance.ToString("N2", ItalianCulture) + " €", TextAlign = ContentAlignment.TopRight });
            HighlightNegativeBalance(row.Controls[2], timeline.OpeningBalance);

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
            HighlightNegativeBalance(row.Controls[2], balance);

            return row;
        }

        private static Panel CreateClosingBalanceRow(ContoMovimenti timeline)
        {
            var row = new Panel { BackColor = Color.FromArgb(214, 226, 238), Margin = new Padding(12, 10, 12, 12), Size = new Size(880, 42) };
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(10, 11), Size = new Size(80, 22), Text = timeline.To.ToString("dd/MM/yy", ItalianCulture) });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Italic), Location = new Point(100, 11), Size = new Size(580, 22), Text = "Saldo previsto" });
            row.Controls.Add(new Label { AutoSize = false, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Location = new Point(695, 11), Size = new Size(165, 22), Text = FormatCurrency(timeline.ClosingBalance), TextAlign = ContentAlignment.TopRight });
            HighlightNegativeBalance(row.Controls[2], timeline.ClosingBalance);

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
            HighlightNegativeBalance(row.Controls[2], balance);

            return row;
        }

        private static void HighlightNegativeBalance(Control cell, decimal balance)
        {
            if (balance < 0)
            {
                cell.ForeColor = Color.DarkRed;
                cell.BackColor = Color.FromArgb(255, 225, 225);
                if (!cell.Font.Bold)
                {
                    cell.Font = new Font(cell.Font, FontStyle.Bold);
                }
            }
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

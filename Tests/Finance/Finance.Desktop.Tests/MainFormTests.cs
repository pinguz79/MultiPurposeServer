using System.Net;
using System.Reflection;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests
{
    public class MainFormTests
    {
        [Theory]
        [InlineData(false, "Nuovo movimento…")]
        [InlineData(true, "Nuovo pedaggio…")]
        public Task MovementToolbarUsesTollAccountCapability(bool tolls, string caption) => WinFormsTest.Run(() =>
        {
            using var http = new HttpClient();
            using var form = new MainForm(new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/" }));
            var conto = new Conto(Guid.NewGuid(), "Qualsiasi", "Qualsiasi", 0m, AbilitaPedaggi: tolls);
            using var actions = (Control)typeof(MainForm).GetMethod("CreateMovementActions", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [conto])!;
            actions.Controls[0].Text.Should().Be(caption);
            actions.Controls[2].Text.Should().Be("Trasferimento…");
            return Task.CompletedTask;
        });

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public Task MovementMenu_TargetsOnlyRealRows(bool cycles, bool confirmed) => WinFormsTest.Run(() =>
        {
            using var http = new HttpClient();
            using var form = new MainForm(new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/" }));
            var conto = new Conto(Guid.NewGuid(), "Conto", "Conto", 0m);
            DateOnly date = new(2026, 9, 30);
            object movement = cycles
                ? new MovimentoCiclo(Guid.NewGuid(), date, "Test", 1m, 1m, 1m, confirmed)
                : new Movimento(Guid.NewGuid(), date, "Test", 1m, 1m, confirmed);
            MethodInfo create = typeof(MainForm).GetMethod(cycles ? "CreateCycleMovementRow" : "CreateMovimentoRow", BindingFlags.Static | BindingFlags.NonPublic)!;
            using var container = new Panel();
            var row = (Panel)create.Invoke(null, [movement])!;
            var summary = new Label { Text = "Saldo" };
            container.Controls.Add(row);
            container.Controls.Add(summary);

            typeof(MainForm).GetMethod("AttachMovementActions", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [container, conto]);

            row.ContextMenuStrip.Should().NotBeNull();
            row.ContextMenuStrip!.Items.Cast<ToolStripItem>().Select(item => item.Text)
                .Should().Equal(confirmed ? ["Modifica…", "Elimina…"] : new[] { "Modifica…", "Elimina…", "Conferma" });
            foreach (Control cell in row.Controls)
            {
                cell.ContextMenuStrip.Should().BeSameAs(row.ContextMenuStrip);
            }
            summary.ContextMenuStrip.Should().BeNull();
            container.ContextMenuStrip.Should().BeNull();
            return Task.CompletedTask;
        });

        [Fact]
        public Task AccountCard_WhenCreated_ExposesTransferOnContentToo() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var http = new HttpClient();
            using var form = new MainForm(new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/" }));
            var conto = new Conto(Guid.NewGuid(), "Bank", "Banca", 0);

            // Act
            using var card = (Control)typeof(MainForm).GetMethod("CreateContoCard", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [conto, false])!;

            // Assert
            card.ContextMenuStrip!.Items.Cast<ToolStripItem>().Select(item => item.Text).Should().Contain("Trasferimento…");
            foreach (Control child in card.Controls)
            {
                child.ContextMenuStrip.Should().BeSameAs(card.ContextMenuStrip);
            }
            return Task.CompletedTask;
        });

        [Theory]
        [InlineData(-10)]
        [InlineData(0)]
        [InlineData(10)]
        public void CreateMovimentoRow_HighlightsOnlyNegativeBalance(decimal balance)
        {
            // Arrange
            var movement = new Movimento(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today).AddDays(1), "Spesa", -20m, balance);
            MethodInfo method = typeof(MainForm).GetMethod("CreateMovimentoRow", BindingFlags.Static | BindingFlags.NonPublic)!;

            // Act
            using var row = (Panel)method.Invoke(null, [movement])!;

            // Assert
            Control cell = row.Controls[3];
            row.BackColor.Should().Be(Color.White);
            cell.Font.Bold.Should().BeTrue();
            cell.ForeColor.Should().Be(balance < 0 ? Color.DarkRed : SystemColors.ControlText);
            cell.BackColor.Should().Be(balance < 0 ? Color.FromArgb(255, 225, 225) : Color.White);
            row.Controls[2].ForeColor.Should().Be(Color.Firebrick);
        }

        [Theory]
        [InlineData("CreateOpeningBalanceRow")]
        [InlineData("CreateClosingBalanceRow")]
        public void CreateSummaryRow_WhenNegative_HighlightsBalance(string name)
        {
            // Arrange
            MethodInfo method = typeof(MainForm).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic, [typeof(DateOnly), typeof(decimal)])!;

            // Act
            using var row = (Panel)method.Invoke(null, [new DateOnly(2026, 10, 1), -100m])!;

            // Assert
            row.Controls[2].ForeColor.Should().Be(Color.DarkRed);
            row.Controls[2].BackColor.Should().Be(Color.FromArgb(255, 225, 225));
            row.Controls[2].Font.Bold.Should().BeTrue();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task RefreshMovementView_WhenDetailIsOpen_ReloadsSameAccountAndPeriod(bool cycles) => WinFormsTest.Run(async () =>
        {
            // Arrange
            var conto = new Conto(Guid.NewGuid(), "Conto", "Conto", 0m);
            var from = new DateOnly(2026, 8, 1);
            var to = new DateOnly(2026, 8, 31);
            var monthly = new ContoMovimenti(conto, 8, 2026, from, to, 0m, 0m, []);
            var cyclic = new ContoCicli(conto, 8, 2026, from, to, 0m, 0m, []);
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = cycles ? JsonSerializer.Serialize(cyclic) : JsonSerializer.Serialize(monthly) };
            using var http = new HttpClient(handler);
            using var form = new MainForm(new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" }));
            MethodInfo render = typeof(MainForm).GetMethod(cycles ? "RenderCicli" : "RenderMovimenti", BindingFlags.Instance | BindingFlags.NonPublic)!;
            render.Invoke(form, cycles ? [cyclic, Array.Empty<ParametroConto>()] : [monthly]);
            var toolbar = (FlowLayoutPanel)form.Controls.Find("movementToolbar", true).Single();
            var list = (FlowLayoutPanel)form.Controls.Find("movementList", true).Single();
            toolbar.Controls.Count.Should().Be(4);
            toolbar.AutoScroll.Should().BeFalse();
            list.AutoScroll.Should().BeTrue();
            list.Controls.Cast<Control>().Should().NotBeEmpty();
            toolbar.Parent.Should().BeSameAs(list.Parent);
            list.Controls.Cast<Control>().Should().NotContain(control => control is Button);
            MethodInfo refresh = typeof(MainForm).GetMethod("RefreshMovementView", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            await (Task)refresh.Invoke(form, null)!;

            // Assert
            handler.Requests.Should().ContainSingle();
            handler.Request!.RequestUri!.AbsolutePath.Should().Contain("/Conto/Conto/");
            handler.Request.RequestUri.Query.Should().Be("?month=8&year=2026");
        });

        [Fact]
        public void RenderContiMenu_WhenRefreshed_KeepsActionsBeforeSeparator()
        {
            // Arrange
            using var http = new HttpClient();
            using var form = new MainForm(new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/" }));
            var accounts = new[] { new Conto(Guid.NewGuid(), "HelloBank", "Hello Bank", 0m) };
            MethodInfo render = typeof(MainForm).GetMethod("RenderContiMenu", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Act
            render.Invoke(form, [accounts]);
            render.Invoke(form, [accounts]);

            // Assert
            var menu = (ToolStripMenuItem)typeof(MainForm).GetField("contiMenuItem", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            menu.DropDownItems.Count.Should().Be(5);
            menu.DropDownItems[1].Text.Should().Be("Da &confermare…");
            menu.DropDownItems[2].Text.Should().Be("&Trasferimento…");
            menu.DropDownItems[3].Should().BeOfType<ToolStripSeparator>();
            menu.DropDownItems[4].Text.Should().Be("Hello Bank");
        }

        [Theory]
        [InlineData("AmEx", 521.37, "Disponibile AmEx: 521,00 €")]
        [InlineData("amex", -1.73, "Disponibile AmEx: 0,00 €")]
        [InlineData("AltraCarta", 521.37, null)]
        public void CreateRevolvingIndicators_WhenRenderingCard_ShowsStatementAvailabilityOnlyForAmEx(string name, decimal remaining, string? expected)
        {
            // Arrange
            var indicators = new RevolvingIndicators(1600m, 160m, remaining, remaining + 160m);
            var conto = new Conto(Guid.NewGuid(), name, name, 1600m - remaining, RevolvingIndicators: indicators);
            MethodInfo method = typeof(MainForm).GetMethod("CreateRevolvingIndicators", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            using var panel = (Control)(method.Invoke(null, [conto, indicators, false]) ?? throw new InvalidOperationException());

            // Assert
            string[] availability = [.. panel.Controls.Cast<Control>().Select(control => control.Text).Where(text => text.StartsWith("Disponibile AmEx:", StringComparison.Ordinal))];
            availability.Should().BeEquivalentTo(expected is null ? Array.Empty<string>() : [expected]);
            panel.Controls.Count.Should().Be(expected is null ? 3 : 4);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateContoCard_WhenNegativeForecastPresent_ShowsFullBalanceAboveForecast(bool highlighted)
        {
            // Arrange
            using var httpClient = new HttpClient();
            var client = new FinanceApiClient(httpClient, new ApiConfiguration { BaseUrl = "https://localhost/" });
            using var form = new MainForm(client);
            var conto = new Conto(Guid.NewGuid(), "Conto", "Conto", 2.06m, new DateOnly(2026, 9, 25), -37.24m);
            MethodInfo method = typeof(MainForm).GetMethod("CreateContoCard", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();

            // Act
            using var card = (Control)(method.Invoke(form, [conto, highlighted]) ?? throw new InvalidOperationException());
            card.PerformLayout();
            Control content = card.Controls[0];
            content.PerformLayout();

            // Assert
            var balance = (Label)content.Controls[0];
            var forecast = (Label)content.Controls[1];
            balance.Height.Should().BeGreaterThanOrEqualTo(balance.PreferredHeight);
            forecast.Height.Should().BeGreaterThanOrEqualTo(forecast.PreferredHeight);
            balance.Bottom.Should().BeLessThanOrEqualTo(forecast.Top);
        }
    }
}

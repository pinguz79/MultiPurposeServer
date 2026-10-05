using System.Net;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class MovimentoDialogTests
    {
        [Fact]
        public Task AmountFocusSelectsEntireText() => WinFormsTest.Run(async () =>
        {
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = "[]" };
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new MovimentoDialog(client, new Conto(Guid.NewGuid(), "Conto", "Conto", 0m));
            dialog.Show();
            await Task.Delay(50);
            var amount = (NumericUpDown)dialog.Controls.Find("amountInput", true).Single();
            amount.Value = 123.45m;
            amount.Focus();
            await Task.Delay(50);
            var editor = amount.Controls.OfType<TextBox>().Single();
            editor.SelectionStart.Should().Be(0);
            editor.SelectionLength.Should().Be(editor.Text.Length);
            dialog.Close();
        });

        [Theory]
        [InlineData(false, "-20.00")]
        [InlineData(true, "20.00")]
        public Task Save_WhenNewExpense_UsesAccountSign(bool card, string formula) => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.Created, ResponseContent = "{}" };
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            var conto = new Conto(Guid.NewGuid(), "Conto", "Conto", 0m, CycleIndicators: card ? new CycleIndicators(default, default, 0, 1000, 0, 0, 1000, 1000, null, null, null) : null);
            using var dialog = new MovimentoDialog(client, conto);
            ((TextBox)dialog.Controls.Find("descriptionInput", true).Single()).Text = "Spesa";
            ((NumericUpDown)dialog.Controls.Find("amountInput", true).Single()).Value = 20m;

            // Act
            await dialog.Save();

            // Assert
            dialog.DialogResult.Should().Be(DialogResult.OK);
            using var payload = JsonDocument.Parse(handler.RequestContent!);
            payload.RootElement.GetProperty("formula").GetString().Should().Be(formula);
            payload.RootElement.GetProperty("isConfirmed").GetBoolean().Should().BeTrue();
        });

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Save_WhenMovingPendingOccurrence_PreservesFormulaAndPendingState(bool changeAccount) => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = "{}" };
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            var conto = new Conto(Guid.NewGuid(), "Conto", "Conto", 0m);
            var movement = new MovimentoEdit(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today).AddDays(-1), "Spesa", "-[Gas]", "Conto", false, null, -31.21m, null);
            using var dialog = new MovimentoDialog(client, conto, movement);
            if (changeAccount)
            {
                var combo = (ComboBox)dialog.Controls.Find("contoInput", true).Single();
                combo.Items.Add(new Conto(Guid.NewGuid(), "Altro", "Altro conto", 0m));
                combo.SelectedIndex = 1;
            }
            ((DateTimePicker)dialog.Controls.Find("dateInput", true).Single()).Value = DateTime.Today.AddDays(7);

            // Act
            await dialog.Save();

            // Assert
            using var payload = JsonDocument.Parse(handler.RequestContent!);
            payload.RootElement.GetProperty("formula").GetString().Should().Be("-[Gas]");
            payload.RootElement.GetProperty("isConfirmed").GetBoolean().Should().BeFalse();
            payload.RootElement.GetProperty("contoName").GetString().Should().Be(changeAccount ? "Altro" : "Conto");
            handler.Request!.Method.Should().Be(HttpMethod.Patch);
        });
    }
}

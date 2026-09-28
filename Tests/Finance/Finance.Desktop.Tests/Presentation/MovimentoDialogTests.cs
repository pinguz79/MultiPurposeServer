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

        [Fact]
        public Task Save_WhenMovingPendingOccurrence_PreservesFormulaAndPendingState() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = "{}" };
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            var conto = new Conto(Guid.NewGuid(), "Conto", "Conto", 0m);
            var movement = new MovimentoEdit(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today).AddDays(-1), "Spesa", "-[Gas]", "Conto", false, null, -31.21m, null);
            using var dialog = new MovimentoDialog(client, conto, movement);
            ((DateTimePicker)dialog.Controls.Find("dateInput", true).Single()).Value = DateTime.Today.AddDays(7);

            // Act
            await dialog.Save();

            // Assert
            using var payload = JsonDocument.Parse(handler.RequestContent!);
            payload.RootElement.GetProperty("formula").GetString().Should().Be("-[Gas]");
            payload.RootElement.GetProperty("isConfirmed").GetBoolean().Should().BeFalse();
            handler.Request!.Method.Should().Be(HttpMethod.Patch);
        });
    }
}

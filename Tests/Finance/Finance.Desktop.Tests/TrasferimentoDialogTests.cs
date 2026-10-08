using System.Net;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests
{
    public class TrasferimentoDialogTests
    {
        private static readonly Conto Bank = new(Guid.NewGuid(), "Bank", "Banca", 0);
        private static readonly Conto Card = new(Guid.NewGuid(), "Card", "Carta", 0);

        private static T Input<T>(Form form, string name) where T : Control => (T)form.Controls.Find(name, true).Single();

        private static FinanceApiClient Client(RecordingHttpMessageHandler handler) => new(new HttpClient(handler), new ApiConfiguration { BaseUrl = "https://localhost/" });

        private static string Parameters(bool card) => JsonSerializer.Serialize(card
            ? new[] { "Plafond", "PercentualeScoperto", "Rata" }.Select(name => new ParametroConto(name, name, TipoParametroConto.Importo, 0, null, null,
                [new ParametroContoDefinition(null, name, 0, new DateOnly(2026, 1, 1), null, 0)])).ToArray()
            : []);

        [Theory]
        [InlineData(false, false, "-20,00", "+20,00")]
        [InlineData(false, true, "-20,00", "-20,00")]
        [InlineData(true, false, "+20,00", "+20,00")]
        [InlineData(true, true, "+20,00", "-20,00")]
        public Task Preview_WhenAccountTypesChange_UsesExpectedSigns(bool originCard, bool destinationCard, string originAmount, string destinationAmount) => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, Parameters(originCard)));
            handler.Responses.Enqueue((HttpStatusCode.OK, Parameters(destinationCard)));
            using var dialog = new TrasferimentoDialog(Client(handler), [Bank, Card], Card.Id);
            await dialog.LoadParameters();
            Input<ComboBox>(dialog, "originInput").SelectedItem = Bank;

            // Act
            Input<DateTimePicker>(dialog, "dateInput").Value = new DateTime(2026, 10, 8);
            Input<NumericUpDown>(dialog, "amountInput").Value = 20;

            // Assert
            Input<Label>(dialog, "previewLabel").Text.Should().Contain($"Banca: {originAmount} €").And.Contain($"Carta: {destinationAmount} €");
            Input<DateTimePicker>(dialog, "dateInput").Value = new DateTime(2025, 12, 31);
            Input<Label>(dialog, "previewLabel").Text.Should().Contain("Banca: -20,00 €").And.Contain("Carta: +20,00 €");
        });

        [Fact]
        public Task Save_WhenAlreadyPending_DoesNotSendTwice() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseContent = "[]" };
            using var dialog = new TrasferimentoDialog(Client(handler), [Bank, Card], Bank.Id);
            await dialog.LoadParameters();
            Input<ComboBox>(dialog, "originInput").SelectedItem = Card;
            Input<NumericUpDown>(dialog, "amountInput").Value = 20;
            Input<TextBox>(dialog, "descriptionInput").Text = "Test";
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            handler.BeforeResponse = _ => release.Task;

            // Act
            Task first = dialog.Save();
            await dialog.Save();
            release.SetResult();
            await first;

            // Assert
            handler.Requests.Count(request => request.Method == HttpMethod.Post).Should().Be(1);
        });

        [Fact]
        public Task Defaults_WhenDestinationProvided_LeavesOtherFieldsUncommitted() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseContent = "[]" };
            using var dialog = new TrasferimentoDialog(Client(handler), [Bank, Card], Bank.Id);

            // Act
            await dialog.LoadParameters();

            // Assert
            Input<ComboBox>(dialog, "destinationInput").SelectedItem.Should().Be(Bank);
            Input<ComboBox>(dialog, "originInput").SelectedIndex.Should().Be(-1);
            Input<NumericUpDown>(dialog, "amountInput").Value.Should().Be(0);
            Input<CheckBox>(dialog, "confirmedCheck").Checked.Should().BeFalse();
            Input<Button>(dialog, "saveButton").Enabled.Should().BeFalse();
        });

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Save_WhenValid_PostsSingleAtomicTransfer(bool confirmed) => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseContent = "[]" };
            using var dialog = new TrasferimentoDialog(Client(handler), [Bank, Card], Bank.Id);
            await dialog.LoadParameters();
            Input<ComboBox>(dialog, "originInput").SelectedItem = Card;
            Input<NumericUpDown>(dialog, "amountInput").Value = 900.25m;
            Input<TextBox>(dialog, "descriptionInput").Text = "Copertura prestiti";
            Input<CheckBox>(dialog, "confirmedCheck").Checked = confirmed;

            // Act
            await dialog.Save();

            // Assert
            handler.Requests.Count(request => request.Method == HttpMethod.Post).Should().Be(1);
            handler.Request!.RequestUri!.AbsolutePath.Should().Be("/Finance/BackEnd/Trasferimento");
            using var json = JsonDocument.Parse(handler.RequestContent!);
            json.RootElement.GetProperty("origineName").GetString().Should().Be("Card");
            json.RootElement.GetProperty("destinazioneName").GetString().Should().Be("Bank");
            json.RootElement.GetProperty("amount").GetDecimal().Should().Be(900.25m);
            json.RootElement.GetProperty("isConfirmed").GetBoolean().Should().Be(confirmed);
            dialog.DialogResult.Should().Be(DialogResult.OK);
        });

        [Fact]
        public Task Save_WhenAccountsCoincide_DoesNotSendRequest() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseContent = "[]" };
            using var dialog = new TrasferimentoDialog(Client(handler), [Bank, Card], Bank.Id);
            await dialog.LoadParameters();
            Input<ComboBox>(dialog, "originInput").SelectedItem = Bank;
            Input<NumericUpDown>(dialog, "amountInput").Value = 20;
            Input<TextBox>(dialog, "descriptionInput").Text = "Test";

            // Act
            await dialog.Save();

            // Assert
            handler.Requests.Should().OnlyContain(request => request.Method == HttpMethod.Get);
        });

        [Fact]
        public Task Save_WhenServerFails_PreventsBlindRetry() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseContent = "[]" };
            using var dialog = new TrasferimentoDialog(Client(handler), [Bank, Card], Bank.Id);
            await dialog.LoadParameters();
            Input<ComboBox>(dialog, "originInput").SelectedItem = Card;
            Input<NumericUpDown>(dialog, "amountInput").Value = 20;
            Input<TextBox>(dialog, "descriptionInput").Text = "Test";
            handler.ResponseStatusCode = HttpStatusCode.ServiceUnavailable;
            handler.ResponseContent = "<html>Unavailable</html>";

            // Act
            await dialog.Save();
            await dialog.Save();

            // Assert
            handler.Requests.Count(request => request.Method == HttpMethod.Post).Should().Be(1);
            Input<Button>(dialog, "saveButton").Enabled.Should().BeFalse();
            Input<Label>(dialog, "errorLabel").Text.Should().Contain("Verifica i movimenti").And.NotContain("<html>");
        });
    }
}

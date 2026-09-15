using System.Net;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class CartaRevolvingDialogTests
    {
        [Fact]
        public Task Save_WhenDefaultsAreConfirmed_SendsAmexValuesAndExcludesOwnAccount() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseContent = "{\"contoName\":\"AmEx\",\"created\":true}" };
            using var httpClient = new HttpClient(handler);
            using CartaRevolvingDialog dialog = CreateDialog(httpClient);
            var combo = (ComboBox)dialog.Controls.Find("contoAddebitoComboBox", true).Single();

            // Act
            await dialog.Save();

            // Assert
            combo.Items.Cast<Conto>().Should().ContainSingle().Which.Name.Should().Be("HelloBank");
            dialog.DialogResult.Should().Be(DialogResult.OK);
            using var payload = JsonDocument.Parse(handler.RequestContent!);
            payload.RootElement.GetProperty("plafond").GetDecimal().Should().Be(1600m);
            payload.RootElement.GetProperty("percentualeScoperto").GetDecimal().Should().Be(0.1m);
            payload.RootElement.GetProperty("quotaRata").GetDecimal().Should().Be(0.1m);
            payload.RootElement.GetProperty("tan").GetDecimal().Should().Be(0.12m);
            payload.RootElement.GetProperty("rataMinima").GetDecimal().Should().Be(72.32m);
            payload.RootElement.GetProperty("bollo").GetDecimal().Should().Be(2m);
            payload.RootElement.GetProperty("sogliaBollo").GetDecimal().Should().Be(70m);
            payload.RootElement.GetProperty("chiusuraCiclo").GetInt32().Should().Be(6);
            payload.RootElement.GetProperty("addebito").GetInt32().Should().Be(19);
        });

        [Fact]
        public Task Save_WhenServerRejects_KeepsInputAndAllowsRetry() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.Conflict, ResponseContent = "{\"detail\":\"Profilo incompatibile.\"}" };
            using var httpClient = new HttpClient(handler);
            using CartaRevolvingDialog dialog = CreateDialog(httpClient);
            var input = (NumericUpDown)dialog.Controls.Find("rataMinimaInput", true).Single();
            input.Value = 80m;

            // Act
            await dialog.Save();

            // Assert
            dialog.DialogResult.Should().Be(DialogResult.None);
            input.Value.Should().Be(80m);
            dialog.Controls.Find("errorLabel", true).Single().Text.Should().Be("Profilo incompatibile.");
            dialog.Controls.Find("saveButton", true).Single().Enabled.Should().BeTrue();
        });

        [Fact]
        public Task Save_WhenPlafondIsZero_DoesNotSendRequest() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            using var httpClient = new HttpClient(handler);
            using CartaRevolvingDialog dialog = CreateDialog(httpClient);
            ((NumericUpDown)dialog.Controls.Find("plafondInput", true).Single()).Value = 0m;

            // Act
            await dialog.Save();

            // Assert
            handler.Requests.Should().BeEmpty();
            dialog.Controls.Find("errorLabel", true).Single().Text.Should().Contain("plafond");
        });

        private static CartaRevolvingDialog CreateDialog(HttpClient httpClient)
        {
            var client = new FinanceApiClient(httpClient, new ApiConfiguration { BaseUrl = "https://localhost/", ApiKey = "test" });
            var card = new Conto(Guid.NewGuid(), "AmEx", "American Express", 0m);
            return new CartaRevolvingDialog(client, card, [card, new Conto(Guid.NewGuid(), "HelloBank", "Hello Bank", 0m)]);
        }
    }
}

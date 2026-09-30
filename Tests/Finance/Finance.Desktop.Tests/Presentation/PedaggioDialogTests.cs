using System.Diagnostics;
using System.Net;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

using Xunit.Abstractions;

namespace Finance.Desktop.Tests.Presentation
{
    public class PedaggioDialogTests(ITestOutputHelper output)
    {
        [Fact]
        public Task Toggle_WhenCatalogHasTwoThousandStations_UsesPreloadedLists() => WinFormsTest.Run(async () =>
        {
            // Arrange
            Casello[] stations = [.. Enumerable.Range(0, 2000).Select(index => new Casello(Guid.NewGuid(), $"Stazione {index:0000}"))];
            TariffaTratta[] tariffs = [.. stations.Skip(1).Take(999).Select(station => new TariffaTratta(Guid.NewGuid(), stations[0].Id, station.Id, "2.00", null, null, 0))];
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(stations)));
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(tariffs)));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new PedaggioDialog(client, new Conto(Guid.NewGuid(), "Telepass", "Telepass", 0m));
            await dialog.LoadData();
            var entrance = (ComboBox)dialog.Controls.Find("entrataInput", true).Single();
            entrance.SelectedItem = entrance.Items.Cast<Casello>().Single(item => item.Id == stations[0].Id);
            var toggle = (CheckBox)dialog.Controls.Find("otherStationsCheck", true).Single();

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (var index = 0; index < 100; index++)
            {
                toggle.Checked = !toggle.Checked;
            }
            stopwatch.Stop();

            // Assert
            ((ComboBox)dialog.Controls.Find("uscitaInput", true).Single()).Items.Count.Should().Be(999);
            handler.Requests.Should().HaveCount(3);
            output.WriteLine($"100 toggle con 2000 stazioni: {stopwatch.ElapsedMilliseconds} ms; nessuna richiesta aggiuntiva.");
        });

        [Fact]
        public Task Toggle_WhenOtherStationsSelected_SwitchesToComplementWithoutNetworkRequests() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var a = new Casello(Guid.NewGuid(), "Genova Est");
            var b = new Casello(Guid.NewGuid(), "Varazze");
            var c = new Casello(Guid.NewGuid(), "Chiavari");
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(new[] { a, b, c })));
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(new[] { new TariffaTratta(Guid.NewGuid(), a.Id, b.Id, "4.00", null, null, 0) })));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new PedaggioDialog(client, new Conto(Guid.NewGuid(), "Telepass", "Telepass", 0m));
            await dialog.LoadData();
            var entrance = (ComboBox)dialog.Controls.Find("entrataInput", true).Single();
            var exit = (ComboBox)dialog.Controls.Find("uscitaInput", true).Single();
            entrance.SelectedItem = entrance.Items.Cast<Casello>().Single(item => item.Id == a.Id);
            exit.Items.Cast<Casello>().Select(item => item.Id).Should().Equal(b.Id);
            exit.SelectedIndex = 0;

            // Act
            ((CheckBox)dialog.Controls.Find("otherStationsCheck", true).Single()).Checked = true;

            // Assert
            exit.Items.Cast<Casello>().Select(item => item.Id).Should().Equal(c.Id, Guid.Empty);
            exit.SelectedIndex.Should().Be(-1);
            handler.Requests.Should().HaveCount(3);
        });

        [Fact]
        public Task SelectExit_WhenTariffUnknown_DisablesConfirmationAndPreservesCustomDescription() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var a = new Casello(Guid.NewGuid(), "Genova Est");
            var b = new Casello(Guid.NewGuid(), "Varazze");
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(new[] { a, b })));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new PedaggioDialog(client, new Conto(Guid.NewGuid(), "Telepass", "Telepass", 0m));
            await dialog.LoadData();
            var entrance = (ComboBox)dialog.Controls.Find("entrataInput", true).Single();
            var exit = (ComboBox)dialog.Controls.Find("uscitaInput", true).Single();
            var description = (TextBox)dialog.Controls.Find("descriptionInput", true).Single();
            entrance.SelectedItem = entrance.Items.Cast<Casello>().Single(item => item.Id == a.Id);
            description.Text = "Serata Aegua";

            // Act
            exit.SelectedItem = exit.Items.Cast<Casello>().Single(item => item.Id == b.Id);

            // Assert
            description.Text.Should().Be("Serata Aegua");
            ((CheckBox)dialog.Controls.Find("confirmedCheck", true).Single()).Enabled.Should().BeFalse();
            ((Label)dialog.Controls.Find("errorLabel", true).Single()).Text.Should().Contain("Tariffa non disponibile");
        });
    }
}

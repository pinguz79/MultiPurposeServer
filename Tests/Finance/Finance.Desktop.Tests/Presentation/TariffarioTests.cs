using System.Net;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class TariffarioTests
    {
        [Fact]
        public Task TrattaDialog_WhenSaving_ClosesAndSendsAllDefinitionsOnce() => WinFormsTest.Run(() =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            using var http = new HttpClient(handler);
            using var dialog = new TrattaDialog(Client(http), "A ↔ B", Definitions());
            dialog.Shown += (_, _) => dialog.BeginInvoke(() => ((Button)dialog.Controls.Find("saveButton", true).Single()).PerformClick());

            // Act
            DialogResult result = dialog.ShowDialog();

            // Assert
            result.Should().Be(DialogResult.OK);
            handler.Requests.Should().HaveCount(1);
            using var body = JsonDocument.Parse(handler.RequestContent!);
            body.RootElement.GetProperty("definitions").GetArrayLength().Should().Be(2);
            return Task.CompletedTask;
        });

        [Fact]
        public Task TariffaDefinitionDialog_WhenSavingException_PreservesValidity() => WinFormsTest.Run(() =>
        {
            // Arrange
            var definition = new SaveTariffaDefinition(15.10m, null, new DateOnly(2026, 12, 31));
            using var dialog = new TariffaDefinitionDialog(definition, false);
            dialog.Show();
            ((NumericUpDown)dialog.Controls.Find("valueInput", true).Single()).Value = 13.80m;

            // Act
            ((Button)dialog.Controls.Find("saveButton", true).Single()).PerformClick();

            // Assert
            dialog.Definition.Should().Be(new SaveTariffaDefinition(13.80m, null, new DateOnly(2026, 12, 31)));
            dialog.DialogResult.Should().Be(DialogResult.OK);
            return Task.CompletedTask;
        });

        [Fact]
        public Task TrattaDialog_WhenCancelled_DoesNotSendRequests() => WinFormsTest.Run(() =>
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            using var http = new HttpClient(handler);
            using var dialog = new TrattaDialog(Client(http), "A ↔ B", Definitions());
            dialog.Show();

            // Act
            ((Button)dialog.Controls.Find("cancelButton", true).Single()).PerformClick();

            // Assert
            handler.Requests.Should().BeEmpty();
            dialog.DialogResult.Should().Be(DialogResult.Cancel);
            return Task.CompletedTask;
        });

        [Fact]
        public Task TariffarioView_WhenFilteringStation_ShowsMatchingBidirectionalRoutes() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var first = new Casello(Guid.NewGuid(), "Genova Est");
            var second = new Casello(Guid.NewGuid(), "Varazze");
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(new[] { first, second })));
            handler.Responses.Enqueue((HttpStatusCode.OK, JsonSerializer.Serialize(new[]
            {
                new TariffaTratta(Guid.NewGuid(), first.Id, second.Id, "4.20", null, null, 0),
            })));
            using var http = new HttpClient(handler);
            var client = Client(http);
            using var view = new TariffarioView(client);
            await view.LoadData();
            var filter = (TextBox)view.Controls.Find("filterInput", true).Single();
            var grid = (DataGridView)view.Controls.Find("grid", true).Single();

            // Act
            filter.Text = "VARA";

            // Assert
            grid.Rows.Count.Should().Be(1);
            grid.Rows[0].Cells[2].Value.Should().Be("4,20 €");
            handler.Requests.Should().HaveCount(2);
        });

        [Fact]
        public Task TrattaDialog_WhenSelectingBase_DisablesRemovalAndReordering() => WinFormsTest.Run(() =>
        {
            // Arrange
            using var http = new HttpClient(new RecordingHttpMessageHandler());
            using var dialog = new TrattaDialog(Client(http), "A ↔ B", Definitions());
            var grid = (DataGridView)dialog.Controls.Find("grid", true).Single();

            // Act
            grid.CurrentCell = grid.Rows[1].Cells[0];

            // Assert
            dialog.Controls.Find("deleteButton", true).Single().Enabled.Should().BeFalse();
            dialog.Controls.Find("upButton", true).Single().Enabled.Should().BeFalse();
            dialog.Controls.Find("downButton", true).Single().Enabled.Should().BeFalse();
            dialog.Controls.Find("editButton", true).Single().Enabled.Should().BeTrue();
            return Task.CompletedTask;
        });

        [Fact]
        public Task TariffaDefinitionDialog_WhenBase_DisablesDateLimits() => WinFormsTest.Run(() =>
        {
            // Arrange
            var definition = new SaveTariffaDefinition(0m, null, null);

            // Act
            using var dialog = new TariffaDefinitionDialog(definition, true);

            // Assert
            dialog.Controls.Find("fromInput", true).Single().Enabled.Should().BeFalse();
            dialog.Controls.Find("toInput", true).Single().Enabled.Should().BeFalse();
            ((NumericUpDown)dialog.Controls.Find("valueInput", true).Single()).Minimum.Should().Be(0);
            return Task.CompletedTask;
        });

        [Fact]
        public async Task UpdateTratta_WhenSaving_PreservesExceptionAndZeroBase()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            using var http = new HttpClient(handler);
            var client = Client(http);
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();

            // Act
            await client.UpdateTratta(first, second, [new(15.10m, null, new DateOnly(2026, 12, 31)), new(0m, null, null)]);

            // Assert
            handler.Request!.Method.Should().Be(HttpMethod.Patch);
            handler.Request.RequestUri!.AbsolutePath.Should().EndWith($"/{first}/{second}");
            using var body = JsonDocument.Parse(handler.RequestContent!);
            JsonElement definitions = body.RootElement.GetProperty("definitions");
            definitions.GetArrayLength().Should().Be(2);
            definitions[0].GetProperty("value").GetDecimal().Should().Be(15.10m);
            definitions[0].GetProperty("validTo").GetString().Should().Be("2026-12-31");
            definitions[1].GetProperty("value").GetDecimal().Should().Be(0m);
            definitions[1].GetProperty("validTo").ValueKind.Should().Be(JsonValueKind.Null);
        }

        [Fact]
        public async Task DeleteCasello_WhenInUse_ReturnsServerExplanationWithoutRetry()
        {
            // Arrange
            var handler = new RecordingHttpMessageHandler();
            handler.Responses.Enqueue((HttpStatusCode.Conflict, "{\"title\":\"Stazione in uso\",\"detail\":\"Esistono pedaggi collegati.\"}"));
            using var http = new HttpClient(handler);
            var client = Client(http);

            // Act
            Func<Task> action = () => client.DeleteCasello(Guid.NewGuid());

            // Assert
            (await action.Should().ThrowAsync<FinanceApiException>()).Which.Message.Should().Contain("pedaggi collegati");
            handler.Requests.Should().HaveCount(1);
        }

        private static FinanceApiClient Client(HttpClient http) => new(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });

        private static TariffaTratta[] Definitions()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            return [new(Guid.NewGuid(), first, second, "15.10", null, new DateOnly(2026, 12, 31), 0), new(Guid.NewGuid(), first, second, "0", null, null, 1)];
        }
    }
}

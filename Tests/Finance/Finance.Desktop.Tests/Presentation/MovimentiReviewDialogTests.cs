using System.Net;
using System.Reflection;
using System.Text.Json;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;

using FluentAssertions;

namespace Finance.Desktop.Tests.Presentation
{
    public class MovimentiReviewDialogTests
    {
        [Fact]
        public Task GridCellMouseClick_WhenMiddleButton_ConfirmsClickedRowNotCurrentRow() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var first = new MovimentoEdit(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), "Primo", "10.00", "Conto", false, null, 10m, null);
            var second = first with { Id = Guid.NewGuid(), Description = "Secondo" };
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = JsonSerializer.Serialize(new[] { first, second }) };
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new MovimentiReviewDialog(client, []) { BindingContext = new BindingContext() };
            await dialog.RefreshItems();
            var grid = (DataGridView)dialog.Controls.Find("grid", true).Single();
            grid.CurrentCell = grid.Rows[0].Cells[1];
            string? confirmation = null;
            handler.BeforeResponse = _ =>
            {
                if (handler.Request?.Method == HttpMethod.Post)
                {
                    confirmation = handler.RequestContent;
                }
                return Task.CompletedTask;
            };
            handler.Responses.Enqueue((HttpStatusCode.OK, "{}"));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));

            // Act
            typeof(DataGridView).GetMethod("OnCellMouseClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(grid,
                [new DataGridViewCellMouseEventArgs(3, 1, 1, 1, new MouseEventArgs(MouseButtons.Middle, 1, 1, 1, 0))]);

            // Assert
            using var payload = JsonDocument.Parse(confirmation!);
            payload.RootElement.GetProperty("ids").EnumerateArray().Select(item => item.GetGuid()).Should().Equal(second.Id);
            handler.Requests.Count(item => item.Method == HttpMethod.Post).Should().Be(1);
        });

        [Fact]
        public Task RunMovementAction_WhenConfirming_IgnoresConcurrentActionsAndIneligibleRows() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var handler = new RecordingHttpMessageHandler { BeforeResponse = token => release.Task.WaitAsync(token) };
            handler.Responses.Enqueue((HttpStatusCode.OK, "{}"));
            handler.Responses.Enqueue((HttpStatusCode.OK, "[]"));
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new MovimentiReviewDialog(client, []);
            var movement = new MovimentoEdit(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), "Spesa", "10.00", "Conto", false, null, 10m, null);

            // Act
            Task pending = dialog.RunMovementAction(movement, "confirm");
            // Assert
            using var body = JsonDocument.Parse(handler.RequestContent!);
            body.RootElement.GetProperty("ids")[0].GetGuid().Should().Be(movement.Id);
            await dialog.RunMovementAction(movement, "confirm");
            handler.Requests.Should().HaveCount(1);
            release.SetResult();
            await pending;

            dialog.Controls.Find("grid", true).Single().Enabled.Should().BeTrue();
            handler.Requests.Should().HaveCount(2);
            await dialog.RunMovementAction(movement with { IsConfirmed = true }, "confirm");
            await dialog.RunMovementAction(movement with { CanConfirm = false }, "confirm");
            handler.Requests.Should().HaveCount(2);
        });

        [Fact]
        public Task RefreshItems_WhenRowsChangeOrder_PreservesCheckedIdsAndCurrentRow() => WinFormsTest.Run(async () =>
        {
            // Arrange
            var first = new MovimentoEdit(Guid.NewGuid(), new DateOnly(2026, 9, 1), "Primo", "10.00", "Conto", false, null, 10m, null);
            var second = first with { Id = Guid.NewGuid(), Description = "Secondo" };
            var handler = new RecordingHttpMessageHandler { ResponseStatusCode = HttpStatusCode.OK, ResponseContent = JsonSerializer.Serialize(new[] { first, second }) };
            using var http = new HttpClient(handler);
            var client = new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/", HeaderName = "X-Key", ApiKey = "test" });
            using var dialog = new MovimentiReviewDialog(client, []);
            dialog.BindingContext = new BindingContext();
            var grid = (DataGridView)dialog.Controls.Find("grid", true).Single();
            await dialog.RefreshItems();
            grid.Rows[0].Cells[0].Value = true;
            grid.CurrentCell = grid.Rows[0].Cells[1];
            handler.ResponseContent = JsonSerializer.Serialize(new[] { second, first with { Description = "Modificato" } });

            // Act
            await dialog.RefreshItems();

            // Assert
            grid.Rows[1].Cells[0].Value.Should().Be(true);
            grid.Rows[0].Cells[0].Value.Should().Be(false);
            ((MovimentoEdit)grid.CurrentRow!.DataBoundItem!).Id.Should().Be(first.Id);
        });
    }
}

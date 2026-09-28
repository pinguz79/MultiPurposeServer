using System.Net;
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

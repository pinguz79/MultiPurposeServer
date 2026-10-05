using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;
using Finance.Desktop.Tests.Infrastructure;
using FluentAssertions;

namespace Finance.Desktop.Tests
{
    public class FinanziamentoViewTests
    {
        [Fact]
        public Task PastInstallmentsHaveMovementBackgroundAndVerifiedCellRemainsDistinct() => WinFormsTest.Run(() =>
        {
            using var http = new HttpClient();
            using var view = new FinanziamentoView(new FinanceApiClient(http, new ApiConfiguration { BaseUrl = "https://localhost/" }), "Test");
            view.BindingContext = new BindingContext();
            var date = new DateOnly(2026, 10, 1);
            var loan = new Finanziamento(Guid.NewGuid(), "Test", "Test", "Test", 1000m, 0m, 100m, 0m, 0m, date.AddMonths(-1), 1, 10, false);
            RataFinanziamento[] rows = [
                new(1, date.AddMonths(-1), true, 100m, 0m, 0m, 0m, 100m, 900m, 0m, 900m),
                new(2, date, false, 100m, 0m, 0m, 0m, 100m, 800m, 0m, null)];
            view.Render(new PianoFinanziamento(loan, date, 900m, 9, 900m, date.AddMonths(8), 0m, 0m, rows[0], rows));
            var grid = (DataGridView)view.Controls.Find("grid", true).Single();
            var toggle = (CheckBox)view.Controls.Find("showPastCheckBox", true).Single();
            grid.Rows.Count.Should().Be(1);
            toggle.Checked = true;
            grid.Rows.Count.Should().Be(2);
            grid.Rows[0].DefaultCellStyle.BackColor.Should().Be(Color.FromArgb(210, 230, 247));
            grid.Rows[0].Cells[9].Style.BackColor.Should().Be(Color.FromArgb(173, 211, 240));
            grid.Rows[1].DefaultCellStyle.BackColor.Should().Be(Color.White);
            toggle.Checked = false;
            grid.Rows.Count.Should().Be(1);
            return Task.CompletedTask;
        });
    }
}

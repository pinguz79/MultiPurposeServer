using System.Reflection;

using Finance.Desktop.Configuration;
using Finance.Desktop.Models;
using Finance.Desktop.Services;

using FluentAssertions;

namespace Finance.Desktop.Tests
{
    public class MainFormTests
    {
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

using System.Net;

using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class LegacyZenPhotoRouteSmokeTests(ITestOutputHelper output)
    {
        [ProductionSmokeFact]
        public async Task BrowseLegacyZenPhotoPath_WhenGuardIsDeployed_ReturnsGone()
        {
            // Regression context: BL-0034 prevents obsolete ZenPhoto paths from reaching Portfolio.Api album routing.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };
            const string legacyPath = "zp-core/full-image.php";

            // Act
            var response = await client.GetAsync(legacyPath);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, legacyPath), (int)response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.Gone);
            content.Should().Contain("Risorsa rimossa definitivamente.");
        }
    }
}

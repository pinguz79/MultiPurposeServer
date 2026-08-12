using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class ServicesPageSmokeTests(ITestOutputHelper output)
    {
        [ProductionSmokeFact]
        public async Task BrowseServicesPage_WhenPageIsDeployed_ExposesEditorialContentMetadataAndContacts()
        {
            // Regression context: BL-0015 protects the services page introduced for AdSense readiness.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            // Act
            var response = await client.GetAsync("servizi-fotografici");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, "servizi-fotografici"), (int)response.StatusCode);
            response.IsSuccessStatusCode.Should().BeTrue();
            content.Should().Contain("<h1>Servizi fotografici</h1>");
            content.Should().Contain("Commissione o collaborazione TF");
            content.Should().Contain("Selezione e postproduzione");
            content.Should().Contain("https://www.instagram.com/marcolepri979");
            content.Should().Contain("https://www.facebook.com/marco.lepre979");
            content.Should().Contain("https://wa.me/393475095788");
            content.Should().Contain("rel=\"canonical\"");
        }
    }
}

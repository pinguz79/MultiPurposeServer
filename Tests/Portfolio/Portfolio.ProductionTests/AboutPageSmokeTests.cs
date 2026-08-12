using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class AboutPageSmokeTests(ITestOutputHelper output)
    {
        [ProductionSmokeFact]
        public async Task BrowseAboutPage_WhenPageIsDeployed_ExposesInterviewMetadataAndContacts()
        {
            // Regression context: BL-0015 protects the editorial author page introduced for AdSense readiness.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            // Act
            var response = await client.GetAsync("chi-sono");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, "chi-sono"), (int)response.StatusCode);
            response.IsSuccessStatusCode.Should().BeTrue();
            content.Should().Contain("<h1>Chi sono</h1>");
            content.Should().Contain("mi sono fatto intervistare da un’AI");
            content.Should().Contain("https://www.instagram.com/marcolepri979");
            content.Should().Contain("https://www.facebook.com/marco.lepre979");
            content.Should().Contain("https://wa.me/393475095788");
            content.Should().Contain("rel=\"canonical\"");
            content.Should().Contain("marco-lepri-profile.png");
        }
    }
}

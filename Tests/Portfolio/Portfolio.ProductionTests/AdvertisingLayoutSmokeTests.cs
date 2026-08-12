using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class AdvertisingLayoutSmokeTests(ITestOutputHelper output)
    {
        private const string AdvertisementScript = "//ad.altervista.org/js.ad/size=300X250/";

        private static readonly (string Path, string Context)[] RepresentativePages =
        [
            ("./", "navigation"),
            ("modelle-modelli", "navigation"),
            ("modelle-modelli/annalisa-l/urban-style", "photo-album")
        ];

        [ProductionSmokeFact]
        public async Task BrowseRepresentativePages_WhenAdvertisingIsDeployed_PreservesSingleContextualBanner()
        {
            // Regression context: BL-0006 protects the Altervista banner from partial, duplicated or misplaced deployments.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            // Act
            var pages = new List<(Uri Url, string Context, HttpResponseMessage Response, string Content)>();

            foreach (var representativePage in RepresentativePages)
            {
                var response = await client.GetAsync(representativePage.Path);
                var content = await response.Content.ReadAsStringAsync();

                pages.Add((new Uri(webBaseUrl, representativePage.Path), representativePage.Context, response, content));
            }

            // Assert
            foreach (var page in pages)
            {
                output.WriteLine("{0}: HTTP {1}", page.Url, (int)page.Response.StatusCode);

                page.Response.IsSuccessStatusCode.Should().BeTrue($"{page.Url} must be reachable");
                page.Content.Should().Contain("aria-label=\"Pubblicità\"", $"{page.Url} must identify advertising accessibly");
                page.Content.Should().Contain($"advertisement--{page.Context}", $"{page.Url} must use its expected placement context");
                page.Content.Should().Contain(AdvertisementScript, $"{page.Url} must load the selected 300x250 Altervista format");
                CountOccurrences(page.Content, AdvertisementScript).Should().Be(1, $"{page.Url} must render exactly one banner");
            }
        }

        [ProductionSmokeFact]
        public async Task LoadAdvertisingStyles_WhenSiteIsDeployed_PreservesResponsivePhotoAlbumOrdering()
        {
            // Regression context: BL-0006 keeps the banner before photos on desktop and after them on tablet/mobile.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            // Act
            var stylesheetUrl = new Uri(webBaseUrl, "public/css/components/advertisement.css");
            var response = await client.GetAsync(stylesheetUrl);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            output.WriteLine("{0}: HTTP {1}", stylesheetUrl, (int)response.StatusCode);

            response.IsSuccessStatusCode.Should().BeTrue($"{stylesheetUrl} must be reachable");
            content.Should().Contain("@media (max-width: 1024px)", "tablet and mobile ordering must have an explicit breakpoint");
            content.Should().Contain(".photo-album-layout .photo-browser", "the photo browser must participate in responsive ordering");
            content.Should().Contain(".photo-album-layout .advertisement--photo-album", "the photo-album banner must participate in responsive ordering");
            content.Should().Contain("order: 1", "photos must precede advertising on tablet and mobile");
            content.Should().Contain("order: 2", "advertising must follow photos on tablet and mobile");
        }

        private static int CountOccurrences(string content, string value)
        {
            return content.Split(value, StringSplitOptions.None).Length - 1;
        }
    }
}

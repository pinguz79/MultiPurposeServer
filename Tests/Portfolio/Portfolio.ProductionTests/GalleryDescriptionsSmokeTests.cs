using System.Net;

using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class GalleryDescriptionsSmokeTests(ITestOutputHelper output)
    {
        private static readonly IReadOnlyCollection<(string Path, string Marker)> GalleryDescriptions =
        [
            ("Calendari", "Il progetto Calendario nasce nel 2019"),
            ("Modelle-Modelli", "Modelle e Modelli è la vetrina dedicata"),
            ("Sfilate-Concorsi", "Sfilate e Concorsi raccoglie i servizi")
        ];

        [ProductionSmokeFact]
        public async Task BrowseGalleries_WhenEditorialDescriptionsAreDeployed_ExposeVisibleAndMetadataContent()
        {
            // Regression context: BL-0015 protects editorial descriptions stored through the routing cache.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            foreach (var gallery in GalleryDescriptions)
            {
                // Act
                var response = await client.GetAsync(gallery.Path);
                var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

                // Assert
                output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, gallery.Path), (int)response.StatusCode);
                response.IsSuccessStatusCode.Should().BeTrue();
                content.Should().Contain("class=\"album-description\"");
                content.Should().Contain(gallery.Marker);
                content.Should().Contain($"<meta name=\"description\" content=\"{gallery.Marker}");
            }
        }
    }
}

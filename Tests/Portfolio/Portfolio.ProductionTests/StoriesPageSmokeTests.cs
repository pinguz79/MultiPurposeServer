using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class StoriesPageSmokeTests(ITestOutputHelper output)
    {
        [ProductionSmokeFact]
        public async Task BrowseStories_WhenMiniCmsIsDeployed_ExposesIndexArticleMetadataAndAlbumBacklink()
        {
            // Regression context: BL-0023 protects the first file-based editorial content introduced for AdSense readiness.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            // Act
            var indexResponse = await client.GetAsync("stories");
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            var articleResponse = await client.GetAsync("stories/fairytales-2021");
            var articleContent = await articleResponse.Content.ReadAsStringAsync();
            var albumResponse = await client.GetAsync("Calendari/2021/FairyTales");
            var albumContent = await albumResponse.Content.ReadAsStringAsync();

            // Assert
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, "stories"), (int)indexResponse.StatusCode);
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, "stories/fairytales-2021"), (int)articleResponse.StatusCode);
            indexResponse.IsSuccessStatusCode.Should().BeTrue();
            articleResponse.IsSuccessStatusCode.Should().BeTrue();
            albumResponse.IsSuccessStatusCode.Should().BeTrue();
            indexContent.Should().Contain("<h1>Storie, progetti e fotografie</h1>");
            indexContent.Should().Contain("/portfolio/stories/fairytales-2021");
            articleContent.Should().Contain("<h1>FairyTales 2021: dietro le quinte di un progetto titanico</h1>");
            articleContent.Should().Contain("property=\"og:type\" content=\"article\"");
            articleContent.Should().Contain("Portfolio/FrontEnd/Media/EditorialCover/f5d0d90a-344b-4f17-bedf-9de2a7f5b01e");
            articleContent.Should().Contain("property=\"article:published_time\" content=\"2026-08-10\"");
            articleContent.Should().Contain("rel=\"canonical\" href=\"https://marcolepriph.altervista.org/portfolio/stories/fairytales-2021\"");
            articleContent.Should().Contain("Calendari/2021/FairyTales-Camilla");
            albumContent.Should().Contain("/portfolio/stories/fairytales-2021");
        }

        [ProductionSmokeFact]
        public async Task BrowseGermanaStory_WhenDeployed_ExposesArticleMetadataAndAlbumBacklink()
        {
            // Regression context: BL-0032 protects the Germana 2023 editorial story and its relationship with the Album.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient { BaseAddress = webBaseUrl };

            // Act
            var indexResponse = await client.GetAsync("stories");
            var indexContent = await indexResponse.Content.ReadAsStringAsync();
            var articleResponse = await client.GetAsync("stories/germana-2023");
            var articleContent = await articleResponse.Content.ReadAsStringAsync();
            var albumResponse = await client.GetAsync("Calendari/2023/Germana-2023");
            var albumContent = await albumResponse.Content.ReadAsStringAsync();

            // Assert
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, "stories/germana-2023"), (int)articleResponse.StatusCode);

            indexResponse.IsSuccessStatusCode.Should().BeTrue();
            articleResponse.IsSuccessStatusCode.Should().BeTrue();
            albumResponse.IsSuccessStatusCode.Should().BeTrue();
            indexContent.Should().Contain("/portfolio/stories/germana-2023");
            articleContent.Should().Contain("<h1>Germana 2023: tre set per un calendario mai stampato</h1>");
            articleContent.Should().Contain("property=\"og:type\" content=\"article\"");
            articleContent.Should().Contain("Portfolio/FrontEnd/Media/EditorialCover/e3d8bf45-6bb9-4168-9f38-d2b986ff72ec");
            articleContent.Should().Contain("property=\"article:published_time\" content=\"2026-08-11\"");
            articleContent.Should().Contain("rel=\"canonical\" href=\"https://marcolepriph.altervista.org/portfolio/stories/germana-2023\"");
            articleContent.Should().Contain("Calendari/2023/Germana-2023");
            albumContent.Should().Contain("/portfolio/stories/germana-2023");
        }
    }
}

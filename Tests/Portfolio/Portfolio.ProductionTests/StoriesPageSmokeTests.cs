using FluentAssertions;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

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
        articleContent.Should().Contain("property=\"article:published_time\" content=\"2026-08-10\"");
        articleContent.Should().Contain("rel=\"canonical\" href=\"https://marcolepriph.altervista.org/portfolio/stories/fairytales-2021\"");
        articleContent.Should().Contain("Calendari/2021/FairyTales-Camilla");
        albumContent.Should().Contain("/portfolio/stories/fairytales-2021");
    }
}

using FluentAssertions;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

public sealed class SocialMetadataSmokeTests(ITestOutputHelper output)
{
    private const string AlbumPath = "Modelle-Modelli/Annalisa-L/Urban-Style";

    [ProductionSmokeFact]
    public async Task BrowseAlbum_WhenSocialMetadataIsDeployed_ExposesStableRecognizablePreview()
    {
        // Regression context: BL-0007 protects the manual sharing preview of public album links.

        // Arrange
        var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
        var expectedCanonicalUrl = new Uri(webBaseUrl, AlbumPath).ToString();
        using var client = new HttpClient { BaseAddress = webBaseUrl };

        // Act
        var response = await client.GetAsync(AlbumPath + "?page=1&pageSize=12");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        output.WriteLine("{0}: HTTP {1}", expectedCanonicalUrl, (int)response.StatusCode);

        response.IsSuccessStatusCode.Should().BeTrue("the representative album must be reachable");
        content.Should().Contain("<title>Urban Style | Marco Lepri Photography</title>");
        content.Should().Contain($"<link rel=\"canonical\" href=\"{expectedCanonicalUrl}\">");
        content.Should().Contain("<meta property=\"og:type\" content=\"website\">");
        content.Should().Contain("<meta property=\"og:site_name\" content=\"Marco Lepri Photography\">");
        content.Should().Contain("<meta property=\"og:title\" content=\"Urban Style\">");
        content.Should().Contain($"<meta property=\"og:url\" content=\"{expectedCanonicalUrl}\">");
        content.Should().Contain("<meta property=\"og:image\"", "a populated photo album must expose a preview image");
        content.Should().Contain("<meta name=\"twitter:card\" content=\"summary_large_image\">");
        content.Should().Contain($"data-share-url=\"{expectedCanonicalUrl}\"", "sharing must use the canonical URL");
    }
}

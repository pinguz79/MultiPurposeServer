using FluentAssertions;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

public sealed class PrivacyLayoutSmokeTests(ITestOutputHelper output)
{
    private static readonly string[] RepresentativePaths =
    [
        "./",
        "modelle-modelli",
        "modelle-modelli/annalisa-l/urban-style"
    ];

    [ProductionSmokeFact]
    public async Task BrowseRepresentativePages_WhenSiteIsDeployed_PreservesConsentHeadAndPrivacyFooter()
    {
        // Regression context: BL-0006 protects the Iubenda CMP and privacy controls from partial or stale deployments.

        // Arrange
        var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
        using var client = new HttpClient { BaseAddress = webBaseUrl };

        // Act
        var pages = new List<(Uri Url, HttpResponseMessage Response, string Content)>();

        foreach (var path in RepresentativePaths)
        {
            var response = await client.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();

            pages.Add((new Uri(webBaseUrl, path), response, content));
        }

        // Assert
        foreach (var page in pages)
        {
            output.WriteLine("{0}: HTTP {1}", page.Url, (int)page.Response.StatusCode);

            page.Response.IsSuccessStatusCode.Should().BeTrue($"{page.Url} must be reachable");
            page.Content.Should().Contain("https://cdn.iubenda.com/cs/tcf/stub-v2.js", $"{page.Url} must load the TCF stub");
            page.Content.Should().Contain("https://cdn.iubenda.com/cs/iubenda_cs.js", $"{page.Url} must load the Iubenda CMP");
            page.Content.Should().Contain("enableTcf: true", $"{page.Url} must enable TCF v2");
            page.Content.Should().Contain("<footer class=\"site-footer\">", $"{page.Url} must render the shared footer");
            page.Content.Should().Contain("https://www.iubenda.com/privacy-policy/24901911", $"{page.Url} must expose the privacy policy");
            page.Content.Should().Contain("iubenda-advertising-preferences-link", $"{page.Url} must expose consent preferences");
        }
    }
}

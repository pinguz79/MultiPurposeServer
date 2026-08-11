using System.Xml.Linq;
using FluentAssertions;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

public sealed class SeoDiscoverySmokeTests(ITestOutputHelper output)
{
    [ProductionSmokeFact]
    public async Task BrowseSeoDiscoveryFiles_WhenDeployed_ExposesRobotsAndNavigableSitemap()
    {
        // Regression context: BL-0015 protects crawler discovery introduced for AdSense readiness.

        // Arrange
        var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
        var siteRootUrl = new Uri(webBaseUrl, "/");
        using var client = new HttpClient();

        // Act
        var robotsResponse = await client.GetAsync(new Uri(siteRootUrl, "robots.txt"));
        var robots = await robotsResponse.Content.ReadAsStringAsync();
        var sitemapResponse = await client.GetAsync(new Uri(webBaseUrl, "sitemap.xml"));
        var sitemapContent = await sitemapResponse.Content.ReadAsStringAsync();

        // Assert
        output.WriteLine("{0}: HTTP {1}", new Uri(siteRootUrl, "robots.txt"), (int)robotsResponse.StatusCode);
        output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, "sitemap.xml"), (int)sitemapResponse.StatusCode);
        robotsResponse.IsSuccessStatusCode.Should().BeTrue();
        robots.Should().Contain("User-agent: *");
        robots.Should().Contain("User-agent: SERankingBacklinksBot");
        robots.Should().Contain("Disallow: /portfolio/zp-core/");
        robots.Should().Contain("Sitemap: https://marcolepriph.altervista.org/portfolio/sitemap.xml");
        sitemapResponse.IsSuccessStatusCode.Should().BeTrue();

        var sitemap = XDocument.Parse(sitemapContent);
        XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = sitemap
            .Descendants(sitemapNamespace + "loc")
            .Select(element => element.Value)
            .ToArray();

        urls.Should().Contain("https://marcolepriph.altervista.org/portfolio/");
        urls.Should().Contain("https://marcolepriph.altervista.org/portfolio/servizi-fotografici");
        urls.Should().Contain("https://marcolepriph.altervista.org/portfolio/chi-sono");
        urls.Should().Contain("https://marcolepriph.altervista.org/portfolio/stories");
        urls.Should().Contain("https://marcolepriph.altervista.org/portfolio/stories/fairytales-2021");
        urls.Should().OnlyHaveUniqueItems();
        urls.Should().HaveCountGreaterThan(3);
    }
}

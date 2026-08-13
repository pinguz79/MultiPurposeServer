using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed partial class MetadataUniquenessSmokeTests(ITestOutputHelper output)
    {
        [ProductionSmokeFact]
        public async Task BrowseSitemapPages_WhenMetadataIsDeployed_ExposesUniqueTitlesDescriptionsAndCanonicalUrls()
        {
            // Regression context: BL-0015 protects distinct metadata for albums with repeated names.

            // Arrange
            var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
            using var client = new HttpClient();
            var sitemapContent = await client.GetStringAsync(new Uri(webBaseUrl, "sitemap.xml"));
            var sitemap = XDocument.Parse(sitemapContent);
            XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urls = sitemap
                .Descendants(sitemapNamespace + "loc")
                .Select(element => element.Value)
                .ToArray();

            // Act
            var pages = new List<PageMetadataSnapshot>();

            foreach (var url in urls)
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                response.IsSuccessStatusCode.Should().BeTrue("{0} must be indexable", url);
                pages.Add(new PageMetadataSnapshot(
                    url,
                    Decode(TitleRegex().Match(content).Groups[1].Value),
                    Decode(DescriptionRegex().Match(content).Groups[1].Value),
                    Decode(CanonicalRegex().Match(content).Groups[1].Value)));
            }

            // Assert
            output.WriteLine("Verified metadata for {0} sitemap URLs.", pages.Count);
            pages.Should().OnlyContain(page => !string.IsNullOrWhiteSpace(page.Title));
            pages.Should().OnlyContain(page => !string.IsNullOrWhiteSpace(page.Description));
            pages.Should().OnlyContain(page => !string.IsNullOrWhiteSpace(page.CanonicalUrl));
            pages.Select(page => page.Title).Should().OnlyHaveUniqueItems();
            pages.Select(page => page.Description).Should().OnlyHaveUniqueItems();
            pages.Select(page => page.CanonicalUrl).Should().OnlyHaveUniqueItems();
            pages.Should().OnlyContain(
                page => NormalizeUrl(page.Url) == NormalizeUrl(page.CanonicalUrl),
                "every sitemap URL must declare itself as canonical");
        }

        private static string Decode(string value) => WebUtility.HtmlDecode(value).Trim();

        private static string NormalizeUrl(string value) => value.TrimEnd('/');

        [GeneratedRegex("<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex TitleRegex();

        [GeneratedRegex("<meta\\s+name=\"description\"\\s+content=\"([^\"]*)\"", RegexOptions.IgnoreCase)]
        private static partial Regex DescriptionRegex();

        [GeneratedRegex("<link\\s+rel=\"canonical\"\\s+href=\"([^\"]*)\"", RegexOptions.IgnoreCase)]
        private static partial Regex CanonicalRegex();
    }
}

using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

public sealed class HeadingHierarchySmokeTests(ITestOutputHelper output)
{
    [ProductionSmokeFact]
    public async Task BrowsePrimaryPages_WhenDeployed_ExposesOneMainHeadingPerPage()
    {
        // Regression context: BL-0015 protects the semantic heading hierarchy used for AdSense readiness.

        // Arrange
        var webBaseUrl = ProductionTestSettings.LoadWebBaseUrl();
        using var client = new HttpClient { BaseAddress = webBaseUrl };
        var paths = new[]
        {
            "",
            "Calendari/2021/FairyTales"
        };

        // Act
        var pages = new List<(string Path, HttpResponseMessage Response, string Content)>();
        foreach (var path in paths)
        {
            var response = await client.GetAsync(path);
            pages.Add((path, response, await response.Content.ReadAsStringAsync()));
        }

        // Assert
        foreach (var page in pages)
        {
            output.WriteLine("{0}: HTTP {1}", new Uri(webBaseUrl, page.Path), (int)page.Response.StatusCode);
            page.Response.IsSuccessStatusCode.Should().BeTrue();
            Regex.Matches(page.Content, "<h1(?:\\s|>)", RegexOptions.IgnoreCase).Should().HaveCount(1);
            page.Content.Should().MatchRegex("/public/css/(?:home|album)\\.css\\?v=\\d+");
        }
    }
}

using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

public sealed class ApiDocumentationSmokeTests(ITestOutputHelper output)
{
    [ProductionSmokeFact]
    public async Task BrowseApiDocumentation_WhenScalarIsDeployed_ExposesUiAndValidOpenApiDocument()
    {
        // Regression context: BL-0016 replaces Swagger UI and Swashbuckle with Scalar and native OpenAPI.

        // Arrange
        var documentationBaseUrl = ProductionTestSettings.LoadApiDocumentationBaseUrl();
        using var client = new HttpClient { BaseAddress = documentationBaseUrl };

        // Act
        var scalarResponse = await client.GetAsync("scalar/");
        var scalarContent = await scalarResponse.Content.ReadAsStringAsync();
        var openApiResponse = await client.GetAsync("openapi/v1.json");
        var openApiContent = await openApiResponse.Content.ReadAsStringAsync();
        var retiredSwaggerResponse = await client.GetAsync("swagger/");

        // Assert
        output.WriteLine("{0}: HTTP {1}", new Uri(documentationBaseUrl, "scalar/"), (int)scalarResponse.StatusCode);
        output.WriteLine("{0}: HTTP {1}", new Uri(documentationBaseUrl, "openapi/v1.json"), (int)openApiResponse.StatusCode);

        scalarResponse.EnsureSuccessStatusCode();
        scalarContent.Should().ContainEquivalentOf("scalar");
        openApiResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(openApiContent);
        var root = document.RootElement;
        root.GetProperty("info").GetProperty("title").GetString().Should().Be("MPS API");

        var securitySchemes = root.GetProperty("components").GetProperty("securitySchemes");
        securitySchemes.TryGetProperty("PortfolioFrontEndApiKey", out _).Should().BeTrue();
        securitySchemes.TryGetProperty("PortfolioBackEndApiKey", out _).Should().BeTrue();

        var documentedPaths = root.GetProperty("paths").EnumerateObject().Select(path => path.Name).ToArray();
        documentedPaths.Should().NotBeEmpty();
        documentedPaths.Should().NotContain(path => path.Contains("health", StringComparison.OrdinalIgnoreCase));
        retiredSwaggerResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

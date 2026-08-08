using FluentAssertions;
using Xunit.Abstractions;

namespace Portfolio.ProductionTests;

public sealed class GalleryCacheRegenerationTests(ITestOutputHelper output)
{
    [ProductionFact]
    public async Task BrowseGallery_WhenCachesAreRegenerated_RemainsNavigableWithColdAndWarmCache()
    {
        // Regression context: BL-0001 records unreachable nested albums caused by stale routing-cache paths.

        // Arrange
        var settings = ProductionTestSettings.Load();
        using var client = new GalleryNavigationClient(settings);

        // Act
        var historicalCacheRun = await client.Browse("Historical cache baseline");
        var clearResult = await client.ClearAllCaches();
        var coldCacheRun = await client.Browse("Cold cache regeneration");
        var warmCacheRun = await client.Browse("Warm cache verification");

        // Assert
        output.WriteLine(historicalCacheRun.Format());
        output.WriteLine(
            "Cache clear: {0} album routes, {1} photo routes, {2} API responses deleted.",
            clearResult.AlbumRoutingEntriesDeleted,
            clearResult.PhotoRoutingEntriesDeleted,
            clearResult.ApiResponseEntriesDeleted);
        output.WriteLine(coldCacheRun.Format());
        output.WriteLine(warmCacheRun.Format());

        coldCacheRun.Failures.Should().BeEmpty(coldCacheRun.Format());
        warmCacheRun.Failures.Should().BeEmpty(warmCacheRun.Format());
    }
}

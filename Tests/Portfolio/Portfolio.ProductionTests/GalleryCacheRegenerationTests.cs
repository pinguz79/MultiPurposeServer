using FluentAssertions;

using Xunit.Abstractions;

namespace Portfolio.ProductionTests
{
    public sealed class GalleryCacheRegenerationTests(ITestOutputHelper output)
    {
        private const string DirectColdCacheAlbumPath = "Modelle-Modelli/Cecilia-B/sunset-at-paraggi";

        [ProductionFact]
        public async Task BrowseGallery_WhenCachesAreRegenerated_RemainsNavigableWithColdAndWarmCache()
        {
            // Regression context: BL-0001 / GitHub #1 records unreachable nested albums caused by stale routing-cache paths.

            // Arrange
            var settings = ProductionTestSettings.Load();
            using var client = new GalleryNavigationClient(settings);
            using var webClient = new HttpClient { BaseAddress = settings.WebBaseUrl };

            // Act
            var historicalCacheRun = await client.Browse("Historical cache baseline");
            var clearResult = await client.ClearAllCaches();
            using var directColdCacheResponse = await webClient.GetAsync(DirectColdCacheAlbumPath);
            var coldCacheRun = await client.Browse("Cold cache regeneration");
            var warmCacheRun = await client.Browse("Warm cache verification");

            // Assert
            output.WriteLine(historicalCacheRun.Format());
            output.WriteLine(
                "Cache clear: {0} album routes, {1} photo routes, {2} API responses deleted.",
                clearResult.AlbumRoutingEntriesDeleted,
                clearResult.PhotoRoutingEntriesDeleted,
                clearResult.ApiResponseEntriesDeleted);
            output.WriteLine(
                "Direct cold-cache access {0}: HTTP {1}.",
                DirectColdCacheAlbumPath,
                (int)directColdCacheResponse.StatusCode);
            output.WriteLine(coldCacheRun.Format());
            output.WriteLine(warmCacheRun.Format());

            directColdCacheResponse.IsSuccessStatusCode.Should().BeTrue(
                "BL-0001 / GitHub #1 requires a directly opened nested album to cache its fullPath");
            coldCacheRun.Failures.Should().BeEmpty(coldCacheRun.Format());
            warmCacheRun.Failures.Should().BeEmpty(warmCacheRun.Format());
        }
    }
}

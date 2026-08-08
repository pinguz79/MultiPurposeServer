# Portfolio production tests

These tests exercise the deployed Portfolio.Api and Portfolio.Web instances. They are intentionally separated from Unit and Integration Tests because their result depends on network access, production data and external hosting.

The cache-regeneration scenario is disabled by default because it clears the rebuildable Portfolio.Web caches before checking cold and warm navigation.

## Required environment variables

```text
PORTFOLIO_RUN_CACHE_REGENERATION_TESTS=true
PORTFOLIO_FRONTEND_API_KEY=<front-end key>
PORTFOLIO_BACKEND_API_KEY=<back-end key>
```

Optional overrides:

```text
PORTFOLIO_API_BASE_URL=https://www.modelbook.cloud/Portfolio/
PORTFOLIO_WEB_BASE_URL=https://marcolepriph.altervista.org/portfolio/
```

Run only this project:

```powershell
dotnet test Tests\Portfolio\Portfolio.ProductionTests\Portfolio.ProductionTests.csproj --logger "console;verbosity=detailed"
```

The test reports the historical-cache baseline, clears all three Portfolio.Web caches, then verifies the entire discovered hierarchy once with cold cache and once with warm cache. Only the cold and warm phases determine the final pass/fail result.

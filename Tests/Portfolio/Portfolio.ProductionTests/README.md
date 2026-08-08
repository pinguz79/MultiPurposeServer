# Portfolio production tests

These tests exercise the deployed Portfolio.Api and Portfolio.Web instances. They are intentionally separated from Unit and Integration Tests because their result depends on network access, production data and external hosting.

All production tests are disabled by default. The layout smoke tests are read-only and verify representative deployed pages. The cache-regeneration scenario is separately enabled because it clears the rebuildable Portfolio.Web caches before checking cold and warm navigation.

## Read-only layout smoke tests

```text
PORTFOLIO_RUN_PRODUCTION_SMOKE_TESTS=true
```

The tests check the deployed home, a collection and a photo album for:

- the Iubenda CMP, TCF configuration, Privacy Policy and advertising-preferences control;
- exactly one 300x250 Altervista banner in the expected page context;
- the responsive photo-album ordering that places photos before advertising on tablet and mobile.

They do not require API keys and do not modify production data.

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

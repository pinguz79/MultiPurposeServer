# MultiPurposeServer (MPS)

Minimal .NET 10 Web API scaffold intended for deployment to Aruba .NET hosting.

Features:
- Health endpoint: GET /api/health
- Sample endpoints: GET /api/sample, GET /api/sample/echo?text=...
- Swagger (in Development)
- Swagger UI available at / when enabled (Development or EnableSwagger=true)
- CORS policy allowed (AllowAll) - adjust before production

To run locally:
1. dotnet restore
2. dotnet run --project MultiPurposeServer

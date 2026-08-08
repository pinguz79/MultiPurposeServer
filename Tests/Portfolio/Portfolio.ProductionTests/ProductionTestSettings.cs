namespace Portfolio.ProductionTests;

internal sealed record ProductionTestSettings(Uri ApiBaseUrl, Uri WebBaseUrl, string FrontEndApiKey, string BackEndApiKey)
{
    public static ProductionTestSettings Load()
    {
        return new ProductionTestSettings(
            ReadUri("PORTFOLIO_API_BASE_URL", "https://www.modelbook.cloud/Portfolio/"),
            ReadUri("PORTFOLIO_WEB_BASE_URL", "https://marcolepriph.altervista.org/portfolio/"),
            ReadRequired("PORTFOLIO_FRONTEND_API_KEY"),
            ReadRequired("PORTFOLIO_BACKEND_API_KEY"));
    }

    private static Uri ReadUri(string variableName, string defaultValue)
    {
        var value = (Environment.GetEnvironmentVariable(variableName) ?? defaultValue).TrimEnd('/') + '/';

        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri
            : throw new InvalidOperationException($"Environment variable {variableName} is not a valid absolute URI.");
    }

    private static string ReadRequired(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Environment variable {variableName} is required.");
    }
}

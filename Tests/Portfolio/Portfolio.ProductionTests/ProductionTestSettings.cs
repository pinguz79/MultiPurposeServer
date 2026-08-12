namespace Portfolio.ProductionTests
{
    internal sealed record ProductionTestSettings(Uri ApiBaseUrl, Uri WebBaseUrl, string FrontEndApiKey, string BackEndApiKey)
    {
        public static Uri LoadApiBaseUrl() => ReadUri("PORTFOLIO_API_BASE_URL", "https://www.modelbook.cloud/Portfolio/");

        public static Uri LoadApiDocumentationBaseUrl() => ReadUri("MPS_DOCUMENTATION_BASE_URL", "https://www.modelbook.cloud/");

        public static Uri LoadWebBaseUrl() => ReadUri("PORTFOLIO_WEB_BASE_URL", "https://marcolepriph.altervista.org/portfolio/");

        public static ProductionTestSettings Load()
        {
            return new ProductionTestSettings(
                LoadApiBaseUrl(),
                LoadWebBaseUrl(),
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
}

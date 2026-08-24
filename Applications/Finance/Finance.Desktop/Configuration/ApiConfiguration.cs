namespace Finance.Desktop.Configuration
{
    public sealed class ApiConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string HeaderName { get; set; } = "X-Finance-Api-Key";
        public string ApiKey { get; set; } = string.Empty;
    }
}

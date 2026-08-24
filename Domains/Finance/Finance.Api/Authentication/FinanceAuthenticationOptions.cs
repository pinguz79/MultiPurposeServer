namespace Finance.Api.Authentication
{
    public class FinanceAuthenticationOptions
    {
        public const string SectionName = "Authentication";
        public const string DefaultHeaderName = "X-Finance-Api-Key";

        public string HeaderName { get; set; } = DefaultHeaderName;
        public string DesktopKey { get; set; } = string.Empty;
    }
}

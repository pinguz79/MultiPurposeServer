namespace Portfolio.Api.Authentication
{
    public class PortfolioAuthenticationOptions
    {
        public const string SectionName = "PortfolioAuthentication";
        public const string DefaultHeaderName = "X-Portfolio-Api-Key";

        public string HeaderName { get; set; } = DefaultHeaderName;
        public string FrontEndKey { get; set; } = string.Empty;
        public string BackEndKey { get; set; } = string.Empty;
    }
}

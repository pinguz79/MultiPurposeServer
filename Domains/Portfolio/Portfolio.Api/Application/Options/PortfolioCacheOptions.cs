namespace Portfolio.Api.Application.Options;

public class PortfolioCacheOptions
{
    public const string SectionName = "PortfolioCache";

    public string BaseUrl { get; set; } = string.Empty;
    public string ClearEndpoint { get; set; } = "/portfolio/internal/cache/clear";
    public string SharedSecret { get; set; } = string.Empty;
}
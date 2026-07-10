namespace Portfolio.Api.Services.Options
{
    public class PortfolioMediaOptions
    {
        public string OriginalsRoot { get; set; } = string.Empty;
        public string CacheRoot { get; set; } = string.Empty;
        public int CoverWidth { get; set; } = 360;
        public int CoverHeight { get; set; } = 240;
    }
}
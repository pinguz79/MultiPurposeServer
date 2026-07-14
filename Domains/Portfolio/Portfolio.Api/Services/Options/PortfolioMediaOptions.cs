namespace Portfolio.Api.Services.Options
{
    public class PortfolioMediaOptions
    {
        public string OriginalsRoot { get; set; } = string.Empty;
        public string CacheRoot { get; set; } = string.Empty;
        public int CoverWidth { get; set; } = 360;
        public int CoverHeight { get; set; } = 240;
        public int ThumbnailWidth { get; set; } = 360;
        public int ThumbnailHeight { get; set; } = 240;
        public int ImageWidth { get; set; } = 800;
        public int ImageHeight { get; set; } = 1200;
    }
}
namespace Portfolio.Api.Application.Options
{
    public class PortfolioAlbumOptions
    {
        public const string SectionName = "Albums";

        public string RootPath { get; set; } = "Portfolio";

        public MissingPhotoStrategy MissingPhotoStrategy { get; set; } = MissingPhotoStrategy.KeepAndReport;

        public int MaxMissingPhotoDeletions { get; set; }

        public string SyncReportPath { get; set; } = "logs/health/portfolio-album-sync.json";
    }
}

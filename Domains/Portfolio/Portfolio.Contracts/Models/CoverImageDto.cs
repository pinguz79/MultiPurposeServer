using MultiPurposeServer.Shared.Utils;
using Portfolio.Constants;
using Portfolio.Data.Models;

namespace Portfolio.Contracts.Models
{
    public class CoverImageDto(Foto foto)
    {
        public string Url { get; set; } = $"{PortfolioUrls.CoverBasePath}/{foto.Id}";
        public string FileName { get; set; } = foto.FileName;
        public string Alt { get; set; } = !string.IsNullOrWhiteSpace(foto.Description) ? foto.Description : FileNameFormatter.FormatFileName(foto.FileName);
    }
}
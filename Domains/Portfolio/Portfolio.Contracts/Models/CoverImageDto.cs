using MultiPurposeServer.Shared.Utils;
using Portfolio.Constants;
using Portfolio.Data.Models;

namespace Portfolio.Contracts.Models
{
    public class CoverImageDto(Foto foto)
    {
        public string ThumbUrl { get; set; } = $"{PortfolioUrls.CoverBasePath}/{foto.Id}";
        public string Alt { get; set; } = !string.IsNullOrWhiteSpace(foto.Description) ? foto.Description : FileNameFormatter.FormatFileName(foto.FileName);
    }
}
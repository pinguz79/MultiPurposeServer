using Portfolio.Constants;
using Portfolio.Data.Models;

namespace Portfolio.Contracts.Responses
{
    public class CoverImageDto(Foto foto)
    {
        public string ThumbUrl { get; set; } = $"{PortfolioUrls.CoverBasePath}/{foto.Id}";
        public string Alt { get; set; } = foto.PhotoName;
    }
}
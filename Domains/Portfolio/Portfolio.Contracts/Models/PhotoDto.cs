using Portfolio.Constants;
using Portfolio.Data.Models;
using System.Diagnostics;

namespace Portfolio.Contracts
{
    [DebuggerDisplay("{FileName} - {Id}")]
    public class PhotoDto(Foto foto)
    {
        public Guid Id { get; set; } = foto.Id;
        public string Name { get; set; } = foto.PhotoName;
        public string Alt { get; set; } = foto.AltText;
        public string ThumbnailUrl { get; set; } = $"{PortfolioUrls.ThumbnailBasePath}/{foto.Id}";

        public string ImageUrl { get; set; } = $"{PortfolioUrls.ImageBasePath}/{foto.Id}";
        override public string ToString() => $"{foto.FileName} - {Id}";
    }
}

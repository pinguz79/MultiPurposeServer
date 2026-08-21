using System.Diagnostics;
using System.Text.Json.Serialization;

using Portfolio.Constants;
using Portfolio.DataModel.Enums;
using Portfolio.DataModel.Models;

namespace Portfolio.Contracts.Responses
{
    [DebuggerDisplay("{FileName} - {Id}")]
    public class PhotoDto(Foto foto)
    {
        public Guid Id { get; set; } = foto.Id;
        public string Name { get; set; } = foto.PhotoName;
        public string Alt { get; set; } = foto.AltText;
        public string ThumbnailUrl { get; set; } = $"{PortfolioUrls.ThumbnailBasePath}/{foto.Id}";

        public string ImageUrl { get; set; } = $"{PortfolioUrls.ImageBasePath}/{foto.Id}";
        public string? SelectionCode { get; set; } = foto.SelectionCode;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PhotoContentRating ContentRating { get; set; } = foto.ContentRating;
        override public string ToString() => $"{foto.FileName} - {Id}";
    }
}

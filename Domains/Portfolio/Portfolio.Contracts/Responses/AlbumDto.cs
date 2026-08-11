using Portfolio.Data.Enums;
using Portfolio.Data.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Portfolio.Contracts.Responses
{
    [DebuggerDisplay("{Name} ({Kind}, {Children} - {Photos})")]
    public class AlbumDto(Album album)
    {
        public Guid Id { get; set; } = album.Id;
        public string Name { get; set; } = album.Name;
        public string? Description { get; set; } = album.Description;
        public string? Path { get; set; } = album.Path;
        public string? FullPath { get; set; } = album.FullPath?.Replace('\\', '/');
        public Guid? ParentId { get; set; } = album.ParentId;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlbumKind Kind { get; set; } = album.Kind;
        public int Children { get; set; } = album.Children?.Count ?? 0;
        public int Photos { get; set; } = album.Photos?.Count ?? 0;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlbumContentRating ContentRating { get; set; } = album.ContentRating;
        public CoverImageDto? CoverImage { get; set; } = album.CoverImage is not null ? new CoverImageDto(album.CoverImage) : null;

        public override string ToString() => $"{Name} ({Kind}, {Children} - {Photos})";
    }
}

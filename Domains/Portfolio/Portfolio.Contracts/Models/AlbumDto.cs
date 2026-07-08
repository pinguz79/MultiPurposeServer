using Portfolio.Contracts.Models;
using Portfolio.Data.Models;
using System.Diagnostics;

namespace Portfolio.Contracts
{
    [DebuggerDisplay("{Name} ({Children} - {Photos})")]
    public class AlbumDto(Album album)
    {
        public Guid Id { get; set; } = album.Id;
        public string Name { get; set; } = album.Name;
        public string? Path { get; set; } = album.Path;
        public Guid? ParentId { get; set; } = album.ParentId;
        public int Children { get; set; } = album.Children.Count;
        public int Photos { get; set; } = album.Photos.Count;

        public CoverImageDto CoverImage { get; set; } = new CoverImageDto() { ThumbUrl = $"{album.CoverImage.Id}", Alt = album.CoverImage.Description }
        public override string ToString() => $"{Name} ({Children} - {Photos})";
    }
}

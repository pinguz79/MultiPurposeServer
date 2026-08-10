using Portfolio.Data.Models;

namespace Portfolio.Contracts.Bulk.Responses
{
    public sealed class AlbumMissingDescriptionsDto(Album album)
    {
        public Guid Id { get; init; } = album.Id;
        public string Name { get; init; } = album.Name;
        public string FullPath { get; init; } = (album.FullPath ?? album.Path ?? album.Name).Replace('\\', '/');
        public string Kind { get; init; } = album.Kind.ToString();
        public int Children { get; init; } = album.ChildrenCounter;
        public int Photos { get; init; } = album.PhotosCounter;
    }
}

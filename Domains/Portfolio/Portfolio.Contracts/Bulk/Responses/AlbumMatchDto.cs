using Portfolio.DataModel.Models;

namespace Portfolio.Contracts.Bulk.Responses
{
    public sealed class AlbumMatchDto(Album album)
    {
        public Guid Id { get; init; } = album.Id;
        public string Name { get; init; } = album.Name;
    }
}

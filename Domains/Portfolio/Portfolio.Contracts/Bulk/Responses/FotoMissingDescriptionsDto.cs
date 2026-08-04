using Portfolio.Data.Models;

namespace Portfolio.Contracts.Bulk.Responses
{
    public sealed class FotoMissingDescriptionsDto(Foto foto)
    {
        public Guid Id { get; init; } = foto.Id;
        public string FileName { get; init; } = foto.FileName;
        public string AlbumName { get; init; } = foto.Album.FullName;
        public string PhotoName { get; init; } = foto.PhotoName;
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

using Portfolio.Data.Enums;

using SystemPath = System.IO.Path;

namespace Portfolio.Data.Models
{
    [DebuggerDisplay("{Name} ({Kind}, {ChildrenCounter} - {PhotosCounter})")]
    public class Album : IEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string? Description { get; set; } = null;
        public virtual string? Path { get; set; } = null;

        public virtual Guid? ParentId { get; set; } = null;
        public virtual Album? Parent { get; set; } = null;
        [NotMapped] public string? FullPath => Parent is not null ? SystemPath.Combine(Parent.FullPath!, Path!) : Path!;
        [NotMapped] public string FullName => Parent is not null ? SystemPath.Combine(Parent.FullName, Name) : Name;

        public virtual ICollection<Album> Children { get; set; } = new List<Album>();
        [NotMapped] public int ChildrenCounter => Children.Count;
        [NotMapped] public AlbumKind Kind => ParentId is null ? AlbumKind.Gallery : Children.Count > 0 ? AlbumKind.Collection : AlbumKind.PhotoAlbum;

        public virtual ICollection<Foto> Photos { get; set; } = new List<Foto>();
        [NotMapped] public int PhotosCounter => Photos.Count;

        private IReadOnlyList<Foto>? _allPhotos;
        [NotMapped] public IReadOnlyList<Foto> AllPhotos => _allPhotos ??= [.. Photos, .. Children.SelectMany(child => child.AllPhotos)];

        private Foto? _coverImage;
        [NotMapped] public Foto? CoverImage => _coverImage ??= SelectCoverImage();

        [NotMapped]
        public AlbumContentRating ContentRating => Photos.Count > 0 ? Classify(Photos.Select(photo => photo.ContentRating == PhotoContentRating.Restricted))
                    : Classify(Children.Select(child => child.ContentRating == AlbumContentRating.Restricted));

        private Foto? SelectCoverImage()
        {
            if (AllPhotos.Count == 0)
            {
                return null;
            }

            var standardPhotos = AllPhotos.Where(photo => photo.ContentRating == PhotoContentRating.Standard).ToList();
            var candidates = standardPhotos.Count > 0 ? standardPhotos : AllPhotos;

            return candidates[Random.Shared.Next(candidates.Count)];
        }

        private static AlbumContentRating Classify(IEnumerable<bool> restrictedItems)
        {
            var items = restrictedItems.ToList();

            return items.Count == 0 || items.All(restricted => !restricted) ? AlbumContentRating.Standard
                : items.All(restricted => restricted) ? AlbumContentRating.Restricted : AlbumContentRating.PartiallyRestricted;
        }

        public override string ToString() => $"{Name} ({Kind}, {ChildrenCounter} - {PhotosCounter})";
    }
}

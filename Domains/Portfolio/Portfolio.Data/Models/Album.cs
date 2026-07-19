using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using SystemPath = System.IO.Path;

namespace Portfolio.Data.Models
{
    [DebuggerDisplay("{Name} ({ChildrenCounter} - {PhotosCounter})")]
    public class Album
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string? Description { get; set; } = null;
        public virtual string? Path { get; set; } = null;
        [NotMapped] public string? FullPath => Parent is not null ? SystemPath.Combine(Parent.FullPath!, Path!) : Path!;
        [NotMapped] public string? FullName => Parent is not null ? SystemPath.Combine(Parent.FullName!, Name) : Name;
        public virtual Guid? ParentId { get; set; } = null;
        public virtual Album? Parent { get; set; } = null;
        public virtual ICollection<Album> Children { get; set; } = new List<Album>();
        [NotMapped] public int ChildrenCounter => Children.Count;
        public virtual ICollection<Foto> Photos { get; set; } = new List<Foto>();
        [NotMapped] public int PhotosCounter => Photos.Count;
        private IReadOnlyList<Foto>? _allPhotos;

        [NotMapped] public IReadOnlyList<Foto> AllPhotos => _allPhotos ??= [.. Photos, .. Children.SelectMany(child => child.AllPhotos)];
        private Foto? _coverImage;

        [NotMapped] public Foto? CoverImage => _coverImage ??= AllPhotos.Count == 0 ? null : AllPhotos[Random.Shared.Next(AllPhotos.Count)];

        public override string ToString() => $"{Name} ({ChildrenCounter} - {PhotosCounter})";
    }
}

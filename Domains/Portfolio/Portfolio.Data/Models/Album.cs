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
        public virtual string? Path { get; set; } = null;
        public virtual string? FullPath => Parent is not null ? SystemPath.Combine(Parent.FullPath!, Path!) : Path!;
        public virtual Guid? ParentId { get; set; } = null;
        public virtual Album? Parent { get; set; } = null;
        public virtual ICollection<Album> Children { get; set; } = new List<Album>();
        public int ChildrenCounter => Children.Count;
        public virtual ICollection<Foto> Photos { get; set; } = new List<Foto>();
        public int PhotosCounter => Photos.Count;
        private IEnumerable<Foto> _allPhotos = null; 
        [NotMapped]
        public IEnumerable<Foto> AllPhotos => _allPhotos ??= Photos.Concat(Children.SelectMany(c => c.AllPhotos));
        private Foto? _coverImage = null;
        public Foto? CoverImage => _coverImage ??= AllPhotos.ElementAtOrDefault(Random.Shared.Next(AllPhotos.Count()));

        public override string ToString() => $"{Name} ({ChildrenCounter} - {PhotosCounter})";
    }
}

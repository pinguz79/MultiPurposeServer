using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Portfolio.Data.Models
{
    [DebuggerDisplay("{Name} ({ChildrenCounter} - {PhotosCounter})")]
    public class Album
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string? Path { get; set; } = null;
        public virtual string? FullPath => Path is not null ? System.IO.Path.Combine(Path, Name) : Name;
        public virtual Guid? ParentId { get; set; } = null;
        public virtual Album? Parent { get; set; } = null;
        public virtual ICollection<Album> Children { get; set; } = new List<Album>();
        public int ChildrenCounter => Children.Count;
        public virtual ICollection<Foto> Photos { get; set; } = new List<Foto>();
        public int PhotosCounter => Photos.Count;
        public override string ToString() => $"{Name} ({ChildrenCounter} - {PhotosCounter})";
    }
}

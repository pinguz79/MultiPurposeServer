using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MultiPurposeServer.Models.Portfolio
{
    [DebuggerDisplay("{Name} ({ChildrenCounter} - {PhotosCounter})")]
    public class Album
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string? Path { get; set; } = null;
        public virtual Guid? ParentId { get; set; } = null;
        [JsonIgnore]
        public virtual Album? Parent { get; set; } = null;
        [JsonIgnore]
        public virtual ICollection<Album> Children { get; set; } = [];
        public int ChildrenCounter => Children.Count;
        [JsonIgnore]
        public virtual ICollection<Foto> Photos { get; set; } = [];
        public int PhotosCounter => Photos.Count;
        public override string ToString() => $"{Name} ({ChildrenCounter} - {PhotosCounter})";
    }
}

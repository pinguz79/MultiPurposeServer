using System.Diagnostics;

namespace Portfolio.Data.Models
{
    [DebuggerDisplay("{FileName} - {AlbumName}")]
    public class Foto
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; } = string.Empty;
        public virtual Guid AlbumId { get; set; }
        public virtual string AlbumName => Album?.Name ?? string.Empty;
        public virtual Album? Album { get; set; } = null;
        public virtual string Description { get; set; } = string.Empty;
        public virtual string PhotoName => !string.IsNullOrWhiteSpace(Description) ? Description : FileName;
        public override string ToString() => $"{FileName} - {AlbumName}";
    }
}

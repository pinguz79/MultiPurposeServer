using MultiPurposeServer.Shared.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Portfolio.Data.Models
{
    [DebuggerDisplay("{FileName} - {AlbumName}")]
    public class Foto : IEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; } = string.Empty;
        public virtual Guid AlbumId { get; set; }
        public virtual string AlbumName => Album?.Name ?? string.Empty;
        public virtual Album? Album { get; set; } = null;
        public virtual string? Description { get; set; } = null;
        [NotMapped] public virtual string RelativePath => Path.Combine(Album!.FullPath!, FileName);
        [NotMapped] public string PhotoName => !string.IsNullOrWhiteSpace(Description) ? Description : new FileNameFormatter(FileName).HumanizedName;
        [NotMapped] public string AltText => PhotoName;
        [NotMapped] public string? SelectionCode => new NamingConventions(FileName).SelectionCode;

        public override string ToString() => $"{FileName} - {AlbumName}";
    }
}

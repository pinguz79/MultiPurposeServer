using MultiPurposeServer.Shared.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations.Schema;
using SystemPath = System.IO.Path;

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
        [NotMapped] public virtual string RelativePath => Path.Combine(Album!.FullPath!, FileName);
        [NotMapped] public string PhotoName => !string.IsNullOrWhiteSpace(Description) ? Description : FileNameFormatter.FormatFileName(FileName);
        [NotMapped] public string AltText => PhotoName;
        [NotMapped] public string? SelectionCode => new NamingConventions(FileName).SelectionCode;

        public override string ToString() => $"{FileName} - {AlbumName}";
    }
}

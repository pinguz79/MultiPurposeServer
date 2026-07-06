using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MultiPurposeServer.Models.Portfolio
{
    [DebuggerDisplay("{FileName} ({Album.Name})")]
    public class Foto
    {
        public virtual Guid Id { get; set; }
        public virtual Guid AlbumId { get; set; }
        [JsonIgnore]
        public virtual Album Album { get; set; }
        public virtual string FileName { get; set; } = string.Empty;
        public override string ToString() => $"{FileName} ({Album.Name})";
    }
}

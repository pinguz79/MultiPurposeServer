using Portfolio.Data.Models;
using System.Diagnostics;

namespace Portfolio.Contracts
{
    [DebuggerDisplay("{FileName} - {Id}")]
    public class FotoDto(Foto foto)
    {
        public Guid Id { get; set; } = foto.Id;
        public string FileName { get; set; } = foto.FileName;
        public Guid AlbumId { get; set; } = foto.AlbumId;
        override public string ToString() => $"{FileName} - {Id}";
    }
}

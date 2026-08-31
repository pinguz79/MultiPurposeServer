namespace Finance.Desktop.Models
{
    internal sealed record VoceRicorrenteCategoriaOption(string? Name, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}

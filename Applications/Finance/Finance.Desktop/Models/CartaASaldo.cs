namespace Finance.Desktop.Models
{
    public sealed record CartaASaldo(
        string ContoName,
        Guid PianificazioneAddebitoId,
        Guid PianificazioneRipristinoId,
        bool Created);
}

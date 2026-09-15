namespace Finance.Desktop.Models
{
    public sealed record CartaRevolving(string ContoName, Guid PianificazioneInteressiId, Guid PianificazioneBolloId, Guid PianificazioneRimborsoId, Guid PianificazioneAddebitoId, bool Created);
}

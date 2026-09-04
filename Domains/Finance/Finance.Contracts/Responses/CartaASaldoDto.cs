namespace Finance.Contracts.Responses
{
    public sealed record CartaASaldoDto(
        string ContoName,
        Guid PianificazioneAddebitoId,
        Guid PianificazioneRipristinoId,
        bool Created);
}

namespace Finance.Contracts.Responses
{
    public class CartaRevolvingDto(string contoName, Guid interessiId, Guid bolloId, Guid rimborsoId, Guid addebitoId, bool created)
    {
        public string ContoName { get; set; } = contoName;
        public Guid PianificazioneInteressiId { get; set; } = interessiId;
        public Guid PianificazioneBolloId { get; set; } = bolloId;
        public Guid PianificazioneRimborsoId { get; set; } = rimborsoId;
        public Guid PianificazioneAddebitoId { get; set; } = addebitoId;
        public bool Created { get; set; } = created;
    }
}

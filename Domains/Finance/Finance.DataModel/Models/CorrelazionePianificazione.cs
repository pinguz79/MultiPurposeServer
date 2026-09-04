namespace Finance.DataModel.Models
{
    public class CorrelazionePianificazione
    {
        public virtual Guid Id { get; set; }

        public virtual Guid PianificazioneAId { get; set; }
        public virtual Pianificazione PianificazioneA { get; set; } = null!;

        public virtual Guid PianificazioneBId { get; set; }
        public virtual Pianificazione PianificazioneB { get; set; } = null!;
    }
}

using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class PianificazioneDto(Pianificazione pianificazione)
    {
        public Guid Id { get; set; } = pianificazione.Id;
        public string ContoName { get; set; } = pianificazione.Conto.Name;
        public string Description { get; set; } = pianificazione.Description;
        public string Formula { get; set; } = pianificazione.MovimentoFormula;
        public DateOnly ValidFrom { get; set; } = pianificazione.ValidFrom;
        public DateOnly ValidTo { get; set; } = pianificazione.ValidTo;
        public FrequenzaPeriodicita Frequency { get; set; } = pianificazione.Periodicita.Frequenza;
        public int Interval { get; set; } = pianificazione.Periodicita.Intervallo;
        public int? DayOfMonth { get; set; } = pianificazione.Periodicita.GiornoMese;
        public DayOfWeek? DayOfWeek { get; set; } = pianificazione.Periodicita.GiornoSettimana;
        public bool EndOfMonth { get; set; } = pianificazione.Periodicita.FineMese;
        public IReadOnlyList<Guid> MovimentoIds { get; set; } = pianificazione.Movimenti.Select(item => item.Id).ToList();
    }
}

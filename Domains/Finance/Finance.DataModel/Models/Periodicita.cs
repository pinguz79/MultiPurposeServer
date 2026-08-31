namespace Finance.DataModel.Models
{
    public class Periodicita
    {
        public virtual Guid Id { get; set; }
        public virtual FrequenzaPeriodicita Frequenza { get; set; }
        public virtual int Intervallo { get; set; }
        public virtual DayOfWeek? GiornoSettimana { get; set; }
        public virtual int? SettimanaMese { get; set; }
        public virtual int? GiornoMese { get; set; }
        public virtual int? MeseAnno { get; set; }
        public virtual bool FineMese { get; set; }

        public virtual ICollection<Pianificazione> Pianificazioni { get; set; } = [];
    }
}

namespace Finance.DataModel.Models
{
    public class Pianificazione
    {
        public virtual Guid Id { get; set; }
        public virtual string Description { get; set; } = string.Empty;
        public virtual string MovimentoFormula { get; set; } = string.Empty;
        public virtual string MovimentoDescription { get; set; } = string.Empty;
        public virtual ModalitaCategoria ModalitaCategoria { get; set; }
        public virtual string? VoceRicorrenteCategoriaName { get; set; }
        public virtual DateOnly ValidFrom { get; set; }
        public virtual DateOnly ValidTo { get; set; }

        public virtual Guid? CategoriaId { get; set; }
        public virtual Categoria? Categoria { get; set; }

        public virtual Guid PeriodicitaId { get; set; }
        public virtual Periodicita Periodicita { get; set; } = null!;

        public virtual Guid ContoId { get; set; }
        public virtual Conto Conto { get; set; } = null!;

        public virtual ICollection<Movimento> Movimenti { get; set; } = [];
    }
}

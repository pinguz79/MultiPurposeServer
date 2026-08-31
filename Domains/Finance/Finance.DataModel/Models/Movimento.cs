namespace Finance.DataModel.Models
{
    public class Movimento
    {
        public virtual Guid Id { get; set; }

        public virtual Guid ContoId { get; set; }
        public virtual Conto Conto { get; set; } = null!;

        public virtual DateOnly Date { get; set; }
        public virtual string Description { get; set; } = string.Empty;
        public virtual string Formula { get; set; } = string.Empty;

        public virtual Guid? CategoriaId { get; set; }
        public virtual Categoria? Categoria { get; set; }

        public virtual Guid? PianificazioneId { get; set; }
        public virtual Pianificazione? Pianificazione { get; set; }
    }
}

namespace Finance.DataModel.Models
{
    public class Pedaggio
    {
        public virtual Guid Id { get; set; }

        public virtual Guid MovimentoId { get; set; }
        public virtual Movimento Movimento { get; set; } = null!;
        public virtual Guid CaselloEntrataId { get; set; }
        public virtual Casello CaselloEntrata { get; set; } = null!;
        public virtual Guid CaselloUscitaId { get; set; }
        public virtual Casello CaselloUscita { get; set; } = null!;
    }
}

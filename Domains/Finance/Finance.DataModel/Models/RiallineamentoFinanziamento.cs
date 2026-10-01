namespace Finance.DataModel.Models
{
    public class RiallineamentoFinanziamento
    {
        public virtual Guid Id { get; set; }
        public virtual int InstallmentNumber { get; set; }
        public virtual decimal Principal { get; set; }

        public virtual Guid FinanziamentoId { get; set; }
        public virtual Finanziamento Finanziamento { get; set; } = null!;
    }
}

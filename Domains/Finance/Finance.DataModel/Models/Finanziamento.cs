namespace Finance.DataModel.Models
{
    public class Finanziamento
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual string Lender { get; set; } = string.Empty;
        public virtual decimal InitialPrincipal { get; set; }
        public virtual decimal Tan { get; set; }
        public virtual decimal Installment { get; set; }
        public virtual decimal Insurance { get; set; }
        public virtual decimal Fees { get; set; }
        public virtual DateOnly FirstDueDate { get; set; }
        public virtual int DueDay { get; set; }
        public virtual int InstallmentCount { get; set; }
        public virtual bool IsClosed { get; set; }

        public virtual ICollection<RiallineamentoFinanziamento> Riallineamenti { get; set; } = [];
    }
}

namespace Finance.DataModel.Models
{
    public class TariffaTratta
    {
        public virtual Guid Id { get; set; }
        public virtual string Formula { get; set; } = "0.00";
        public virtual DateOnly? ValidFrom { get; set; }
        public virtual DateOnly? ValidTo { get; set; }
        public virtual int Index { get; set; }

        public virtual Guid CaselloAId { get; set; }
        public virtual Casello CaselloA { get; set; } = null!;
        public virtual Guid CaselloBId { get; set; }
        public virtual Casello CaselloB { get; set; } = null!;
    }
}

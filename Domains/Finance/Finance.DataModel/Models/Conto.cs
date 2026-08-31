namespace Finance.DataModel.Models
{
    public class Conto
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual decimal InitialBalance { get; set; }

        public virtual ICollection<Movimento> Movimenti { get; set; } = [];

    }
}

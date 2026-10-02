namespace Finance.DataModel.Models
{
    public class GruppoMovimenti
    {
        public virtual Guid Id { get; set; }
        public virtual ICollection<Movimento> Movimenti { get; set; } = [];
    }
}

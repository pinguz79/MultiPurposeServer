namespace Finance.DataModel.Models
{
    public class Categoria
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;

        public virtual ICollection<Movimento> Movimenti { get; set; } = [];
        public virtual ICollection<Pianificazione> Pianificazioni { get; set; } = [];
        public virtual ICollection<VoceRicorrente> VociRicorrenti { get; set; } = [];
    }
}

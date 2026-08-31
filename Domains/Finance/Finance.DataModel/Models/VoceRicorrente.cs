namespace Finance.DataModel.Models
{
    public class VoceRicorrente
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual decimal Value { get; set; }
        public virtual DateOnly? ValidFrom { get; set; }
        public virtual DateOnly? ValidTo { get; set; }
        public virtual int Index { get; set; }

        public virtual Guid? CategoriaId { get; set; }
        public virtual Categoria? Categoria { get; set; }
    }
}

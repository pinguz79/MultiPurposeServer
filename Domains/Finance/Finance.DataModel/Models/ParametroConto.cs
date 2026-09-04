namespace Finance.DataModel.Models
{
    public class ParametroConto
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual TipoParametroConto Type { get; set; }
        public virtual decimal Value { get; set; }
        public virtual DateOnly? ValidFrom { get; set; }
        public virtual DateOnly? ValidTo { get; set; }
        public virtual int Index { get; set; }

        public virtual Guid ContoId { get; set; }
        public virtual Conto Conto { get; set; } = null!;
    }
}

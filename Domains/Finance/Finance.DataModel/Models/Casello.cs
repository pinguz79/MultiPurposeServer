using System.ComponentModel.DataAnnotations.Schema;

namespace Finance.DataModel.Models
{
    public class Casello
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;

        public virtual ICollection<TariffaTratta> TariffeComeA { get; set; } = [];
        public virtual ICollection<TariffaTratta> TariffeComeB { get; set; } = [];

        [NotMapped] public IEnumerable<TariffaTratta> Tariffe => TariffeComeA.Union(TariffeComeB);
    }
}

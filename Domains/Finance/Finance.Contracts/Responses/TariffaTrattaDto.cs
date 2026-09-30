using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class TariffaTrattaDto(TariffaTratta tariffa)
    {
        public Guid Id { get; set; } = tariffa.Id;
        public Guid CaselloAId { get; set; } = tariffa.CaselloAId;
        public Guid CaselloBId { get; set; } = tariffa.CaselloBId;
        public string Formula { get; set; } = tariffa.Formula;
        public DateOnly? ValidFrom { get; set; } = tariffa.ValidFrom;
        public DateOnly? ValidTo { get; set; } = tariffa.ValidTo;
        public int Index { get; set; } = tariffa.Index;
    }
}

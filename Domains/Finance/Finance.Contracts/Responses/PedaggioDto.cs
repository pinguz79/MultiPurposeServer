using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class PedaggioDto(Pedaggio pedaggio)
    {
        public Guid MovimentoId { get; set; } = pedaggio.MovimentoId;
        public Guid EntrataId { get; set; } = pedaggio.CaselloEntrataId;
        public Guid UscitaId { get; set; } = pedaggio.CaselloUscitaId;
    }
}

using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public sealed record PianificazioneCartaRevolvingConfiguration(Guid ContoId, string Description, string MovimentoDescription, string Formula, NaturaMovimento Natura, int Day);
}

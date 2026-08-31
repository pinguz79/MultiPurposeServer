using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record CreatePianificazioneRequest(
        [property: Normalize, Required] string ContoName,
        [property: Normalize, Required] string Description,
        [property: Normalize, Required] string MovimentoDescription,
        [property: Required] string Formula,
        DateOnly ValidFrom,
        DateOnly ValidTo,
        int Interval,
        int? DayOfMonth,
        bool EndOfMonth) : IRequest;
}

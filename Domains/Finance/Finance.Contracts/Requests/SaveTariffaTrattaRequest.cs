using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record SaveTariffaTrattaRequest([property: NormalizeChildren, Required] IReadOnlyList<TariffaTrattaDefinitionRequest> Definitions) : IRequest;
}

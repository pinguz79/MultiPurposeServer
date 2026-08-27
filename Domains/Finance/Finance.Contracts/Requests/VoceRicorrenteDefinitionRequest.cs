using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record VoceRicorrenteDefinitionRequest(
        Guid? Id,
        [property: Normalize, Required] string DisplayName,
        decimal Value,
        DateOnly? ValidFrom,
        DateOnly? ValidTo) : IRequest;
}

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record ConfirmMovimentiRequest([property: Required] IReadOnlyList<Guid> Ids) : IRequest;
}

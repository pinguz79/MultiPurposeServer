using MultiPurposeServer.Shared.Contracts.Abstractions;

namespace Finance.Contracts.Requests
{
    public sealed record DeleteGruppoMovimentiRequest(IReadOnlyList<Guid> Ids) : IRequest;
}

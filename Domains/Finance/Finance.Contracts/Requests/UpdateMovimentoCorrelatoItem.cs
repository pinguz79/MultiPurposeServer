using MultiPurposeServer.Shared.Contracts.Abstractions;

namespace Finance.Contracts.Requests
{
    public sealed record UpdateMovimentoCorrelatoItem(Guid Id, UpdateMovimentoRequest Changes) : IRequest;
}

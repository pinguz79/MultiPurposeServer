using MultiPurposeServer.Shared.Contracts.Abstractions;

namespace Finance.Contracts.Requests
{
    public sealed record SaveRiallineamentoRequest(decimal Principal) : IRequest;
}

using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record SaveCaselloRequest([property: Normalize, Required] string Name) : IRequest;
}

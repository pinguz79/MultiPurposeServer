using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Finance.Contracts.Requests
{
    public sealed record UpdateMovimentoRequest(
        [property: RequiredAtLeastOne] DateOnly? Date,
        [property: Normalize, RequiredAtLeastOne] string? Description,
        [property: Normalize, RequiredAtLeastOne] string? Formula,
        [property: Normalize, RequiredAtLeastOne] string? CategoryName,
        [property: RequiredAtLeastOne] bool? ClearCategory) : IRequest;
}
